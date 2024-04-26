using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace WpfGrabber
{
    public static class ObjectExtensions
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

        public static int FindIndex<T>(this IEnumerable<T> data, Func<T, bool> predicate)
        {
            int i = 0;
            foreach (var item in data)
            {
                if (predicate(item))
                    return i;
                i++;
            }
            return -1;
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

        public static byte[] GetRange(this byte[] data, int start, int count)
        {
            var result = new byte[count];
            for (int i = 0; i < count; i++)
            {
                if (start + i < data.Length)
                    result[i] = data[start + i];
            }
            return result;
        }

        public static string ToHex(this byte[] data, string separator = " ")
        {
            return String.Join(separator, data.Select(x => x.ToString("X2")));
        }


    }
}
