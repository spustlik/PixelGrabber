using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace WpfGrabber.Readers
{
    public class BitImageReader
    {
        public BitmapSource ReadBitmap(DataReader reader, int total_w, int total_h, int col_w, int space)
        {
            uint[] pixels = ReadPixels(reader, total_w, total_h, col_w, space);
            var bmp = BitmapSource.Create(total_w, total_h, 96, 96, PixelFormats.Pbgra32, null, pixels, total_w * 4);
            return bmp;
        }

        public uint[] ReadPixels(DataReader reader, int total_w, int total_h, int col_w, int space)
        {
            var columnX = 0;
            var pixels = new uint[total_h * total_w];
            while (columnX + col_w <= total_w)
            {
                for (int y = 0; y < total_h; y++)
                {
                    for (int x = 0; x < col_w; x++)
                    {
                        var bit = reader.ReadBit();
                        if (reader.BytePosition >= reader.DataLength)
                            break;
                        var dest = y * total_w + x + columnX;
                        if (dest >= pixels.Length)
                            break;
                        pixels[dest] = bit ? 0xffffffff : 0xff000000;
                    }
                }
                columnX += col_w + space;
            }
            return pixels;
        }
        public BitAddressFunction GetAddressFunction(int total_w, int total_h, int col_w, int space)
        {
            return (x, y) =>
            {
                if (x < 0 || y < 0 || x >= total_w)
                    return -1;
                int col = x / (col_w + space);
                int colOffset = x % (col_w + space);
                if (colOffset >= col_w)
                    return -1;
                int addr = colOffset / 8 + col * col_w/8 * total_h + y*col_w/8;
                return addr;//y * total_w + x + columnX;
            };
        }
    }
    public delegate int BitAddressFunction(int x, int y);
}
