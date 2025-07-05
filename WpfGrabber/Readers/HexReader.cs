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
        public bool ShowAddr { get; set; } = true;
        public bool ShowAscii { get; set; } = true;
        public bool ShowHex { get; set; } = true;

        public (string Hex, string Ascii) ReadLineStr(DataReader rd)
        {
            var hexline = new StringBuilder();
            var asciiLine = new StringBuilder();
            var x = 0;

            while (!rd.IsEmpty)
            {
                var b = rd.ReadByte();
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

        public IEnumerable<string> ReadLines(DataReader rd)
        {
            while (!rd.IsEmpty)
            {
                var sb = new StringBuilder();
                if (ShowAddr)
                    sb.Append(ToHex(rd.BytePosition)).Append(": ");
                var line = ReadLineStr(rd);
                if (ShowHex)
                    sb.Append(line.Hex);
                if (ShowHex && ShowAscii)
                    sb.Append("  " + new String(' ', 3 * 16 + 2 - line.Hex.Length));
                if (ShowAscii)
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
                x >>= 4;
            }
            return new string(s);
        }

        public static string ToBinary(byte b, bool flipX)
        {
            if (flipX)
                b = DataReader.GetFlippedX(b);
            var sb = new StringBuilder();
            for (int i = 0; i < 8; i++)
            {
                sb.Append(b & 1);
                b >>= 1;
            }
            return sb.ToString();

        }
    }
}
