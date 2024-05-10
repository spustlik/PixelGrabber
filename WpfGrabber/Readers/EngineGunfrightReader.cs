using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfGrabber.Readers
{
    internal class EngineGunfrightReader : EngineReader
    {
        public EngineGunfrightReader(DataReader reader):base(reader) { }

        public override IEnumerable<ReaderImageResult> ReadImages()
        {
            var r1 = new MaskReader(Reader)
            {
                FlipByte = true,
                FlipY = true,
                Type = MaskReaderType.ByteDataMask,
                Preambule = MaskReaderPreambule.WidthHeight
            };
            var r2 = new MaskReader(Reader)
            {
                FlipByte = true,
                FlipY = true,
                Type = MaskReaderType.ByteDataMask,
                Preambule = MaskReaderPreambule.None
            };
            var r3 = new MaskReader(Reader)
            {
                FlipY = true,
                FlipByte = true,
                Type = MaskReaderType.ImageData,
                Preambule = MaskReaderPreambule.None
            };
            while (!Reader.IsEmpty)
            {
                var pos = Reader.BytePosition;
                var b = Reader.PeekByte();
                var r = r1;
                while (b == 0 && !Reader.IsEmpty)
                {
                    b = Reader.ReadByte();
                }
                if ((b & 0x80) == 0)
                {
                    //gunfright
                    r = r3;
                    //Reader.ReadByte();
                    r.Height = Reader.ReadByte();
                    r.Width = 2;
                }
                else
                {
                    //3B75: 84 20 - 4x32, no mask
                    r = r2;
                    var w = Reader.ReadByte();
                    var h = Reader.ReadByte();
                    r.Width = w & 0x0F;
                    r.Height = h;
                }

                var bmp = r.Read();
                var img = new ReaderImageResult(bmp, pos, r.Reader.BytePosition);
                yield return img;
            }
        }
    }
}
