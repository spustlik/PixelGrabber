using Microsoft.Win32;
using System;
using System.Linq;
using System.Windows.Forms;

namespace WpfGrabber.Readers.Z80
{
    public abstract class Z80Param
    {
        public static implicit operator Z80Param(string value) => new Z80ParamString(value);
        public static implicit operator Z80Param(Z80Flag flag) => new Z80ParamFlag(flag);
        public static implicit operator Z80Param(Z80Register register)
        {
            if (register.Is8BitBasic())
                return Z80Param8BitRegister.From(register);
            if (register.Is16Bit())
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
            return new Z80Param8BitRegister() { register=register };
        }

        public override string ToString()
        {
            if (register == Z80Register.HL)
                return "(HL)";
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
        public override string ToString() => register.ToString();

    }



    public class Z80ParamString : Z80Param
    {
        public string Value { get; }

        public Z80ParamString(string value)
        {
            this.Value = value;
        }

        public override string ToString() => Value;
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
}
