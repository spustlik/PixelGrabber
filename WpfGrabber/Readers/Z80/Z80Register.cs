using System;
using System.Collections.Generic;
using System.Linq;

namespace WpfGrabber.Readers.Z80
{
    public enum Z80Register
    {
        A,
        B,
        C,
        D,
        E,
        F,
        H,
        L,
        I,
        R,

        BC,
        DE,
        HL,

        IX,
        IY,
        SP,
        PC,
        AF,
        AF_EX
    }
    public static class Z80Ex
    {
        public static IEnumerable<Z80Register> PairRegisters16 { get; }
            = new[] { Z80Register.BC, Z80Register.DE, Z80Register.HL, Z80Register.SP };
        public static IEnumerable<Z80Register> Registers16 { get; }
            = PairRegisters16.Concat(new[] { Z80Register.IX, Z80Register.IY, Z80Register.PC, Z80Register.AF, Z80Register.AF_EX });

        public static bool Is16BitBasic(this Z80Register reg)
        {
            return PairRegisters16.Contains(reg);
        }
        public static bool IsAny16Bit(this Z80Register reg)
        {
            return Registers16.Contains(reg);
        }
        private static IEnumerable<Z80Register> _registers8Basic { get; }
            = new[] { Z80Register.B, Z80Register.C, Z80Register.D, Z80Register.E, Z80Register.H, Z80Register.L, Z80Register.HL, Z80Register.A };


        public static bool Is8BitBasic(this Z80Register reg)
        {
            return _registers8Basic.Contains(reg);
        }

        public static Z80Register GetRegister8ByIndex(int r)
        {
            return _registers8Basic.ElementAt(r);
        }

        public static IEnumerable<Z80Flag> Flags { get; }
            //= new[] {Z80Flags.NZ}
            = Enum.GetValues(typeof(Z80Flag)).Cast<Z80Flag>();
    }
    public enum Z80Flag
    {
        NZ, Z, NC, C, PO, PE, P, M
    }

}
