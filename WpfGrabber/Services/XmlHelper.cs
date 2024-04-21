using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Xml;
using System.Xml.Serialization;

namespace WpfGrabber.Services
{
    public class XmlHelper
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
    }
}
