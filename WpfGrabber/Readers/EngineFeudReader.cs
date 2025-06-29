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
            //4x4 x (8) bytes + 4*4 colors
            var result = new ByteBitmap8Bit(32, 32);
            var tileData = Reader.ReadBytes(4 * 4 * 8);
            var colorsFG = Reader.ReadBytes(16);
            for (int u = 0; u < 4; u++)
            {
                for (int v = 0; v < 4; v++)
                {
                    var o = v + u * 4;
                    var data = tileData.GetRange(o * 8, 8);
                    ReadSprite(data, result, v * 8, u * 8, colorsFG[v + (3-u) * 4]);
                }
            }
            return result;
        }

        private static void ReadSprite(byte[] data, ByteBitmap8Bit result, int dx, int dy, byte colorFG)
        {
            for (var y = 0; y < 8; y++)
            {
                var b = data[y];
                for (var x = 0; x < 8; x++)
                {
                    var bit = (byte)((b >> (7 - x)) & 1);
                    var c = (bit == 1) ? colorFG >> 4 : colorFG & 0xF;
                    result.SetPixel(dx + x, dy + y, (byte)c);
                }
            }
        }
    }
}