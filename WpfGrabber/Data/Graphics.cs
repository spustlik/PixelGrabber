using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace WpfGrabber.Data
{

    public static class Graphics
    {
        #region Line
        private static void Swap<T>(ref T lhs, ref T rhs) { (lhs, rhs) = (rhs, lhs); }

        public delegate bool PlotDelegate(int x, int y);
        public static void Line(int x0, int y0, int x1, int y1, PlotDelegate plot)
        {
            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep) { Swap(ref x0, ref y0); Swap(ref x1, ref y1); }
            if (x0 > x1) { Swap(ref x0, ref x1); Swap(ref y0, ref y1); }
            int dX = (x1 - x0), dY = Math.Abs(y1 - y0), err = (dX / 2), ystep = (y0 < y1 ? 1 : -1), y = y0;

            for (int x = x0; x <= x1; ++x)
            {
                if (!(steep ? plot(y, x) : plot(x, y)))
                    return;
                err -= dY;
                if (err < 0) { y += ystep; err += dX; }
            }
        }

        private static bool Plot8(int x, int y, ByteBitmap8Bit bmp, byte pixel)
        {
            if (x < 0 || y < 0 || x > bmp.Width || y > bmp.Height)
                return false;
            bmp.SetPixel(x, y, pixel);
            return true;
        }

        public static void DrawLine(this ByteBitmap8Bit bmp, int x0, int y0, int x1, int y1, byte pixel = 2)
        {
            Line(x0, y0, x1, y1, (x, y) => Plot8(x, y, bmp, pixel));
        }


        #endregion

        #region rect
        public static void Rect(int x0, int y0, int x1, int y1, PlotDelegate plot)
        {
            Line(x0, y0, x1, y0, plot);
            Line(x1, y0, x1, y1, plot);
            Line(x1, y1, x0, y1, plot);
            Line(x0, y1, x0, y0, plot);
        }
        public static void FillRect(int x0, int y0, int x1, int y1, PlotDelegate plot)
        {
            if (x0 > x1) Swap(ref x0, ref x1);
            if (y0 > y1) Swap(ref y0, ref y1);
            for (int y = y0; y <= y1; y++)
            {
                for (int x = x0; x <= x1; x++)
                {
                    plot(x, y);
                }
            }
        }

        public static void DrawRect(this ByteBitmap8Bit bmp, int x0, int y0, int x1, int y1, byte pixel = 2)
        {
            Rect(x0, y0, x1, y1, (x, y) => Plot8(x, y, bmp, pixel));
        }

        public static void DrawFillRect(this ByteBitmap8Bit bmp, int x0, int y0, int x1, int y1, byte pixel = 2)
        {
            FillRect(x0, y0, x1, y1, (x, y) => Plot8(x, y, bmp, pixel));
        }


        #endregion

        #region font

        public static void PutText(FontData font, 
            int x, int y, string text,
            Action<int, int, BitBitmap> drawChar)
        {
            for (int i = 0; i < text.Length; i++)
            {
                var chr = font.GetCharBmp(text[i]);
                if (chr != null)
                {
                    drawChar(x, y, chr);
                    x += chr.WidthPixels;
                }
                else
                {
                    //unknown char
                    x += 6;
                }
                x += font.SpaceX;
            }
        }
        public static void DrawText(this ByteBitmapRgba target, FontData font, int posx, int posy, string text, Colorizer colorizer)
        {
            PutText(font, posx, posy, text, (x, y, chr) =>
            {
                target.DrawBitmap(chr, x, y, colorizer);
            });
        }
        public static void DrawText(this ByteBitmap8Bit target, FontData font, int posx, int posy, string text, byte pixel)
        {
            PutText(font, posx, posy, text, (x, y, chr) =>
            {
                target.DrawBitmap(chr, x, y, pixel);
            });
        }


        #endregion

        #region Draw bitmap

        public static void DrawBitmapFunctioned(
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
        public static void DrawBitmapByFunc(
            ByteBitmapRgba dest,
            int width,
            int height,
            int posX,
            int posY,
            Func<int, int, uint> getPixel,
            Colorizer colorizer)
        {
            DrawBitmapFunctioned(width, height,
                getPixel,
                (x, y, b) =>
                {
                    uint orig = dest.GetPixel(posX + x, posY + y);
                    uint c = colorizer(b, orig);
                    dest.SetPixel(posX + x, posY + y, c);
                });
        }


        public static void DrawBitmap(this ByteBitmapRgba dest, BitBitmap src, int posX, int posY, Colorizer colorizer)
        {
            DrawBitmapByFunc(
                dest,
                src.WidthPixels,
                src.Height,
                posX,
                posY,
                (x, y) => src.GetPixel(x, y) ? (byte)1 : (byte)0,
                colorizer);
        }


        public static void DrawBitmap(this ByteBitmapRgba dest, ByteBitmap8Bit src, int posX, int posY, Colorizer colorizer)
        {
            DrawBitmapByFunc(
                dest,
                src.Width,
                src.Height,
                posX,
                posY,
                (x, y) => src.GetPixel(x, y),
                colorizer);
        }
        public static void DrawBitmap(this ByteBitmapRgba dest, ByteBitmapRgba src, int posX, int posY, Colorizer colorizer)
        {
            DrawBitmapFunctioned(src.Width, src.Height,
                (x, y) => src.GetPixel(x, y),
                (x, y, pixel) =>
                {
                    uint orig = dest.GetPixel(posX + x, posY + y);
                    uint c = colorizer(pixel, orig);
                    dest.SetPixel(posX + x, posY + y, c);
                });
        }

        public static void DrawBitmap(this ByteBitmap8Bit dest, BitBitmap src, int posX, int posY, 
            byte pixel)
        {
            DrawBitmapFunctioned(src.WidthPixels, src.Height,
                (x, y) => src.GetPixel(x, y) ? (uint)1 : 2, //1=no font pixel
                (x, y, px) =>
                {
                    if(px==1)
                        dest.SetPixel(posX + x, posY + y, pixel);
                });
        }
        #endregion


        public static ByteBitmapRgba ToRgba(this ByteBitmap8Bit src, Colorizer colorizer)
        {
            var result = new ByteBitmapRgba(src.Width, src.Height);
            result.DrawBitmap(src, 0, 0, colorizer);
            return result;
        }

    }
}
