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
        private int _lastInstr = 0;
        public Z80Reader(byte[] data, int start = 0)
        {
            Data = data;
            Addr = start;
            _lastInstr = start;
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

        public struct Opcode
        {
            public int x { get; set; }
            public int y { get; set; }
            public int z { get; set; }
            public int p { get; set; }
            public int q { get; set; }
            public byte Op { get; set; }
        }
        private Opcode CreateOpCode(byte opcode)
        {
            var x = (opcode & 0b11000000) >> 6;
            var y = (opcode & 0b00111000) >> 3;
            var z = (opcode & 0b00000111);
            var p = y >> 1;
            var q = y % 2;
            if (y < 0 || y > 7)
                throw new InvalidOperationException();
            return new Opcode() { Op = opcode, x = x, y = y, z = z, p = p, q = q };
        }
        private Opcode ReadOpCode()
        {
            return CreateOpCode(ReadByte());
        }
        private Opcode ReadOpCodeData(out int x, out int y, out int z, out int p, out int q)
        {
            var r = CreateOpCode(ReadByte());
            x = r.x; 
            y = r.y; 
            z = r.z; 
            p = r.p; 
            q = r.q;
            return r;
        }

        public Z80Instruction ReadInstruction()
        {
            var opcode = ReadOpCode();
            if (opcode.Op == 0xCB) // b11001011
                return ReadCB();
            if (opcode.Op == 0xED)
                return ReadED();
            if (opcode.Op == 0xDD)
                return ReadDDFD(opcode, "IX");
            if (opcode.Op == 0xFD)
                return ReadDDFD(opcode,"IY");

            switch (opcode.x)
            {
                case 0: return Read0(opcode);
                case 1: return Read1(opcode);
                case 2: return Read2(opcode);
                case 3: return Read3(opcode);
                default: return ReadUnknown(opcode);
            }
        }

        private Z80Instruction ReadDDFD(Opcode opcode, string reg)
        {
            //this need to refactor Read0-3 and call it with replacement of HL,H,L, not (HL) registers as IX,IXH,IXL
            // (HL) is replaced by (IX+d) d=ReadSignedByte()
            // and more (read)
            return CreateInstr(Z80Op.Unknown, opcode.Op.ToString("X2"), reg);
        }

        private Z80Instruction ReadED()
        {
            var opcode = ReadOpCodeData(out var x, out var y, out var z, out _, out _);
            if (x == 0 || x == 3)
                return CreateInstr(Z80Op.NoniNOP);
            switch (x)
            {
                case 1: return ReadED1(opcode);
                case 2:
                    if (y < 4 || z > 3)
                        return CreateInstr(Z80Op.NoniNOP);
                    return CreateInstr(Z80Op.ByParam, GetBli(y, z));
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
                        case 6: return CreateInstr(Z80Op.IN, "(C)");
                        default: return CreateInstr(Z80Op.IN, GetR(opcode.y), "(C)");
                    }
                case 1:
                    switch (opcode.y)
                    {
                        case 6: return CreateInstr(Z80Op.OUT, "(C)");
                        default: return CreateInstr(Z80Op.OUT, GetR(opcode.y), "(C)");
                    }
                case 2:
                    switch (opcode.q)
                    {
                        case 0: return CreateInstr(Z80Op.SBC, "HL", GetRP(opcode.p));
                        case 1: return CreateInstr(Z80Op.ADC, "HL", GetRP(opcode.p));
                        default: throw new InvalidOperationException();
                    }
                case 3:
                    switch (opcode.q)
                    {
                        case 0: return CreateInstr(Z80Op.LD, ReadIndirectAddr16(), GetRP(opcode.p));
                        case 1: return CreateInstr(Z80Op.LD, GetRP(opcode.p), ReadIndirectAddr16());
                        default: throw new InvalidOperationException();
                    }
                case 4: return CreateInstr(Z80Op.NEG);
                case 5:
                    switch (opcode.y)
                    {
                        case 1: return CreateInstr(Z80Op.ByParam, "RETI");
                        default: return CreateInstr(Z80Op.ByParam, "RETN");
                    }
                case 6: return CreateInstr(Z80Op.ByParam, "IM", GetIM(opcode.y));
                case 7:
                    switch (opcode.y)
                    {
                        case 0: return CreateInstr(Z80Op.LD, "I", "A");
                        case 1: return CreateInstr(Z80Op.LD, "R", "A");
                        case 2: return CreateInstr(Z80Op.LD, "A", "I");
                        case 3: return CreateInstr(Z80Op.LD, "A", "R");
                        case 4: return CreateInstr(Z80Op.ByParam, "RRD");
                        case 5: return CreateInstr(Z80Op.ByParam, "RLD");
                        case 6: return CreateInstr(Z80Op.NOP);
                        case 7: return CreateInstr(Z80Op.NOP);
                        default: throw new InvalidOperationException();
                    }
                default: throw new InvalidOperationException();
            }
        }

        private Z80Instruction ReadCB()
        {
            ReadOpCodeData(out var x, out var y, out var z, out _, out _);
            switch (x)
            {
                case 0: return CreateInstr(Z80Op.ByParam, GetRot(y), GetR(z));
                case 1: return CreateInstr(Z80Op.BIT, y + "", GetR(z));
                case 2: return CreateInstr(Z80Op.RES, y + "", GetR(z));
                case 3: return CreateInstr(Z80Op.SET, y + "", GetR(z));
                default: throw new InvalidOperationException();
            }
        }

        private string GetRot(int i)
        {
            return new[] { "RLC", "RRC", "RL", "RR", "SLA", "SRA", "SLL", "SRL" }[i];
        }

        private Z80Instruction Read3(Opcode opcode)
        {
            switch (opcode.z)
            {
                case 0: return CreateInstr(Z80Op.RET, GetCC(opcode.y));
                case 1:
                    switch (opcode.q)
                    {
                        case 0: return CreateInstr(Z80Op.POP, GetRP2(opcode.p));
                        case 1:
                            switch (opcode.p)
                            {
                                case 0: return CreateInstr(Z80Op.RET);
                                case 1: return CreateInstr(Z80Op.ByParam, "EXX");
                                case 2: return CreateInstr(Z80Op.JP, "HL");
                                case 3: return CreateInstr(Z80Op.LD, "SP", "HL");
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
                        case 2: return CreateInstr(Z80Op.OUT, $"({ReadByte()})", "A");
                        case 3: return CreateInstr(Z80Op.IN, "A", $"({ReadByte()})");
                        case 4: return CreateInstr(Z80Op.EX, "SP", "(HL)");
                        case 5: return CreateInstr(Z80Op.EX, "DE", "HL");
                        case 6: return CreateInstr(Z80Op.ByParam, "DI");
                        case 7: return CreateInstr(Z80Op.ByParam, "EI");
                        default: throw new InvalidOperationException();
                    }
                case 4: return CreateInstr(Z80Op.CALL, GetCC(opcode.y), GetAddrLabel(ReadUnsigned16()));
                case 5:
                    switch (opcode.q)
                    {
                        case 0: return CreateInstr(Z80Op.PUSH, GetRP2(opcode.p));
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
                case 6: return ReadAlu(opcode.y, $"0x{ReadByte():X2}");
                case 7: return CreateInstr(Z80Op.ByParam, "RST", $"0x{opcode.y * 8}:X2");
                default: throw new InvalidOperationException();
            }
        }

        private string GetRP2(int p)
        {
            return new[] { "BC", "DE", "HL", "AF" }[p];
        }

        private Z80Instruction Read2(Opcode opcode)
        {
            return ReadAlu(opcode.y, GetR(opcode.z));
        }

        private Z80Instruction ReadAlu(int alu, string p)
        {
            switch (alu)
            {
                case 0: return CreateInstr(Z80Op.ADD, "A", p);
                case 1: return CreateInstr(Z80Op.ADC, "A", p);
                case 2: return CreateInstr(Z80Op.SUB, "A", p);
                case 3: return CreateInstr(Z80Op.SBC, "A", p);
                case 4: return CreateInstr(Z80Op.AND, p);
                case 5: return CreateInstr(Z80Op.XOR, p);
                case 6: return CreateInstr(Z80Op.OR, p);
                case 7: return CreateInstr(Z80Op.CP, p);
                default: throw new InvalidOperationException();
            }
        }

        private Z80Instruction Read1(Opcode opcode)
        {
            if(opcode.y==6 && opcode.z==6)
                return CreateInstr(Z80Op.ByParam, "HALT");
            return CreateInstr(Z80Op.LD, GetR(opcode.y), GetR(opcode.z));
        }

        private Z80Instruction Read0(Opcode opcode)
        {
            switch (opcode.z)
            {
                case 0:
                    switch (opcode.y)
                    {
                        case 0: return CreateInstr(Z80Op.NOP);
                        case 1: return CreateInstr(Z80Op.EX, "AF", "AF`");
                        case 2: return CreateInstr(Z80Op.DJNZ, GetAddrLabel(Addr + ReadSignedByte() + 1));
                        case 3: return CreateInstr(Z80Op.JR, GetAddrLabel(Addr + ReadSignedByte() + 1));
                        //4-7
                        default: return CreateInstr(Z80Op.JR, GetCC(opcode.y - 4), GetAddrLabel(Addr + ReadSignedByte() + 1));
                    }
                case 1:
                    switch (opcode.q)
                    {
                        case 0: return CreateInstr(Z80Op.LD, GetRP(opcode.p), $"0x{ReadUnsigned16():X4}");
                        case 1: return CreateInstr(Z80Op.ADD, "HL", GetRP(opcode.p));
                        default: throw new InvalidOperationException();
                    }
                case 2:
                    switch (opcode.q)
                    {
                        case 0:
                            switch (opcode.p)
                            {
                                case 0: return CreateInstr(Z80Op.LD, "(BC)", "A");
                                case 1: return CreateInstr(Z80Op.LD, "(DE)", "A");
                                case 2: return CreateInstr(Z80Op.LD, ReadIndirectAddr16(), "HL");
                                case 3: return CreateInstr(Z80Op.LD, ReadIndirectAddr16(), "A");
                                default: throw new InvalidOperationException();
                            }
                        case 1:
                            switch (opcode.p)
                            {
                                case 0: return CreateInstr(Z80Op.LD, "A", "(BC)");
                                case 1: return CreateInstr(Z80Op.LD, "A", "(DE)");
                                case 2: return CreateInstr(Z80Op.LD, "HL", ReadIndirectAddr16());
                                case 3: return CreateInstr(Z80Op.LD, "A", ReadIndirectAddr16());
                                default: throw new InvalidOperationException();
                            }
                        default: throw new InvalidOperationException();
                    }
                case 3:
                    switch (opcode.q)
                    {
                        case 0: return CreateInstr(Z80Op.INC, GetRP(opcode.p));
                        case 1: return CreateInstr(Z80Op.DEC, GetRP(opcode.p));
                        default: throw new InvalidOperationException();
                    }
                case 4:
                    return CreateInstr(Z80Op.INC, GetR(opcode.y));
                case 5:
                    return CreateInstr(Z80Op.DEC, GetR(opcode.y));
                case 6:
                    return CreateInstr(Z80Op.LD, GetR(opcode.y), $"0x{ReadByte():X2}");
                case 7:
                    switch (opcode.y)
                    {
                        case 0: return CreateInstr(Z80Op.ByParam, "RLCA");
                        case 1: return CreateInstr(Z80Op.ByParam, "RRCA");
                        case 2: return CreateInstr(Z80Op.ByParam, "RLA");
                        case 3: return CreateInstr(Z80Op.ByParam, "RRA");
                        case 4: return CreateInstr(Z80Op.ByParam, "DAA");
                        case 5: return CreateInstr(Z80Op.ByParam, "CPL");
                        case 6: return CreateInstr(Z80Op.ByParam, "SCF");
                        case 7: return CreateInstr(Z80Op.ByParam, "CCF");
                        default: throw new InvalidOperationException();
                    }
                default: throw new InvalidOperationException();
            }
        }

        private string ReadIndirectAddr16()
        {
            return $"(0x{ReadUnsigned16():X4})";
        }

        private string GetBli(int y, int z)
        {
            /* 0 1 2 3
            a=4	LDI	CPI	INI	OUTI
            a=5	LDD	CPD	IND	OUTD
            a=6	LDIR	CPIR	INIR	OTIR
            a=7	LDDR	CPDR	INDR	OTDR
            */
            var bli = new string[4, 4]
            {
                { "LDI","CPI","INI","OUTI" },
                { "LDD","CPD","IND","OUTD" },
                { "LDIR","CPIR","INIR","OTIR" },
                { "LDDR","CPDR","INDR","OTDR" }
            };
            return bli[y - 4, z];
        }
        private string GetIM(int i)
        {
            return new[] { "0", "0/1", "1", "2", "0", "0/1", "1", "2" }[i];
        }


        private string GetRP(int rp)
        {
            return new[] { "BC", "DE", "HL", "SP" }[rp];
        }
        private string GetCC(int cc)
        {
            return new[] { "NZ", "Z", "NC", "C", "PO", "PE", "P", "M" }[cc];
        }
        public string GetR(int r)
        {
            return new[] { "B", "C", "D", "E", "H", "L", "(HL)", "A" }[r];
        }
        private string GetAddrLabel(int addr)
        {
            return "L" + addr.ToString("X4");
        }

        private Z80Instruction ReadUnknown(Opcode opcode)
        {
            return CreateInstr(Z80Op.Unknown, opcode.Op.ToString("X2"));
        }

        //params should be InstructionParam with implicit cast from Register enum, int addr, etc
        private Z80Instruction CreateInstr(Z80Op op, string p1 = null, string p2 = null)
        {
            var r = new Z80Instruction(op, _lastInstr, Addr - _lastInstr, p1, p2);
            _lastInstr = Addr;
            return r;
        }
    }
}
