using System;
using System.Windows.Ink;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using WpfGrabber.Data;
using System.Runtime.InteropServices;

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
        public byte[] ToBytes()
        {
            var bytes = new byte[Data.Length * sizeof(uint)];
            Array.Copy(Data, bytes, Data.Length);
            return bytes;
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

        public delegate uint Colorizer(uint data, uint orig);

        public void DrawBitmap(ByteBitmap8Bit src, int posX, int posY, Colorizer colorizer = null)
        {
            DrawBitmapByFunc(
                src.Width,
                src.Height,
                posX,
                posY,
                (x, y) => src.GetPixel(x, y),
                colorizer);
        }

        public void DrawBitmap(BitBitmap src, int posX, int posY, Colorizer colorizer = null)
        {
            DrawBitmapByFunc(
                src.WidthPixels,
                src.Height,
                posX,
                posY,
                (x, y) => src.GetPixel(x, y) ? (byte)1 : (byte)0,
                colorizer);
        }

        public void DrawBitmap(ByteBitmapRgba src, int posX, int posY, Colorizer colorizer)
        {
            if (colorizer == null)
                colorizer = GetColorCopy;
            DrawBitmapFunctioned( src.Width, src.Height,
                (x, y) => src.GetPixel(x,y),
                (x, y, pixel) => {
                    uint orig = GetPixel(posX + x, posY + y);
                    uint c = colorizer(pixel, orig);
                    SetPixel(posX + x, posY + y, c);
                });
        }

        public void DrawBitmapByFunc(
            int width,
            int height,
            int posX,
            int posY,
            Func<int, int, uint> getPixel,
            Colorizer colorizer = null)
        {
            if (colorizer == null)
            {
                colorizer = GetColor01Gray;
            }
            DrawBitmapFunctioned(width, height, 
                getPixel,
                (x, y, b) =>{
                    uint orig = GetPixel(posX + x, posY + y);
                    uint c = colorizer(b, orig);
                    SetPixel(posX + x, posY + y, c);
                });
        }

        public void DrawBitmapFunctioned(
            int width,
            int height,
            Func<int, int, uint> getPixel,
            Action<int, int, uint> setPixel)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var b = getPixel(x, y);
                    setPixel(x, y, b);
                }
            }
        }

        public static ByteBitmapRgba FromBitmap(ByteBitmap8Bit src, Colorizer colorizer = null)
        {
            var result = new ByteBitmapRgba(src.Width, src.Height);
            result.DrawBitmap(src, 0, 0, colorizer);
            return result;
        }


        public static uint GetColorCopy(uint c, uint orig)
        {
            return c;
        }

        public static uint GetColor01Gray(uint b, uint orig)
        {
            switch (b)
            {
                case 0: return 0xFFFFFFFF;
                case 1: return 0;
                default: return GetColorGray(b, orig);
            }
        }
        public static uint GetColor10Gray(uint b, uint orig)
        {
            switch (b)
            {
                case 0: return 0;
                case 1: return 0xFFFFFFFF;
                default: return GetColorGray(b, orig);
            }
        }

        public static uint GetColorBlack(uint b, uint orig)
        {
            if (b == 0) return 0;
            return 0xFF000000;
        }
        public static uint GetColorWhite(uint b, uint orig)
        {
            if (b == 0) return 0;
            return 0xFFFFFFFF;
        }
        public static uint GetColorGray(uint d, uint orig)
        {
            return 0xff000000 | d | d >> 8 | d >> 16;
        }

    }
}