using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace WpfGrabber.Readers
{
    public class HexReader
    {
        public int Position { get; set; }
        public byte[] Data { get; private set; }
        public HexReader(byte[] data, int offset)
        {
            Data = data;
            Position = offset;
        }

        public (string Hex, string Ascii) ReadLineStr()
        {
            var hexline = new StringBuilder();
            var asciiLine = new StringBuilder();
            var x = 0;
            while (Position < Data.Length)
            {
                var b = Data[Position++];
                Char c = (char)b;
                //if (b < 32)
                //    c = '☺';
                //else if (b >= 0x7f)
                //    c = '♯';
                if (c < 0x20 || c >= 0x7f)
                    c = ' ';
                asciiLine.Append(c);
                hexline.Append(ToHex(b, 2));
                hexline.Append(" ");
                if (x == 7)
                {
                    hexline.Append("| ");
                    asciiLine.Append(" ");
                }
                x++;
                if (x >= 16)
                    break;
            }
            return (Hex: hexline.ToString(), Ascii: asciiLine.ToString());
        }
        public string ReadLine()
        {
            return ReadLineStr().Hex;
        }
        public IEnumerable<string> ReadLines(bool showAddr = false, bool showAscii = false, bool showHex = false)
        {
            while (Position < Data.Length)
            {
                var sb = new StringBuilder();
                if (showAddr)
                    sb.Append(ToHex(Position)).Append(": ");
                var line = ReadLineStr();
                if (showHex)
                    sb.Append(line.Hex);
                if (showHex && showAscii)
                    sb.Append("  " + new String(' ', 3 * 16 + 2 - line.Hex.Length));
                if (showAscii)
                    sb.Append(line.Ascii);
                yield return sb.ToString();
            }
        }

        public static string ToHex(int x, int len = 4)
        {
            const string _HEX = "0123456789ABCDEF";
            var s = new char[len];
            for (int i = len - 1; i >= 0; i--)
            {
                s[i] = _HEX[x & 0x0F];
                x = x >> 4;
            }
            return new string(s);
        }
    }
}
