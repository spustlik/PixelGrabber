using Microsoft.Win32;
using System;
using System.Linq;
using System.Windows.Forms;

namespace WpfGrabber.Readers.Z80
{
    public abstract class Z80Param
    {

        public static implicit operator Z80Param(Z80Flag flag) => new Z80ParamFlag(flag);
        public static implicit operator Z80Param(Z80Register register)
        {
            if (register.Is8BitBasic())
                return Z80Param8BitRegister.From(register);
            if (register.IsAny16Bit())
                return Z80Param16BitRegister.From(register);
            throw new NotSupportedException();
        }
    }

    public class Z80Param8BitRegister : Z80Param
    {
        private Z80Register register;

        private Z80Param8BitRegister()
        {
        }
        public static Z80Param From(int r)
        {
            return new Z80Param8BitRegister() { register = Z80Ex.GetRegister8ByIndex(r) };
        }
        public static Z80Param From(Z80Register register)
        {
            return new Z80Param8BitRegister() { register = register };
        }

        public override string ToString()
        {
            if (register == Z80Register.HL)
                return "(HL)"; // warning, it can be 16 bit!
            return register.ToString();
        }

    }
    public class Z80Param16BitRegister : Z80Param
    {
        private Z80Register register;
        private Z80Param16BitRegister()
        {
        }
        public static Z80Param From(Z80Register register)
        {
            return new Z80Param16BitRegister() { register = register };
        }
        public static Z80Param From(int rp)
        {
            var register = Z80Ex.PairRegisters16.ElementAt(rp);
            return new Z80Param16BitRegister() { register = register };
        }
        public override string ToString()
        {
            if (register == Z80Register.AF_EX)
                return "AF`";
            return register.ToString();
        }
    }

    public class Z80ParamLiteral : Z80Param
    {
        public int Value { get; }
        public int Size { get; }

        public Z80ParamLiteral(int value, int size)
        {
            Value = value;
            Size = size;
        }
        public override string ToString()
        {
            if (Size == 1)
                return Value.ToString();
            if (Size == 2)
                return "0x" + Value.ToString("X2");
            return "0x" + Value.ToString("X4");
        }
    }
    public class Z80ParamAddressLabel : Z80Param
    {
        public int Addr { get; }
        public string Alias { get; set; }
        public Z80ParamAddressLabel(int addr)
        {
            Addr = addr;
        }
        public override string ToString() => Alias ?? ("L" + Addr.ToString("X4"));

    }
    
    public class Z80ParamFlag : Z80Param
    {
        public Z80Flag Flag { get; }

        public Z80ParamFlag(Z80Flag flag)
        {
            this.Flag = flag;
        }

        public override string ToString() => Flag.ToString();
    }

    public class Z80ParamIndirect : Z80Param
    {
        public Z80Param DirectParam { get; }
        public Z80ParamIndirect(Z80Param directParam)
        {
            DirectParam = directParam;
        }

        public override string ToString() => "(" + DirectParam + ")";
    }
}
