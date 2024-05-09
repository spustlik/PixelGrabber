using System;
using WpfGrabber.Data;

namespace WpfGrabber
{
    public class ByteBitmapRgba
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public uint[] Data { get; private set; }
        public ByteBitmapRgba(int width, int height)
        {
            this.Width = width;
            this.Height = height;
            Data = new uint[width * height];
        }


        public byte[] ToBytes()
        {
            var bytes = new byte[Data.Length * sizeof(uint)];
            Array.Copy(Data, bytes, Data.Length);
            return bytes;
        }

        public void SetPixel(int x, int y, uint value)
        {
            if (x >= Width || y >= Height)
                return;
            Data[x + y * Width] = value;
        }
        public uint GetPixel(int x, int y)
        {
            if (x >= Width || y >= Height)
                return 0x00000000;
            return Data[x + y * Width];
        }

    }
}