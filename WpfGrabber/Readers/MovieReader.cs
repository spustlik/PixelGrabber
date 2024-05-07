using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfGrabber.Readers
{
    //Movie game engine, but not complete - user must find sprite starts
    //height(16bit), data(h*WIDTH), mask reversed
    public class MovieReader
    {
        private DataReader rd;

        public MovieReader(DataReader reader)
        {
            this.rd = reader;
        }

        public bool FlipVertical { get; set; }
        public int Width { get; set; }
        public IEnumerable<ReaderImageResult> ReadImages()
        {
            while (!rd.IsEmpty)
            {
                var pos = rd.BytePosition;
                int w = rd.ReadByte();
                if (w == 0xc0)
                {
                    var unknown = rd.ReadByte(); //skip,  next is 0x8? or 0xC1
                    //6B3D: C0 00 C1 39 
                    w = rd.ReadByte();
                }

                int h = 0;
                var readmask = true;
                if ((w & 0b11000000) == 0b10000000)
                {
                    //highest bit(s?) can mean that there is mask & data, data otherwise
                    //0x81(w=2), 0x83 BOM (w=4), 0x82(0x98FF-man)
                    w = 1 + (byte)(w & 0b0111111);
                    h = rd.ReadByte();
                }
                else if ((w & 0b11000000) == 0b11000000)
                {
                    //0xC1, 0xC0
                    if (w == 0xC1)
                    {
                        w = 1 + (byte)(w & 0b0011111);
                        h = rd.ReadByte();
                    }
                    else
                    {
                        //???
                        // E3
                        //F1,FB,F8
                    }
                    readmask = false;
                }
                else
                {
                    w = Width;
                    h = rd.ReadByte();
                }
                var bmp = ReadBitmap(w, h, readmask);
                yield return new ReaderImageResult(bmp, pos, rd.BytePosition);
            }
        }

        private ByteBitmap8Bit ReadBitmap(int w, int h, bool readmask)
        {
            var datar = new DataReader(rd.ReadBytes(w * h), 0, flipX: true);
            var maskr = new DataReader(rd.ReadBytes(w * h), 0, flipX: false);
            var bmp = new ByteBitmap8Bit(w * 8, h);
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        var d = datar.ReadBit();
                        var m = readmask ? maskr.ReadBit() : false;
                        var ry = y;
                        if (FlipVertical)
                            ry = bmp.Height - y - 1;
                        //bmp.SetPixel(x * 8 + i, ry, (byte)(m ? 1 : 2));
                        bmp.SetPixel(x * 8 + i, ry, (byte)(m ? 1 : d ? 0 : 2));
                    }
                }
            }
            return bmp;
        }
    }
}
