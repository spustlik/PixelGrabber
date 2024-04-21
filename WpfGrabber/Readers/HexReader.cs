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
                asciiLine.Append(b >= 32 ? 'A' : ' '); //☺ (char)b 
                hexline.Append(b.ToString("X2"));
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
                    sb.Append(Position.ToString("X4")).Append(": ");
                var line = ReadLineStr();
                if(showHex)
                    sb.Append(line.Hex);
                if (showHex && showAscii)
                    sb.Append("  ");
                if (showAscii)
                    sb.Append(line.Ascii);
                yield return sb.ToString();
            }
        }

    }
}
