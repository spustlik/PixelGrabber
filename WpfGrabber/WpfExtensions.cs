using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
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
            using(var s = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                dest.BeginInit();
                dest.CacheOption = BitmapCacheOption.OnLoad;
                dest.StreamSource = s;
                dest.EndInit();
            }
        }

    }
}
