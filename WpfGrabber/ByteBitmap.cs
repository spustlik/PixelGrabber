using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfGrabber
{
    public class ByteBitmap
    {
        private int width;
        private int height;
        //private uint[,] data;
        private uint[] data;
        public ByteBitmap(int width, int height)
        {
            this.width = width;
            this.height = height;
            //data = new uint[width, height];
            data = new uint[width * height];
        }

        public BitmapSource ToBitmapSource()
        {
            var bmp = BitmapSource.Create(width, height,
                96, 96, PixelFormats.Pbgra32, null, data, width * sizeof(uint));
            return bmp;
        }

        public void SetPixel(int x, int y, uint value)
        {
            //data[x,y] = value;
            data[x + y * width] = value;
        }
        public uint GetPixel(int x, int y)
        {
            //return data[x,y];
            return data[x + y * width];
        }
    }
}