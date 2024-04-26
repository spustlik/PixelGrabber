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
    // vzdy dva byte DATA,MASK, nejaka vyska a (sirka v bytech)
    //varianta 2 - vzdy WidthBytes Data, pak WidthBytes Mask
    //varianta 3 - vzdy cely obr (WidthBuytes*Height) Data, cely obr mask
    public class MaskReader
    {
        public byte[] Data { get; }
        public int DataLength =>Data.Length;
        public int Position { get; set; }
        public int WidthBytes { get; set; } = 3;
        public int Height { get; set; } = 16;
        public bool FlipX { get;set; }
        public bool FlipY { get;set; }
        public MaskReader(byte[] data)
        {
            Data = data;
        }

        protected byte ReadByte()
        {
            if (Position >= DataLength)
                return 0;
            return Data[Position++];
        }

        public ByteBitmap8Bit Read()
        {
            var result = new ByteBitmap8Bit(WidthBytes * 8, Height);
            for (var y = 0; y < Height; y++)
            {
                var posY = y;
                if (FlipY)
                    posY = Height - y;
                for (int x = 0; x < WidthBytes; x++)
                {
                    var data = ReadByte();
                    if (FlipX)
                        data = BitReader.GetFlippedX(data);
                    var mask = ReadByte();
                    if (FlipX)
                        mask = BitReader.GetFlippedX(mask);
                    for(var i = 0; i < 8; i++)
                    {
                       byte b = 0;
                        if ((mask & 1) != 0)
                        {
                            if ((data & 1) == 0)
                                b = 1;
                            else
                                b = 2;
                        }
                        result.SetPixel(x * 8 + i, posY, b);
                        data = (byte)(data >> 1);
                        mask = (byte)(mask >> 1);
                    }
                }
            }
            return result;
        }

        public void Process(BitmapImage source, int itemHeight, int itemsCount)
        {
            var src = ByteBitmapRgba.FromBitmapSource(source);
            var dst = new ByteBitmapRgba(src.Width, src.Height);
            ProcessMask(src, dst, itemHeight, itemsCount);
            dst.ToBitmapSource();
        }

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
