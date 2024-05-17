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
            if (src.Format == PixelFormats.Indexed8)
                return FromIndexed(src);
            if (src.Format == PixelFormats.Bgr32)
                return FromBGRA(src);
            throw new NotImplementedException($"Bitmap format {src.Format} not implemented");
        }

        private static ByteBitmapRgba FromBGRA(BitmapSource src)
        {
            int width = src.PixelWidth;
            int height = src.PixelHeight;
            var bytesPerPx = src.Format.BitsPerPixel / 8;
            var pixels = new uint[width * height];
            src.CopyPixels(pixels, stride: width * bytesPerPx, 0);
            var r = new ByteBitmapRgba(width, height);
            Array.Copy(pixels, r.Data, pixels.Length);
            return r;
        }

        private static ByteBitmapRgba FromIndexed(BitmapSource src)
        {
            if (src.Palette == null)
                throw new NotImplementedException($"Bitmap format {src.Format} without palette not implemented");
            if (src.Palette.Colors.Count != 256)
                throw new FormatException($"Palette should have 256 colors ({src.Palette.Colors.Count})");
            int width = src.PixelWidth;
            int height = src.PixelHeight;
            var r = new ByteBitmapRgba(width, height);
            var pixels = new byte[width * height];
            src.CopyPixels(pixels, stride: width, 0);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var b = pixels[x + y * width];
                    var cc = src.Palette.Colors[b];
                    var c = ByteColor.FromRgba(cc.R, cc.G, cc.B, cc.A);
                    r.SetPixel(x, y, c);
                }
            }
            return r;
        }
    }
}
