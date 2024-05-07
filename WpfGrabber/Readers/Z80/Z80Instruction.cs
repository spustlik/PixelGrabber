using System.Linq;
using System.Text;

namespace WpfGrabber.Readers.Z80
{
    public class Z80Instruction
    {
        public Z80Op Op { get; }
        private Z80Param Operand1 { get; }
        private Z80Param Operand2 { get; }
        public int Start { get; }
        public int Len { get; }

        public Z80Instruction(Z80Op op, int start, int length, Z80Param op1 = null, Z80Param op2 = null)
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
            sb.Append(Op.ToString());
            var args = ops.Skip(skip).Where(x => x != null).Select(x => x.ToString()).ToArray();
            if (args.Length > 0)
            {
                sb.Append(" ");
                sb.Append(string.Join(", ", args));
            }
            return sb.ToString();
        }
    }
}
