using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

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
            {
                collection.Replace(data);
                return;
            }
            if (data == null)
                return;

            foreach (var item in data)
            {
                collection.Add(item);
            }
        }
        public static void RemoveAll<T>(this ObservableCollection<T> collection, Func<T, bool> predicate)
        {
            if (predicate == null)
                return;
            for (int i = collection.Count - 1; i >= 0; i--)
            {
                if (predicate(collection[i]))
                {
                    collection.RemoveAt(i);
                }
            }
        }
        public static void Replace<T>(this ObservableCollection<T> collection, IEnumerable<T> data)
        {
            if (data == null)
            {
                collection.Clear();
                return;
            }
            var pos = 0;
            foreach (var item in data.ToArray())//data can be linq to same collection, so we need to materialize it
            {
                var i = collection.IndexOf(item);
                if (i >= 0)
                {
                    if (i != pos)
                    {
                        //uz tam je, takze ho presuneme na pos
                        collection.Move(i, pos);
                    }
                    pos++;
                    continue;
                }
                //neni tam, takze ho dame na pos, tim se dalsi posouvaji dale
                collection.Insert(pos, item);
                pos++;
            }
            //nektere polozky jsou navic, takze je odstranime
            for (int i = collection.Count - 1; i >= pos; i--)
            {
                collection.RemoveAt(i);
            }
        }

        public static void Push<T>(this ObservableCollection<T> collection, T item)
        {
            collection.Add(item);
        }
        public static T Pop<T>(this ObservableCollection<T> collection)
        {
            if (collection.Count == 0)
                return default;
            var result = collection.Last();
            collection.RemoveAt(collection.Count - 1);
            return result;
        }
        public static T NextItem<T>(this IEnumerable<T> values, Func<T, bool> findCurrent)
        {
            var i = values.FindIndex(findCurrent);
            if (i < 0 || i + 1 > values.Count())
                return default;
            return values.ElementAt(i + 1);
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

        public static StringBuilder AppendIndented(this StringBuilder sb, string indent, params string[] lines)
        {
            if (lines.Length == 1)
            {
                lines = lines[0].Split('\n');
            }
            foreach (var line in lines)
            {
                sb.Append(indent)
                    .Append(line.TrimEnd())
                    .AppendLine();
            }
            return sb;
        }

    }
}
