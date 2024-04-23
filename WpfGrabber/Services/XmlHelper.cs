using System;
using System.Collections.Generic;
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
        public static void SaveToConfig<T>(string fileName, T data) where T : class
        {
            var s = new XmlSerializer(typeof(T));
            using (var st = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
            {
                using (var xw = XmlWriter.Create(st, new XmlWriterSettings() { Indent = true }))
                {
                    s.Serialize(xw, data);
                }
            }
        }

        public static T LoadFromConfig<T>(string fileName) where T : class
        {
            if (!File.Exists(fileName))
                return default;
            var s = new XmlSerializer(typeof(T));
            using (var st = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var xr = XmlReader.Create(st))
                {
                    var data = s.Deserialize(xr) as T;
                    return data;
                }
            }
        }

        public static object GetAttrValue(this XElement e, string name, Type t)
        {
            //if ()
            //{
            //    foreach (var ele in e.Elements(name))
            //    {

            //    }
            //    return null; //do not set
            //}
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
                if (IsList(p.PropertyType, out var itemType))
                {
                    LoadList(e, o, p, itemType);
                    continue;
                }
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
                if (IsList(p.PropertyType, out var itemType))
                {
                    SaveList(e, o, p, itemType);
                    continue;
                }
                var s = ConvertToString(p.GetValue(o), p.PropertyType);
                e.SetAttributeValue(p.Name, s);
            }

        }

        private static void SaveList(XElement e, object o, PropertyInfo p, Type itemType)
        {
            var list = p.GetValue(o) as System.Collections.IList;
            foreach (var item in list)
            {
                if (itemType.IsPrimitive)
                {
                    var v = ConvertToString(item, itemType);
                    e.Add(new XElement(p.Name, v));
                }
                else
                    throw new NotSupportedException($"List {itemType} is not supported");
            }
        }

        private static void LoadList(XElement e, object o, PropertyInfo p, Type itemType)
        {
            var list = p.GetValue(o) as System.Collections.IList;
            foreach (var ele in e.Elements(p.Name))
            {
                if (itemType.IsPrimitive)
                {
                    var v = ConvertFromString(ele.Value, itemType);
                    list.Add(v);
                }
                else
                    throw new NotSupportedException($"List {itemType} is not supported");
            }
        }

        private static bool IsList(Type t, out Type itemType)
        {
            if (t.IsGenericType && t.GetGenericTypeDefinition().IsAssignableFrom(typeof(IList<>)))
            {
                itemType = t.GetGenericArguments()[0];
                return true;
            }
            itemType = null;
            return false;
        }
    }
}
