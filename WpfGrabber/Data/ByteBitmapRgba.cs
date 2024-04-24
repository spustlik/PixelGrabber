using System;
using System.Windows.Ink;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

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
            //data = new uint[width, height];
            Data = new uint[width * height];
        }

        public BitmapSource ToBitmapSource()
        {
            var bmp = BitmapSource.Create(Width, Height,
                96, 96, PixelFormats.Pbgra32, null, Data, Width * sizeof(uint));
            return bmp;
        }

        public WriteableBitmap ToWriteableBitmap()
        {
            var bmp = new WriteableBitmap(Width, Height, 96, 96, PixelFormats.Pbgra32, null);
            bmp.WritePixels(new Int32Rect(0, 0, Width, Height), Data, Width * 4, 0);
            return bmp;
        }

        public static ByteBitmapRgba FromBitmapSource(BitmapSource src)
        {
            int width = (int)src.Width;
            int height = (int)src.Height;
            var pixels = new uint[width * height];
            src.CopyPixels(pixels, stride: width * 4, 0);
            var r = new ByteBitmapRgba(width, height);
            r.Data = pixels;
            return r;
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

        public void DrawBitmap(ByteBitmap8Bit src, int posX,int posY, Func<byte, uint> colorizer = null)
        {
            if (colorizer == null)
            {
                colorizer = GetColor01Gray;
            }
            for (int y = 0; y < src.Width; y++)
            {
                for (int x = 0; x < src.Width; x++)
                {
                    var b = src.GetPixel(x, y);
                    var c = colorizer(b);
                    SetPixel(posX + x, posY + y, c);
                }
            }
        }

        public static uint GetColor01Gray(byte b)
        {
            switch (b)
            {
                case 0: return 0xFFFFFFFF;
                case 1: return 0;
                default: return GetColorGray(b);
            }
        }

        public static uint GetColorBlack(byte b)
        {
            if (b == 0) return 0;
            return 0xFF000000;
        }
        public static uint GetColorWhite(byte b)
        {
            if (b == 0) return 0;
            return 0xFFFFFFFF;
        }
        public static uint GetColorGray(byte b)
        {
            var d = (uint)b;
            return 0xff000000 | d | d >> 8 | d >> 16;
        }

    }
}