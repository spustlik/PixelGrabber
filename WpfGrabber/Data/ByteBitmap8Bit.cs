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

        /// <summary>
        /// sets pixel value - 0=white, 1=transparent, 2=black
        /// </summary>
        public void SetPixel(int x, int y, byte value)
        {
            if (x >= Width || y >= Height)
                return;
            Data[x, y] = value;
        }
        public byte GetPixel(int x, int y)
        {
            if (x >= Width || y >= Height)
                return 0;
            return Data[x, y];
        }


    }
}
