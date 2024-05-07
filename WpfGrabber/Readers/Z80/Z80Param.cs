namespace WpfGrabber.Readers.Z80
{
    public abstract class Z80Param
    {
        public static implicit operator Z80Param(string value) => new Z80ParamString(value);
        public static implicit operator Z80Param(Z80Register register) => new Z80ParamRegister(register);
    }

    public class Z80ParamRegister : Z80Param
    {
        public Z80Register Register { get; }

        public Z80ParamRegister(Z80Register register)
        {
            this.Register = register;
        }

        public override string ToString() => Register.ToString();
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
}
