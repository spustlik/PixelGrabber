using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfGrabber.Readers
{
    internal class EngineFilmationReader
    {
        public DataReader Reader { get; }

        public EngineFilmationReader(DataReader reader)
        {
            this.Reader = reader;
        }

        public IEnumerable<ReaderImageResult> ReadImages()
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
            while (!Reader.IsEmpty)
            {
                var pos = Reader.BytePosition;
                var b = Reader.PeekByte();
                var r = r1;
                if ((b & 0xF0) == 0)
                {
                    //32A7: 03 1F - 3x31, masked
                    r = r1;
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
