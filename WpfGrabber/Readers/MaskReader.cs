using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Xaml;

namespace WpfGrabber.Readers
{
    public class MaskReader
    {

        private void ProcessMask(ByteBitmapRgba src, ByteBitmapRgba dst, int itemHeight, int itemsCount)
        {
            var srcposY = 0;
            var dstposY = 0;
            for (int i = 0; i < itemsCount; i++)
            {
                for (int y = 0; y < itemHeight; y++)
                {
                    for (int x = 0; x < src.Width; x++)
                    {
                        var sbp = src.GetPixel(x, y + srcposY);
                        var sbm = src.GetPixel(x, srcposY + itemHeight + y);
                        if ((sbm & 0xFFFFFF) == 0)
                        {
                            dst.SetPixel(x, dstposY + y, sbp);
                        }
                    }
                }
                srcposY += itemHeight * 2;
                dstposY += itemHeight;
            }
        }

        public WriteableBitmap ProcessMask(BitmapImage src, int itemHeight, int itemsCount)
        {
            int width = (int)src.Width;
            int height = (int)src.Height;
            var srcpixels = new uint[width * height];
            src.CopyPixels(srcpixels, stride: width * 4, 0);

            //var src = new ByteBitmapRgba()
            var dstpixels = new uint[width * height];
            var srcposY = 0;
            var dstposY = 0;
            for (int i = 0; i < itemsCount; i++)
            {
                for (int y = 0; y < itemHeight; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        var sb = srcpixels[(srcposY + y) * width + x];
                        var sbm = srcpixels[(srcposY + itemHeight + y) * width + x];
                        if ((sbm & 0xFFFFFF) == 0)
                        {
                            dstpixels[(dstposY + y) * width + x] = sb;
                        }
                    }
                }
                srcposY += itemHeight * 2;
                dstposY += itemHeight;
            }
            var bmp = new WriteableBitmap(width, height, 96, 96, PixelFormats.Pbgra32, null);
            bmp.WritePixels(new Int32Rect(0, 0, width, height), dstpixels, width*4, 0);
            return bmp;
        }
    }
}
