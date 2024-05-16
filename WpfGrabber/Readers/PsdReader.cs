using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WpfGrabber.Data;
using System.IO;
using Ntreev.Library.Psd;
using System.Collections;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace WpfGrabber.Readers
{
    /*
    <package id="Ntreev.Library.Psd" version="1.1.18134.1310" targetFramework="net48" />
    */

    public class PsdReader
    {
        private readonly PsdDocument doc;
        public bool RespectBounds { get; set; } = true;
        public bool ReverseLayerOrder { get; set; } = true;
        public PsdReader(Stream source)
        {
            doc = PsdDocument.Create(source);
        }

        public IEnumerable<(ByteBitmapRgba bmp, string name, string dump)> ReadImages()
        {
            var items = doc.Childs
                .Where(l => l.HasImage)
                .Select(layer => (
                    bmp: ToByteBitmap(layer),
                    name: layer.Name,
                    dump: layer.GetDump()
                    ));
            if (ReverseLayerOrder)
                items = items.Reverse();
            return items;
        }
        public ByteBitmapRgba ToByteBitmap(IPsdLayer src)
        {
            var data = src.MergeChannels(); // Channels Data for each pixel in reversed order
            var ch_R = src.Channels.FirstOrDefault(c => c.Type == ChannelType.Red);
            var ch_G = src.Channels.FirstOrDefault(c => c.Type == ChannelType.Green);
            var ch_B = src.Channels.FirstOrDefault(c => c.Type == ChannelType.Blue);
            var ch_A = src.Channels.FirstOrDefault(c => c.Type == ChannelType.Alpha);
            var ch_M = src.Channels.FirstOrDefault(c => c.Type == ChannelType.Mask);
            if (src.Depth != 8)
                throw new NotImplementedException($"Image depth {src.Depth} not implemented");
            if (src.HasMask || ch_M != null)
                throw new NotImplementedException("Mask not implemented");
            if (ch_R == null || ch_G == null || ch_B == null)
                throw new NotImplementedException("Missing RGB channells");
            var dw = src.Width;
            if (RespectBounds) dw = src.Width + src.Left;
            var dh = src.Height;
            if (RespectBounds) dh = src.Height + src.Top;
            var sw = src.Width;
            var sh = src.Height;
            var bmp = new ByteBitmapRgba(dw, dh);
            Func<int, int, uint> getPixel = (int x, int y) =>
            {
                byte r = ch_R.Data[y * sw + x];
                byte g = ch_G.Data[y * sw + x];
                byte b = ch_B.Data[y * sw + x];
                byte a = 0xFF;
                if (ch_A != null)
                    a = ch_A.Data[y * sw + x];
                return ByteColor.FromRgba(r, g, b, a);
            };
            Action<int, int, uint> setPixel = bmp.SetPixel;
            if (RespectBounds)
            {
                setPixel = (int x, int y, uint color) =>
                {
                    var c = ByteColor.FromUint(color);
                    c.R = 0;
                    c.G = 0;
                    c.B = 0x40;
                    bmp.SetPixel(x + src.Left, y + src.Top, c);
                };
            }
            for (var y = sh - 1; y >= 0; --y)
            {
                for (var x = 0; x < sw; x++)
                {
                    setPixel(x, y, getPixel(x, y));
                }
            }
            return bmp;
        }
    }

    public static class NtreevPsdExtensions
    {
        public static string GetDump(this IPsdLayer layer)
        {
            var dump = new StringBuilder();
            dump.AppendLine($"Name='{layer.Name}' [{layer.Left},{layer.Top}] {layer.Width} x {layer.Height}");
            var info = new List<string>();

            if (layer.Depth != 8)
                info.Add($"Depth: {layer.Depth}");
            if (layer.Opacity != 1)
                info.Add($"Opacity: {layer.Opacity}");
            if (layer.BlendMode != BlendMode.Normal)
                info.Add($"BlendMode: {layer.BlendMode}");
            if (!layer.HasImage)
                info.Add($"!NOIMAGE");
            if (layer.HasMask)
                info.Add("MASK");
            if (layer.IsClipping)
                info.Add("Clipping");
            dump.AppendLine(string.Join(", ", info));
            if (layer.LinkedLayer != null)
            {
                dump.Append($"Linked layer: {layer.LinkedLayer.Name}");
                //dump.AppendIndented("  ", GetDump(layer.LinkedLayer));
            }

            dump.Append("Channels: ")
                .Append(string.Join(",", layer.Channels.Select(c => c.Type)))
                .Append(string.Join(", ", layer.Channels.Select(c => $"({c.Data.Length})")))
                .AppendLine();
            if (layer.Resources.Count > 0)
            {
                dump.AppendLine("Resources:");
                dump.AppendIndented("  ", GetPropertiesDump(layer.Resources));
            }
            if (layer.Childs.Length > 0)
            {
                dump.AppendLine("Childs:");
                dump.AppendIndented("  ", layer.Childs.Select(child => child.GetDump()).ToArray());
            }
            return dump.ToString();
        }

        public static string GetPropertiesDump(this IProperties properties)
        {
            var dump = new StringBuilder();
            foreach (var item in properties)
            {
                if (item.Value is IProperties p)
                {
                    dump.AppendLine($"({item.Key})");
                    if (IsEmpty(item))
                        continue;
                    dump.AppendIndented("  ", GetPropertiesDump(p));
                    continue;
                }
                if (item.Value is IEnumerable enumerable && !(item.Value is string))
                {
                    int index = 0;
                    foreach (var i in enumerable)
                    {
                        dump.AppendLine($"{item.Key}[{index}]");
                        if (i is IProperties p2)
                            dump.AppendIndented("  ", GetPropertiesDump(p2));
                        else
                            dump.AppendIndented("  ", $"{i} ({i.GetType().Name})");
                        index++;
                    }
                    continue;
                }
                dump.AppendLine($"({item.Key}) {item.Value} ({item.Value.GetType().Name})");
            }
            return dump.ToString().Trim();
        }
        public static string GetPropertiesDump2(this IProperties properties)
        {
            var dump = new StringBuilder();
            foreach (var resource in properties)
            {
                if (resource.Key == "TySh")
                    continue; // hack error in lib
                if (resource.Value is IProperties props)
                {
                    if (props.Count == 1 && new[] { "lyid", "lsct", "fxrp", "iOpa" }.Contains(resource.Key))
                    {
                        dump.AppendLine($"({resource.Key}){props.ElementAt(0).Key}:{props.ElementAt(0).Value}");
                        continue;
                    }
                    string s = props.GetPropertiesDump();
                    if (!string.IsNullOrEmpty(s))
                        dump.AppendIndented("  ", s);
                }
                if (IsEmpty(resource))
                    continue;
                dump.AppendLine($"  {resource.Key}={resource.Value}");
                if (!new[] { typeof(string) }.Contains(resource.Value.GetType())
                    && resource.Value is IEnumerable collection
                    && !(resource.Value is IProperties))
                {
                    foreach (var item in collection)
                    {
                        if (item is IProperties ppp)
                            dump.AppendIndented("    ", GetPropertiesDump(ppp));
                        else
                            dump.AppendLine("    " + item.ToString());
                    }
                    continue;
                }
            }
            return dump.ToString().TrimEnd();
        }

        private static bool IsEmpty(KeyValuePair<string, object> resource)
        {
            var ns = typeof(Ntreev.Library.Psd.PsdDocument).Namespace;
            var emptyTypeName = ns + ".Readers.EmptyResourceReader";
            return resource.Value.GetType().FullName == emptyTypeName;
        }

        public static BitmapSource GetBitmap(this IImageSource imageSource)
        {
            if (imageSource.HasImage == false)
                return null;

            byte[] data = imageSource.MergeChannels();
            var channelCount = imageSource.Channels.Length;
            var pitch = imageSource.Width * imageSource.Channels.Length;
            var w = imageSource.Width;
            var h = imageSource.Height;

            //var format = channelCount == 3 ? TextureFormat.RGB24 : TextureFormat.ARGB32;
            //var tex = new Texture2D(w, h, format, false);
            var colors = new Color[data.Length / channelCount];


            var k = 0;
            for (var y = h - 1; y >= 0; --y)
            {
                for (var x = 0; x < pitch; x += channelCount)
                {
                    var n = x + y * pitch;

                    var c = Color.FromArgb(1, 1, 1, 1);
                    if (channelCount == 4)
                    {
                        c.B = data[n++];
                        c.G = data[n++];
                        c.R = data[n++];
                        c.A = (byte)System.Math.Round(data[n++] / 255f * imageSource.Opacity * 255f);
                    }
                    else
                    {
                        c.B = data[n++];
                        c.G = data[n++];
                        c.R = data[n++];
                        c.A = (byte)System.Math.Round(imageSource.Opacity * 255f);
                    }
                    colors[k++] = c;
                }
            }
            if (channelCount == 4)
                return BitmapSource.Create(imageSource.Width, imageSource.Height, 96, 96, PixelFormats.Bgra32, null, data, pitch);
            return BitmapSource.Create(imageSource.Width, imageSource.Height, 96, 96, PixelFormats.Bgr24, null, data, pitch);
        }

    }
}