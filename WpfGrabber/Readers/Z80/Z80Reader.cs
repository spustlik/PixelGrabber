using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace WpfGrabber.Readers.Z80
{
    //http://www.z80.info/decoding.htm
    public class Z80Reader
    {
        public byte[] Data { get; private set; }
        public int Addr { get; private set; }
        private int _lastInstrAddr = 0;
        public Z80Reader(byte[] data, int start = 0)
        {
            Data = data;
            Addr = start;
            _lastInstrAddr = start;
        }

        public byte ReadByte()
        {
            return Data[Addr++];
        }
        public int ReadSignedByte()
        {
            var b = ReadByte();
            if (b < 128)
                return b;
            //TODO:check
            return b - 256;
        }
        private int ReadUnsigned16()
        {
            var b1 = ReadByte();
            var b2 = ReadByte();
            return b2 << 8 | b1;
        }

        public IEnumerable<Z80Instruction> ReadInstructions()
        {
            while (true)
            {
                yield return ReadInstruction();
                if (Addr >= Data.Length)
                    break;
            }
        }
        class Context
        {

        }
        public struct Opcode
        {
            public int x { get; set; }
            public int y { get; set; }
            public int z { get; set; }
            public int p { get; set; }
            public int q { get; set; }
            public byte Op { get; set; }
            public Opcode(byte opcode)
            {
                Op = opcode;
                x = (opcode & 0b11000000) >> 6;
                y = (opcode & 0b00111000) >> 3;
                z = (opcode & 0b00000111);
                p = y >> 1;
                q = y % 2;
                if (y < 0 || y > 7)
                    throw new InvalidOperationException();
            }
        }

        private Opcode ReadOpCode()
        {
            return new Opcode(ReadByte());
        }


        public Z80Instruction ReadInstruction()
        {
            var ctx = new Context();
            return ReadInstr(ctx);
        }

        private Z80Instruction ReadInstr(Context ctx)
        {
            var opcode = ReadOpCode();
            if (opcode.Op == 0xCB) // b11001011
                return ReadCB();
            if (opcode.Op == 0xED)
                return ReadED();
            if (opcode.Op == 0xDD)
                return ReadDDFD(opcode, Z80Register.IX);
            if (opcode.Op == 0xFD)
                return ReadDDFD(opcode, Z80Register.IY);

            switch (opcode.x)
            {
                case 0: return Read0(opcode);
                case 1: return Read1(opcode);
                case 2: return Read2(opcode);
                case 3: return Read3(opcode);
                default: return ReadUnknown(opcode);
            }
        }

        private Z80Instruction ReadDDFD(Opcode opcode, Z80Register reg)
        {
            //this need to refactor Read0-3 and call it with replacement of HL,H,L, not (HL) registers as IX,IXH,IXL
            // (HL) is replaced by (IX+d) d=ReadSignedByte()
            // and more (read)
            return CreateInstr(Z80Op.Unknown, opcode.Op.ToString("X2"), reg);
        }

        private Z80Instruction ReadED()
        {
            var op = ReadOpCode();
            if (op.x == 0 || op.x == 3)
                return CreateInstr(Z80Op.NoniNOP);
            switch (op.x)
            {
                case 1: return ReadED1(op);
                case 2:
                    if (op.y < 4 || op.z > 3)
                        return CreateInstr(Z80Op.NoniNOP);
                    return CreateInstr(GetBliOp(op.y, op.z));
                default: throw new InvalidOperationException();
            }
        }

        private Z80Instruction ReadED1(Opcode opcode)
        {
            switch (opcode.z)
            {
                case 0:
                    switch (opcode.y)
                    {
                        case 6: return CreateInstr(Z80Op.IN, GetInOutRegisterC());
                        default: return CreateInstr(Z80Op.IN, GetRegister8(opcode.y), GetInOutRegisterC());
                    }
                case 1:
                    switch (opcode.y)
                    {
                        case 6: return CreateInstr(Z80Op.OUT, GetInOutRegisterC());
                        default: return CreateInstr(Z80Op.OUT, GetRegister8(opcode.y), GetInOutRegisterC());
                    }
                case 2:
                    switch (opcode.q)
                    {
                        case 0: return CreateInstr(Z80Op.SBC, Z80Register.HL, GetRegister16Pair(opcode.p));
                        case 1: return CreateInstr(Z80Op.ADC, Z80Register.HL, GetRegister16Pair(opcode.p));
                        default: throw new InvalidOperationException();
                    }
                case 3:
                    switch (opcode.q)
                    {
                        case 0: return CreateInstr(Z80Op.LD, ReadIndirectAddr16(), GetRegister16Pair(opcode.p));
                        case 1: return CreateInstr(Z80Op.LD, GetRegister16Pair(opcode.p), ReadIndirectAddr16());
                        default: throw new InvalidOperationException();
                    }
                case 4: return CreateInstr(Z80Op.NEG);
                case 5:
                    switch (opcode.y)
                    {
                        case 1: return CreateInstr(Z80Op.RETI);
                        default: return CreateInstr(Z80Op.RETN);
                    }
                case 6: return CreateInstr(Z80Op.IM, GetIMCode(opcode.y));
                case 7:
                    switch (opcode.y)
                    {
                        case 0: return CreateInstr(Z80Op.LD, Z80Param8BitRegister.From(Z80Register.I), Z80Register.A);
                        case 1: return CreateInstr(Z80Op.LD, Z80Param8BitRegister.From(Z80Register.R), Z80Register.A);
                        case 2: return CreateInstr(Z80Op.LD, Z80Register.A, Z80Param8BitRegister.From(Z80Register.I));
                        case 3: return CreateInstr(Z80Op.LD, Z80Register.A, Z80Param8BitRegister.From(Z80Register.R));
                        case 4: return CreateInstr(Z80Op.RRD);
                        case 5: return CreateInstr(Z80Op.RLD);
                        case 6: return CreateInstr(Z80Op.NOP);
                        case 7: return CreateInstr(Z80Op.NOP);
                        default: throw new InvalidOperationException();
                    }
                default: throw new InvalidOperationException();
            }
        }

        private Z80Instruction ReadCB()
        {
            var op = ReadOpCode();
            switch (op.x)
            {
                case 0: return CreateInstr(GetRotationOp(op.y), GetRegister8(op.z));
                case 1: return CreateInstr(Z80Op.BIT, new Z80ParamLiteral(op.y, 1), GetRegister8(op.z));
                case 2: return CreateInstr(Z80Op.RES, new Z80ParamLiteral(op.y, 1), GetRegister8(op.z));
                case 3: return CreateInstr(Z80Op.SET, new Z80ParamLiteral(op.y, 1), GetRegister8(op.z));
                default: throw new InvalidOperationException();
            }
        }
        private Z80Instruction Read3(Opcode opcode)
        {
            switch (opcode.z)
            {
                case 0: return CreateInstr(Z80Op.RET, GetCC(opcode.y));
                case 1:
                    switch (opcode.q)
                    {
                        case 0: return CreateInstr(Z80Op.POP, GetRegister16Pair_2(opcode.p));
                        case 1:
                            switch (opcode.p)
                            {
                                case 0: return CreateInstr(Z80Op.RET);
                                case 1: return CreateInstr(Z80Op.EXX);
                                case 2: return CreateInstr(Z80Op.JP, Z80Register.HL);
                                case 3: return CreateInstr(Z80Op.LD, Z80Register.SP, Z80Register.HL);
                                default: throw new InvalidOperationException();
                            }
                        default: throw new InvalidOperationException();
                    }
                case 2: return CreateInstr(Z80Op.JP, GetCC(opcode.y), GetAddrLabel(ReadUnsigned16()));
                case 3:
                    switch (opcode.y)
                    {
                        case 0: return CreateInstr(Z80Op.JP, GetAddrLabel(ReadUnsigned16()));
                        //case 1: CB prefix
                        case 2: return CreateInstr(Z80Op.OUT, ReadInOutPort(), Z80Register.A);
                        case 3: return CreateInstr(Z80Op.IN, Z80Register.A, ReadInOutPort());
                        case 4: return CreateInstr(Z80Op.EX, Z80Register.SP, Z80Param8BitRegister.From(Z80Register.HL));
                        case 5: return CreateInstr(Z80Op.EX, Z80Register.DE, Z80Register.HL);
                        case 6: return CreateInstr(Z80Op.DI);
                        case 7: return CreateInstr(Z80Op.EI);
                        default: throw new InvalidOperationException();
                    }
                case 4:
                    return CreateInstr(Z80Op.CALL, GetCC(opcode.y), GetAddrLabel(ReadUnsigned16()));
                case 5:
                    switch (opcode.q)
                    {
                        case 0: return CreateInstr(Z80Op.PUSH, GetRegister16Pair_2(opcode.p));
                        case 1:
                            switch (opcode.p)
                            {
                                case 0: return CreateInstr(Z80Op.CALL, GetAddrLabel(ReadUnsigned16()));
                                //case 1: //dd prefix
                                //case 2: //ed prefix
                                //case 3: //fd prefix
                                default: throw new InvalidOperationException();
                            }
                        default: throw new InvalidOperationException();
                    }
                case 6: return ReadAlu(opcode.y, new Z80ParamLiteral(ReadByte(), 2));
                case 7: return CreateInstr(Z80Op.RST, new Z80ParamLiteral(opcode.y * 8, 2));
                default: throw new InvalidOperationException();
            }
        }

        private Z80Instruction Read2(Opcode opcode)
        {
            return ReadAlu(opcode.y, GetRegister8(opcode.z));
        }

        private Z80Instruction ReadAlu(int alu, Z80Param p)
        {
            switch (alu)
            {
                case 0: return CreateInstr(Z80Op.ADD, Z80Register.A, p);
                case 1: return CreateInstr(Z80Op.ADC, Z80Register.A, p);
                case 2: return CreateInstr(Z80Op.SUB, Z80Register.A, p);
                case 3: return CreateInstr(Z80Op.SBC, Z80Register.A, p);
                case 4: return CreateInstr(Z80Op.AND, p);
                case 5: return CreateInstr(Z80Op.XOR, p);
                case 6: return CreateInstr(Z80Op.OR, p);
                case 7: return CreateInstr(Z80Op.CP, p);
                default: throw new InvalidOperationException();
            }
        }

        private Z80Instruction Read1(Opcode opcode)
        {
            if (opcode.y == 6 && opcode.z == 6)
                return CreateInstr(Z80Op.HALT);
            return CreateInstr(Z80Op.LD, GetRegister8(opcode.y), GetRegister8(opcode.z));
        }

        private Z80Instruction Read0(Opcode opcode)
        {
            switch (opcode.z)
            {
                case 0:
                    switch (opcode.y)
                    {
                        case 0: return CreateInstr(Z80Op.NOP);
                        case 1: return CreateInstr(Z80Op.EX, Z80Register.AF, Z80Register.AF_EX);
                        case 2: return CreateInstr(Z80Op.DJNZ, GetAddrLabel(Addr + ReadSignedByte() + 1));
                        case 3: return CreateInstr(Z80Op.JR, GetAddrLabel(Addr + ReadSignedByte() + 1));
                        //4-7
                        default: return CreateInstr(Z80Op.JR, GetCC(opcode.y - 4), GetAddrLabel(Addr + ReadSignedByte() + 1));
                    }
                case 1:
                    switch (opcode.q)
                    {
                        case 0: return CreateInstr(Z80Op.LD, GetRegister16Pair(opcode.p), new Z80ParamLiteral(ReadUnsigned16(), 4));
                        case 1: return CreateInstr(Z80Op.ADD, Z80Register.HL, GetRegister16Pair(opcode.p));
                        default: throw new InvalidOperationException();
                    }
                case 2:
                    switch (opcode.q)
                    {
                        case 0:
                            switch (opcode.p)
                            {
                                case 0: return CreateInstr(Z80Op.LD, "(BC)", Z80Register.A);
                                case 1: return CreateInstr(Z80Op.LD, "(DE)", Z80Register.A);
                                case 2: return CreateInstr(Z80Op.LD, ReadIndirectAddr16(), Z80Register.HL);
                                case 3: return CreateInstr(Z80Op.LD, ReadIndirectAddr16(), Z80Register.A);
                                default: throw new InvalidOperationException();
                            }
                        case 1:
                            switch (opcode.p)
                            {
                                case 0: return CreateInstr(Z80Op.LD, Z80Register.A, "(BC)");
                                case 1: return CreateInstr(Z80Op.LD, Z80Register.A, "(DE)");
                                case 2: return CreateInstr(Z80Op.LD, Z80Register.HL, ReadIndirectAddr16());
                                case 3: return CreateInstr(Z80Op.LD, Z80Register.A, ReadIndirectAddr16());
                                default: throw new InvalidOperationException();
                            }
                        default: throw new InvalidOperationException();
                    }
                case 3:
                    switch (opcode.q)
                    {
                        case 0: return CreateInstr(Z80Op.INC, GetRegister16Pair(opcode.p));
                        case 1: return CreateInstr(Z80Op.DEC, GetRegister16Pair(opcode.p));
                        default: throw new InvalidOperationException();
                    }
                case 4:
                    return CreateInstr(Z80Op.INC, GetRegister8(opcode.y));
                case 5:
                    return CreateInstr(Z80Op.DEC, GetRegister8(opcode.y));
                case 6:
                    return CreateInstr(Z80Op.LD, GetRegister8(opcode.y), new Z80ParamLiteral(ReadByte(), 2));
                case 7:
                    switch (opcode.y)
                    {
                        case 0: return CreateInstr(Z80Op.RLCA);
                        case 1: return CreateInstr(Z80Op.RRCA);
                        case 2: return CreateInstr(Z80Op.RLA);
                        case 3: return CreateInstr(Z80Op.RRA);
                        case 4: return CreateInstr(Z80Op.DAA);
                        case 5: return CreateInstr(Z80Op.CPL);
                        case 6: return CreateInstr(Z80Op.SCF);
                        case 7: return CreateInstr(Z80Op.CCF);
                        default: throw new InvalidOperationException();
                    }
                default: throw new InvalidOperationException();
            }
        }

        private Z80Op GetRotationOp(int i)
        {
            return new Z80Op[] { Z80Op.RLC, Z80Op.RRC, Z80Op.RL, Z80Op.RR, Z80Op.SLA, Z80Op.SRA, Z80Op.SLL, Z80Op.SRL }[i];
        }
        private Z80Op GetBliOp(int y, int z)
        {
            /* 0 1 2 3
            a=4	LDI	CPI	INI	OUTI
            a=5	LDD	CPD	IND	OUTD
            a=6	LDIR	CPIR	INIR	OTIR
            a=7	LDDR	CPDR	INDR	OTDR
            */
            var bli = new Z80Op[4, 4]
            {
                { Z80Op.LDI,Z80Op.CPI,Z80Op.INI,Z80Op.OUTI },
                { Z80Op.LDD,Z80Op.CPD,Z80Op.IND,Z80Op.OUTD },
                { Z80Op.LDIR,Z80Op.CPIR,Z80Op.INIR,Z80Op.OTIR },
                { Z80Op.LDDR,Z80Op.CPDR,Z80Op.INDR,Z80Op.OTDR }
            };
            return bli[y - 4, z];
        }
        private string GetIMCode(int i)
        {
            return new[] { "0", "0/1", "1", "2", "0", "0/1", "1", "2" }[i];
        }
        private string ReadIndirectAddr16()
        {
            return $"(0x{ReadUnsigned16():X4})";
        }
        private static string GetInOutRegisterC()
        {
            return "(C)";
        }
        private string ReadInOutPort()
        {
            return $"({ReadByte()})";
        }
        private string GetAddrLabel(int addr)
        {
            return "L" + addr.ToString("X4");
        }

        private Z80Param GetRegister16Pair(int rp)
        {
            //"BC", "DE", "HL", "SP"
            return Z80Param16BitRegister.From(rp);
        }
        private Z80Register GetRegister16Pair_2(int rp)
        {
            var regs2 = new[] { Z80Register.BC, Z80Register.DE, Z80Register.HL, Z80Register.AF };
            return regs2[rp];
        }
        private Z80Flag GetCC(int cc)
        {
            //"NZ", "Z", "NC", "C", "PO", "PE", "P", "M"
            return Z80Ex.Flags.ElementAt(cc);
        }

        public Z80Param GetRegister8(int r)
        {
            // "B", "C", "D", "E", "H", "L", "(HL)", "A"
            return Z80Param8BitRegister.From(r);
        }


        private Z80Instruction ReadUnknown(Opcode opcode)
        {
            return CreateInstr(Z80Op.Unknown, opcode.Op.ToString("X2"));
        }

        private Z80Instruction CreateInstr(Z80Op op, Z80Param p1 = null, Z80Param p2 = null)
        {
            var r = new Z80Instruction(op, _lastInstrAddr, Addr - _lastInstrAddr, p1, p2);
            _lastInstrAddr = Addr;
            return r;
        }
    }

}
