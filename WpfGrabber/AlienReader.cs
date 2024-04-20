using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfGrabber
{
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
        public AlienImage ReadMaskedImage()
        {
            var w = Read();
            var h = Read();
            var result = new AlienImage(w*8, h);
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

    public class AlienImage
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public byte[,] Data { get; private set; }

        public AlienImage(int w, int h)
        {
            Width = w;
            Height = h;
            Data = new byte[w, h];
        }

        public void SetPixel(int x, int y, byte value)
        {
            Data[x, y] = value;
        }
        public byte GetPixel(int x, int y)
        {
            return Data[x, y];
        }

        public void PutToBitmap(ByteBitmap bmp, int posX, int posY)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    var b = GetPixel(x, y);
                    if (b != 0)
                    {
                        bmp.SetPixel(x + posX, y + posY, b == 1 ? 0xFF000000 : 0xFFFFFFFF);
                    }
                }
            }
        }
    }
}
