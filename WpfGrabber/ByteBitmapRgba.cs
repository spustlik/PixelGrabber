using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfGrabber
{
    public class ByteBitmapRgba
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        //private uint[,] data;
        public uint[] Data { get; private set; }
        public ByteBitmapRgba(int width, int height)
        {
            this.Width = width;
            this.Height = height;
            //data = new uint[width, height];
            Data = new uint[width * height];
        }

        public BitmapSource ToBitmapSource()
        {
            var bmp = BitmapSource.Create(Width, Height,
                96, 96, PixelFormats.Pbgra32, null, Data, Width * sizeof(uint));
            return bmp;
        }

        public void SetPixel(int x, int y, uint value)
        {
            //data[x,y] = value;
            Data[x + y * Width] = value;
        }
        public uint GetPixel(int x, int y)
        {
            //return data[x,y];
            return Data[x + y * Width];
        }
    }
}