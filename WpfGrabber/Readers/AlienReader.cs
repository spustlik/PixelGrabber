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
        private DataReader reader;

        public bool FlipY { get; set; }
        public AlienReader(DataReader reader)
        {
            this.reader = reader;
        }

        public static IEnumerable<AlienImage> ReadList(byte[] bytes, int pos, int endpos, bool flipY)
        {
            if (endpos <= 0)
                endpos = bytes.Length;
            var dr = new DataReader(bytes, pos, flipX: false);
            var ar = new AlienReader(dr) { FlipY = flipY };
            var images = new List<AlienImage>();
            while (!dr.IsEmpty)
            {
                var start = dr.BytePosition;
                if (endpos > 0 && dr.BytePosition >= endpos)
                    break;
                if (!ar.TryRead(out var w, out var h))
                    break;
                if (pos > endpos || w >= 64 || h >= 64)
                    break;
                var aimg = ar.ReadMaskedImage();
                images.Add(new AlienImage() { Bitmap = aimg, Position = start });
            }
            return images;
        }

        public class AlienImage
        {
            public ByteBitmap8Bit Bitmap { get; set; }
            public int Position { get; set; }
        }

        public bool TryRead(out int w, out int h)
        {
            w = 0;
            h = 0;
            if (reader.BytePosition > reader.DataLength - 2)
                return false;
            w = reader.Data[reader.BytePosition];
            h = reader.Data[reader.BytePosition + 1];
            return w != 0 && h != 0;
        }
        public ByteBitmap8Bit ReadMaskedImage()
        {
            var w = reader.ReadByte();
            var h = reader.ReadByte();
            var result = new ByteBitmap8Bit(w * 8, h);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (reader.IsEmpty)
                        break;
                    var d = reader.ReadByte();
                    var m = reader.ReadByte();
                    for (int i = 0; i < 8; i++)
                    {
                        int shift = (7 - i);
                        var db = (d >> shift) & 1;
                        var mb = (m >> shift) & 1;
                        var ry = y;
                        if (FlipY)
                            ry = h - y - 1;
                        if (mb == 1)
                        {
                            result.SetPixel(x * 8 + i, ry, 0);
                        }
                        else
                        {
                            result.SetPixel(x * 8 + i, ry, db == 0 ? (byte)1 : (byte)2);
                        }
                    }
                }
            }
            return result;
        }
    }
}
