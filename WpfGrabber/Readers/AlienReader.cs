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
        public static List<ByteBitmap8Bit> ReadList(byte[] bytes, int pos, int endpos)
        {
            var ar = new AlienReader(bytes) { Position = pos };
            var images = new List<ByteBitmap8Bit>();
            while (true)
            {
                if (endpos > 0 && ar.Position >= endpos)
                    break;
                if (!ar.TryRead(out var w, out var h))
                    break;
                if (endpos < 0 && (w >= 64 || h >= 64))
                    break;
                var aimg = ar.ReadMaskedImage();
                images.Add(aimg);
            }
            return images;
        }

        public byte Read()
        {
            if (Position >= bytes.Length)
                return 0;
            return bytes[Position++];
        }

        public bool TryRead(out int w, out int h)
        {
            w = 0;
            h = 0;
            if (Position > bytes.Length - 2)
                return false;
            w = bytes[Position];
            h = bytes[Position + 1];
            return w != 0 && h != 0;
        }
        public ByteBitmap8Bit ReadMaskedImage()
        {
            var w = Read();
            var h = Read();
            var result = new ByteBitmap8Bit(w * 8, h);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (Position >= bytes.Length)
                        break;
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
