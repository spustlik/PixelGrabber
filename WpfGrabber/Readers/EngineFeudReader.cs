using System;
using System.Collections.Generic;
using WpfGrabber.Readers;

namespace WpfGrabber.ViewParts
{
    public class EngineFeudReader : EngineReader
    {
        public int EndPos { get; private set; }
        public EngineFeudReader(DataReader r) : base(r)
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
                var aimg = ReadImage();
                yield return new ReaderImageResult(aimg, start, Reader.BytePosition);
            }
        }

        private ByteBitmap8Bit ReadImage()
        {
            //4x4 x (8x8) pixels
            var result = new ByteBitmap8Bit(32, 32);
            for (int u = 0; u < 4; u++)
            {
                for (int v = 0; v < 4; v++)
                {
                    ReadTile(result, v * 8, u * 8);
                }
            }
            var wtf = Reader.ReadBytes(16);
            return result;
        }

        private void ReadTile(ByteBitmap8Bit result, int dx, int dy)
        {
            for (var y = 0; y < 8; y++)
            {
                var b = Reader.ReadByte();
                for (var x = 0; x < 8; x++)
                {
                    var bit = (byte)((b >> (7 - x)) & 1);
                    var c = (bit == 0) ? 1 : 0;
                    result.SetPixel(dx + x, dy + y, (byte)c);
                }
            }
        }
    }
}