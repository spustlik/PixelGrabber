using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfGrabber.Data;

namespace WpfGrabber.Readers
{
    public class FontReader
    {
        public bool FlipY { get; set; }
        public int Height { get; set; }
        public FontReader(int height)
        {
            Height = height;
        }

        public IEnumerable<BitBitmap> ReadImages(DataReader br, int count = 256)
        {
            const int width = 8;
            int counter = 0;
            while (br.BytePosition < br.DataLength)
            {
                var letter = new BitBitmap(width, Height);
                for (int y = 0; y < Height; y++)
                {
                    var ly = FlipY ? Height - y : y;
                    for (int x = 0; x < width; x++)
                    {
                        var b = br.ReadBit();
                        if (b)
                            letter.SetPixel(x, ly, true);
                    }
                }
                yield return letter;
                counter++;
                if (counter >= count)
                    break;
            }
        }


    }
}
