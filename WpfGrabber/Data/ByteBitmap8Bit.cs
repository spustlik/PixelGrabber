namespace WpfGrabber
{
    public class ByteBitmap8Bit
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public byte[,] Data { get; private set; }

        public ByteBitmap8Bit(int w, int h)
        {
            Width = w;
            Height = h;
            Data = new byte[w, h];
        }

        public void SetPixel(int x, int y, byte value)
        {
            if (x >= Width || y >= Height)
                return;
            Data[x, y] = value;
        }
        public byte GetPixel(int x, int y)
        {
            return Data[x, y];
        }

        public void PutToBitmap(ByteBitmapRgba bmp, int posX, int posY)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    var b = GetPixel(x, y);
                    switch (b)
                    {
                        case 0: 
                            bmp.SetPixel(x + posX, y + posY, 0xFFFFFFFF);
                            break;
                        case 1:
                            bmp.SetPixel(x + posX, y + posY, 0);
                            break;
                        default:
                            var d = (uint)b; 
                            bmp.SetPixel(x + posX, y + posY, 0xff000000 | d | d>>8 | d>>16);
                            break;
                    }
                }
            }
        }
    }
}
