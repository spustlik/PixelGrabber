using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace WpfGrabber
{
    public static class WpfExtensions
    {
        public static void SaveToPngFile(this BitmapSource source, string fileName)
        {
            var enc = new PngBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(source));
            using (var s = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
            {
                enc.Save(s);
            }
        }

        public static void LoadFromFile(this BitmapImage dest, string fileName)
        {
            using (var s = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                LoadFromStream(dest, s);
            }
        }

        public static void LoadFromStream(this BitmapImage dest, Stream s)
        {
            dest.BeginInit();
            dest.CacheOption = BitmapCacheOption.OnLoad;
            dest.StreamSource = s;
            dest.EndInit();
        }

        public static int GetFirstValid(this FrameworkElement e, params double[] n)
        {
            foreach (var v in n)
            {
                if (double.IsNaN(v))
                    continue;
                if (v == 0)
                    continue;
                return (int)v;
            }
            return 0;
        }

        /*
        public static BitmapSource CaptureVisual(this Visual target, double dpiX, double dpiY)
        {
            var bounds = VisualTreeHelper.GetDescendantBounds(target);
            var rtb = new RenderTargetBitmap((int)(bounds.Width * dpiX / 96.0),
                                             (int)(bounds.Height * dpiY / 96.0),
                                             dpiX,
                                             dpiY,
                                             PixelFormats.Default);
            var dv = new DrawingVisual();
            using (DrawingContext ctx = dv.RenderOpen())
            {
                var vb = new VisualBrush(target);
                ctx.DrawRectangle(vb, null, new System.Windows.Rect(new Point(), bounds.Size));
            }
            rtb.Render(dv);
            return rtb;
        }
        */

    }
}
