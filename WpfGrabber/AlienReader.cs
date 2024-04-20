using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfGrabber
{
    //reader of stored images
    //format:
    // byte width - pixels 
    // byte height
    // byte[] data - 2 bytes for mask and data for each 8 pixels

    public class AlienReader
    {
        private readonly byte[] bytes;
        public int Position { get; set; }
        public AlienReader(byte[] bytes)
        {
            this.bytes = bytes;
        }

        public byte Read()
        {
            return bytes[Position++];
        }
        public ByteImage8Bit ReadMaskedImage()
        {
            var w = Read();
            var h = Read();
            var result = new ByteImage8Bit(w*8, h);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var d = Read();
                    var m = Read();
                    for (int i = 0; i < 8; i++)
                    {
                        int shift = (7 - i);
                        var db = (d >> shift) & 1;
                        var mb = (m >> shift) & 1;
                        if (mb == 1)
                        {
                            result.SetPixel(x * 8 + i, y, 0);
                        }
                        else
                        {
                            result.SetPixel(x * 8 + i, y, db == 0 ? (byte)1 : (byte)2);
                        }
                    }
                }
            }
            return result;
        }

    }
}
