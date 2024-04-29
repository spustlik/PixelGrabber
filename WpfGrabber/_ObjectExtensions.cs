using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;

namespace WpfGrabber
{
    public static class ObjectExtensions
    {

        public static void WriteBytes(this Stream s, byte[] data)
        {
            s.Write(data, 0, data.Length);
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
            if (serviceProvider != null)
            {
                var svc = serviceProvider.FindService<T>();
                if (svc != default)
                    return svc;
            }
            throw new ApplicationException($"Service {typeof(T).Name} not found");
        }

        public static T FindService<T>(this IServiceProvider serviceProvider) where T : class
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
