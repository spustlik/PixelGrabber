using System;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace WpfGrabber.Data
{
    public static class GraphicsWpf
    {
        public static BitmapSource ToBitmapSource(this ByteBitmapRgba src, PixelFormat? format = null)
        {
            var bmp = BitmapSource.Create(src.Width, src.Height,
                96, 96, format ?? PixelFormats.Pbgra32, null, src.Data, src.Width * sizeof(uint));
            return bmp;
        }

        public static WriteableBitmap ToWriteableBitmap(this ByteBitmapRgba src, PixelFormat? format = null)
        {
            var bmp = new WriteableBitmap(src.Width, src.Height, 96, 96, format ?? PixelFormats.Pbgra32, null);
            bmp.WritePixels(new Int32Rect(0, 0, src.Width, src.Height), src.Data, src.Width * 4, 0);
            return bmp;
        }

        public static ByteBitmapRgba ToRgba(this BitmapSource src)
        {
            int width = (int)src.Width;
            int height = (int)src.Height;
            var pixels = new uint[width * height];
            src.CopyPixels(pixels, stride: width * 4, 0);
            var r = new ByteBitmapRgba(width, height);
            //r.Data = pixels;
            Array.Copy(pixels, r.Data, pixels.Length);
            return r;
        }
    }
}
