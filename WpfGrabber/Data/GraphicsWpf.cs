﻿using System;
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
            if (src.Format == PixelFormats.Bgr32
                || src.Format == PixelFormats.Bgra32
                || src.Format == PixelFormats.Pbgra32
                )
                return FromBGRA(src);
            throw new NotImplementedException($"Bitmap format {src.Format} not implemented");
        }

        private static ByteBitmapRgba FromBGRA(BitmapSource src)
        {
            if (src.Format == PixelFormats.Bgra32)
            {
                src = new FormatConvertedBitmap(src, PixelFormats.Pbgra32, src.Palette, 0);
            }
            int width = src.PixelWidth;
            int height = src.PixelHeight;
            var bytesPerPx = src.Format.BitsPerPixel / 8;
            var pixels = new uint[width * height];
            src.CopyPixels(pixels, stride: width * bytesPerPx, 0);
            var r = new ByteBitmapRgba(width, height, pixels);

            /*
             * probably not properly working -- see below
            if (src.Format == PixelFormats.Bgra32)
            {
                // ??? premultiplied alpha... it seems good
                PremultiplyAlpha(r);
            }*/

            return r;
        }

        private static void PremultiplyAlpha(this ByteBitmapRgba bmp)
        {
            for (int i = 0; i < bmp.Data.Length; i++)
            {
                var c = ByteColor.FromUint(bmp.Data[i]);
                var a = c.A / 255;
                c = ByteColor.FromRgba((byte)(a * c.R), (byte)(c.G * a), (byte)(c.B * a), c.A);
                bmp.Data[i] = c;
            }
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

        public static void __DrawBitmap(this WriteableBitmap dest, BitmapSource src, int x, int y)
        {
            if (src.Format != dest.Format)
                throw new ArgumentException($"Formats of src and dest must be same. {src.Format}!={dest.Format}");
            var bytesPerPx = dest.Format.BitsPerPixel / 8;
            var stride = dest.PixelWidth * bytesPerPx;
            var pixels = GetSrcPixels(src);
            dest.WritePixels(new Int32Rect(x, y, src.PixelWidth, src.PixelHeight), pixels, stride, 0);
        }

        private static uint[] GetSrcPixels(BitmapSource src)
        {
            var bytesPerPx = src.Format.BitsPerPixel / 8;
            var pixels = new uint[src.PixelWidth * src.PixelHeight];
            var stride = src.PixelWidth * bytesPerPx;
            src.CopyPixels(pixels, stride, 0);
            return pixels;
        }
    }
}
