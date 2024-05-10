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
    public class EngineAlienReader : EngineReader
    {
        public bool FlipY { get; set; }
        public int EndPos { get; private set; }
        public EngineAlienReader(DataReader reader) : base(reader)
        {
        }

        public override IEnumerable<ReaderImageResult> ReadImages()
        {
            if (EndPos <= 0)
                EndPos = this.Reader.DataLength;
            while (!Reader.IsEmpty)
            {
                var start = Reader.BytePosition;
                if (EndPos > 0 && Reader.BytePosition >= EndPos)
                    break;
                if (!TryRead(out var w, out var h))
                    break;
                if (Reader.BytePosition >= EndPos || w >= 64 || h >= 64)
                    break;
                var aimg = ReadMaskedImage();
                yield return new ReaderImageResult(aimg, start, Reader.BytePosition);
            }
        }

        public bool TryRead(out int w, out int h)
        {
            w = 0;
            h = 0;
            if (Reader.BytePosition > Reader.DataLength - 2)
                return false;
            w = Reader.Data[Reader.BytePosition];
            h = Reader.Data[Reader.BytePosition + 1];
            return w != 0 && h != 0;
        }
        public ByteBitmap8Bit ReadMaskedImage()
        {
            var w = Reader.ReadByte();
            var h = Reader.ReadByte();
            var result = new ByteBitmap8Bit(w * 8, h);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (Reader.IsEmpty)
                        break;
                    var d = Reader.ReadByte();
                    var m = Reader.ReadByte();
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
