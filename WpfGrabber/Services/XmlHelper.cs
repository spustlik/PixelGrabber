using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace WpfGrabber.Services
{
    public static class XmlHelper
    {
        public static void SerializeToFile<T>(string fileName, T data) where T : class
        {
            var ser = new XmlSerializer(typeof(T));
            using (var st = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                using (var xw = XmlWriter.Create(st, new XmlWriterSettings() { Indent = true }))
                {
                    ser.Serialize(xw, data);
                }
            }
        }

        public static T SerializeFromFile<T>(string fileName) where T : class
        {
            var ser = new XmlSerializer(typeof(T));
            using (var st = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var xr = XmlReader.Create(st))
                {
                    var data = ser.Deserialize(xr) as T;
                    return data;
                }
            }
        }

        public static void SaveToFile<T>(string fileName, T data) where T : class
        {
            var root = new XElement(typeof(T).Name);
            SaveProperties(root, data);
            root.Save(fileName);
        }

        public static T LoadFromFile<T>(string fileName) where T : class, new()
        {
            if (!File.Exists(fileName))
                return default;
            var doc = XDocument.Load(fileName);
            var data = new T();
            LoadProperties(doc.Root, data);
            return data;
        }

        public static object GetAttrValue(this XElement e, string name, Type t)
        {
            var at = e.Attribute(name);
            if (at == null)
                return null;
            var s = at.Value;
            return ConvertFromString(s, t);
        }

        private static object ConvertFromString(string s, Type t)
        {
            if (t == typeof(int)) return int.Parse(s);
            if (t == typeof(bool)) return bool.Parse(s);
            if (t == typeof(string)) return s;
            throw new NotSupportedException($"Type {t} is not supported");
        }
        private static string ConvertToString(object v, Type t)
        {
            if (t == typeof(int)) return v.ToString();
            if (t == typeof(bool)) return ((bool)v) ? "true" : "false";
            if (t == typeof(string)) return v == null ? null : v.ToString();
            throw new NotSupportedException($"Type {t} is not supported");
        }
        
        public static void LoadProperties(this XElement e, object o)
        {
            var props = o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var p in props)
            {
                if (p.GetCustomAttribute<XmlIgnoreAttribute>() != null)
                    continue;
                var v = e.GetAttrValue(p.Name, p.PropertyType);
                if (v != null)
                    p.SetValue(o, v);
            }
        }

        public static void SaveProperties(this XElement e, object o)
        {
            var props = o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var p in props)
            {
                if (p.GetCustomAttribute<XmlIgnoreAttribute>() != null)
                    continue;
                var s = ConvertToString(p.GetValue(o), p.PropertyType);
                e.SetAttributeValue(p.Name, s);
            }

        }

        public static void LoadColection<T>(this XElement parent, string collectionElementName, IList<T> collection) where T:class,new()
        {
            LoadList(parent, collectionElementName, (e) =>
            {
                var o = new T();
                LoadProperties(e, o);
                collection.Add(o);
            });
        }
        public static void SaveCollection<T>(this XElement parent, string collectionElementName, IEnumerable<T> collection) where T : class
        {
            SaveList(parent, collectionElementName, collection, (e, item) =>
            {
                SaveProperties(e, item);
            });
        }

        public static T[] LoadArray<T>(this XElement parent, string collectionElementName)
        {
            var data = new List<T>();
            LoadList(parent, collectionElementName, (e) =>
            {
                var v = (T)ConvertFromString(e.Value, typeof(T));
                data.Add(v);
            });
            return data.ToArray();
        }
        public static void SaveArray<T>(this XElement parent, string collectionElementName, T[] data)
        {
            SaveList(parent, collectionElementName, data, (e, item) =>
            {
                e.Value = ConvertToString(item, typeof(T));
            });
        }
        public static void LoadList(XElement parent, string collectionElementName, Action<XElement> adder)
        {
            foreach (var ele in parent.Elements(collectionElementName))
            {
                adder(ele);
            }
        }
        public static void SaveList<T>(XElement parent, string collectionElementName, IEnumerable<T> data, Action<XElement, T> saver)
        {
            foreach (var item in data)
            {
                var t = new XElement(collectionElementName);
                parent.Add(t);
                saver(t, item);
            }
        }


    }
}
