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
        public BitmapSource ReadBitmap(BitReader reader, int total_w, int total_h, int w, int space)
        {
            uint[] pixels = ReadPixels(reader, total_w, total_h, w, space);
            var bmp = BitmapSource.Create(total_w, total_h, 96, 96, PixelFormats.Pbgra32, null, pixels, total_w * 4);
            return bmp;
        }

        public uint[] ReadPixels(BitReader reader, int total_w, int total_h, int w, int space)
        {
            var columnX = 0;
            var pixels = new uint[total_h * total_w];
            while (columnX + w <= total_w)
            {
                for (int y = 0; y < total_h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        var bit = reader.ReadBit();
                        if (reader.Position >= reader.DataLength)
                            break;
                        var dest = y * total_w + x + columnX;
                        if (dest >= pixels.Length)
                            break;
                        pixels[dest] = bit ? 0xffffffff : 0xff000000;
                    }
                }
                columnX += w + space;
            }

            return pixels;
        }
    }
}
