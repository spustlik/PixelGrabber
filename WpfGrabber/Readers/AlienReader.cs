using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfGrabber.Readers;

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
            //flipX:false
            this.reader = reader;
        }

        public IEnumerable<ReaderImageResult> ReadImages(int endpos)
        {
            if (endpos <= 0)
                endpos = this.reader.DataLength;
            while (!reader.IsEmpty)
            {
                var start = reader.BytePosition;
                if (endpos > 0 && reader.BytePosition >= endpos)
                    break;
                if (!TryRead(out var w, out var h))
                    break;
                if (reader.BytePosition >= endpos || w >= 64 || h >= 64)
                    break;
                var aimg = ReadMaskedImage();
                yield return new ReaderImageResult(aimg, start, reader.BytePosition);
            }
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
