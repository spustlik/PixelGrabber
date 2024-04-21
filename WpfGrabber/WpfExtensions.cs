using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
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
            using (var s = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                dest.BeginInit();
                dest.CacheOption = BitmapCacheOption.OnLoad;
                dest.StreamSource = s;
                dest.EndInit();
            }
        }

        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> data, bool clear = false)
        {
            if (clear)
                collection.Clear();
            foreach (var item in data)
            {
                collection.Add(item);
            }
        }

        public static T GetService<T>(this IServiceProvider serviceProvider) where T : class
        {
            return serviceProvider.GetService(typeof(T)) as T;
        }

        public static void AddService<T>(this IServiceContainer serviceContainer, T service) 
            where T : class
        {
            serviceContainer.AddService(typeof(T), service);
        }

        public static int FindIndex<T>(this IEnumerable<T> data, Func<T,bool> predicate) 
        {
            int i = 0;
            foreach (var item in data)
            {
                if (predicate(item))
                    return i;
            }
            return -1;
        }
    }
}
