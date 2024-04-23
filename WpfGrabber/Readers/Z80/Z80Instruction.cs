using System.Linq;
using System.Text;

namespace WpfGrabber.Readers.Z80
{
    public enum Z80Op
    {
        Unknown,
        ByParam,
        NOP,
        EX,
        DJNZ,
        JR,
        LD,
        ADD,
        INC,
        DEC,
        ADC,
        SUB,
        SBC,
        OR,
        XOR,
        CP,
        AND,
        RET,
        POP,
        JP,
        OUT,
        IN,
        CALL,
        PUSH,
        BIT,
        RES,
        SET,
        NEG,
        NoniNOP,
    }

    public class Z80Instruction
    {
        public Z80Op Op { get; }
        private string Operand1 { get; }
        private string Operand2 { get; }
        public int Start { get; }
        public int Len { get; }

        public Z80Instruction(Z80Op op, int start, int length, string op1 = null, string op2 = null)
        {
            Op = op;
            Start = start;
            Len = length;
            Operand1 = op1;
            Operand2 = op2;
        }


        public override string ToString()
        {
            var sb = new StringBuilder();
            var ops = new[] { Operand1, Operand2 };
            int skip = 0;
            if (Op == Z80Op.ByParam)
            {
                sb.Append(Operand1);
                skip++;
            }
            else
            {
                sb.Append(Op.ToString());
            }
            var args = ops.Skip(skip).Where(x => x != null).ToArray();
            if(args.Length > 0)
            {
                sb.Append(" ");
                sb.Append(string.Join(", ", args));
            }
            return sb.ToString();
        }
    }
}
