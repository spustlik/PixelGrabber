using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows.Media;
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
            if (t == typeof(int)) return int.Parse(s, CultureInfo.InvariantCulture);
            if (t == typeof(bool)) return bool.Parse(s);
            if (t == typeof(string)) return s;
            if (t == typeof(double)) return double.Parse(s, CultureInfo.InvariantCulture);
            if (t.IsEnum) return Enum.Parse(t, s);
            throw new NotSupportedException($"Type {t} is not supported");
        }
        private static string ConvertToString(object v, Type t)
        {
            if (t == typeof(int)) return Convert.ToString(v, CultureInfo.InvariantCulture);
            if (t == typeof(bool)) return ((bool)v) ? "true" : "false";
            if (t == typeof(string)) return v?.ToString();
            if (t == typeof(double)) return Convert.ToString(v, CultureInfo.InvariantCulture);
            if (t.IsEnum) return ((Enum)v).ToString();
            throw new NotSupportedException($"Type {t} is not supported");
        }

        public static void LoadProperties(this XElement e, object o, params string[] ignore)
        {
            var props = o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var p in props)
            {
                if (IsIgnored(p, ignore))
                    continue;
                var v = e.GetAttrValue(p.Name, p.PropertyType);
                if (v != null)
                    p.SetValue(o, v);
            }
        }

        public static void SaveProperties(this XElement e, object o, params string[] ignore)
        {
            var props = o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var p in props)
            {
                if (IsIgnored(p, ignore))
                    continue;
                var s = ConvertToString(p.GetValue(o), p.PropertyType);
                e.SetAttributeValue(p.Name, s);
            }
        }
        private static bool IsIgnored(PropertyInfo p, string[] ignore)
        {
            if (p.GetCustomAttribute<XmlIgnoreAttribute>() != null)
                return true;
            if (ignore.Contains(p.Name))
                return true;
            return false;
        }

        /*
        public static void SaveToFile<T>(string fileName, T data) where T : class
        {
            var root = new XElement(typeof(T).Name);
            SavePropertiesRec(root, data, recursive: true, simple: false);
            root.Save(fileName);
        }

        public static T LoadFromFile<T>(string fileName) where T : class, new()
        {
            if (!File.Exists(fileName))
                return default;
            var doc = XDocument.Load(fileName);
            var data = new T();
            LoadPropertiesRec(doc.Root, data, recursive: true, simple: false);
            return data;
        }



        public static void LoadPropertiesRec(this XElement e, object o, bool recursive, bool simple)
        {
            var props = o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var p in props)
            {
                if (p.GetCustomAttribute<XmlIgnoreAttribute>() != null)
                    continue;
                if (GetListWrapper(p, o, out var listWrapper))
                {
                    if (simple)
                        continue;
                    var data = new List<object>();
                    if (listWrapper.ListType.IsArray)
                    {
                        //simple

                    }
                        
                    foreach (var ele in e.Elements(listWrapper.ElementName))
                    {
                        var item = listWrapper.CreateItem();
                        LoadPropertiesRec(ele, item, recursive, simple);
                    }
                    listWrapper.SetItems(data);
                    continue;
                }
                if (p.PropertyType.IsPrimitive)
                {
                    var v = e.GetAttrValue(p.Name, p.PropertyType);
                    if (v != null)
                        p.SetValue(o, v);
                    continue;
                }
                throw new NotImplementedException($"Loading property {p.Name} of type {p.PropertyType.Name}");
            }
        }
        public static void SavePropertiesRec(this XElement e, object o, bool recursive, bool simple)
        {
            var props = o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var p in props)
            {
                if (p.GetCustomAttribute<XmlIgnoreAttribute>() != null)
                    continue;
                if (GetListWrapper(p, o, out var listWrapper))
                {
                    if (simple)
                        continue;
                    foreach (var item in listWrapper.GetItems())
                    {
                        var ele = new XElement(listWrapper.ElementName);
                        e.Add(ele);
                        SavePropertiesRec(ele, item, recursive, simple);
                    }
                    continue;
                }
                if (p.PropertyType.IsPrimitive)
                {
                    var s = ConvertToString(p.GetValue(o), p.PropertyType);
                    e.SetAttributeValue(p.Name, s);
                    continue;
                }
                throw new NotImplementedException($"Saving property {p.Name} of type {p.PropertyType.Name}");
            }
        }
        class ListWrapper
        {
            public string ElementName { get; private set; }
            public Type ItemType { get; private set; }
            public Type ListType { get; private set; }
            public Func<IEnumerable<object>> GetItems { get; set; }
            public Action<IEnumerable<object>> SetItems { get; set; }
            public Func<object> CreateItem { get; set; }

            public ListWrapper(string elementName, Type itemType, Type listType)
            {
                ElementName = elementName;
                ItemType = itemType;
                ListType = listType;
            }

        }

        private static bool GetListWrapper(PropertyInfo p, object o, out ListWrapper listWrapper)
        {
            listWrapper = null;
            var t = p.PropertyType;
            if (t.IsArray)
            {
                listWrapper = new ListWrapper(p.Name, t.GetElementType(), t)
                {
                    GetItems = () =>
                    {
                        var arr = p.GetValue(o) as Array;
                        if (arr != null)
                            return arr.Cast<object>();
                        return new object[0];

                    },
                    SetItems = (data) =>
                    {
                        var objects = data.ToArray();
                        var arr = Array.CreateInstance(t, objects.Length);
                        Array.Copy(objects, arr, objects.Length);
                        p.SetValue(o, arr);
                    }
                };
                return true;
            }
            if (t.IsPrimitive)
                return false;
            if (t.IsGenericType && t.GetGenericTypeDefinition().IsAssignableFrom(typeof(IList<>)))
            {
                listWrapper = new ListWrapper(p.Name, t.GenericTypeArguments[0], typeof(IList))
                {
                    GetItems = () =>
                    {
                        var list = p.GetValue(o) as IList;
                        return list.Cast<object>();
                    },
                    SetItems = (data) =>
                    {
                        var list = p.GetValue(o) as IList;
                        if (list == null)
                        {
                            list = Activator.CreateInstance(p.PropertyType) as IList;
                            if (list == null)
                                throw new InvalidOperationException($"Cannot create list property {p.Name} of type {p.PropertyType.Name}");
                        }
                        foreach (var item in data)
                        {
                            list.Add(item);
                        }
                    }
                };
                return true;
            }
            return false;
        }
        */
    }
}
