using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WpfGrabber.Data;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace WpfGrabber.Readers
{
    /*
    <package id="RavuAlHemio.PSD" version="0.0.5" targetFramework="net48" />
    using RavuAlHemio.PSD;
    public class PsdReader
    {
        private readonly Stream source;
        private readonly PSDFile psd;

        public PsdReader(Stream source)
        {
            this.source = source;
            psd = new PSDFile();
            psd.Read(source);
            if (psd.Depth != 8)
                throw new NotImplementedException($"Depth {psd.Depth} is not implemented");
        }

        public IEnumerable<(ByteBitmapRgba bmp, string name, string dump)> ReadImages()
        {
            foreach (var layer in psd.Layers.Reverse())
            {
                var bmp = ToByteBitmap(layer);
                var dump = GetDump(layer);
                yield return (bmp, name: layer.Name, dump);
            }
        }

        private string GetDump(PSDLayer layer)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{layer.Name} ({layer.Left},{layer.Top}) [{layer.Right - layer.Left} x {layer.Bottom - layer.Top}]");
            var vals = new List<string>();
            if (layer.Opacity != 255)
                vals.Add($"Opacity: {layer.Opacity}");
            if (layer.BlendMode != BlendMode.Normal)
                vals.Add($"Blend mode: {layer.BlendMode}");
            if (layer.Obsolete)
                vals.Add("OBSOLETE");
            if (!layer.Visible)
                vals.Add("HIDDEN");
            if (layer.NonBaseClipping)
                vals.Add("NonBaseClipping");
            if (layer.PixelDataIrrelevantToDocumentAppearance)
                vals.Add("PixelDataIrrelevantToDocumentAppearance");
            if (vals.Count > 0)
                sb.AppendLine(string.Join(", ", vals));
            if (layer.AdditionalInformation.Count > 0)
            {
                sb.AppendLine("AdditionalInformation:");
                foreach (var item in layer.AdditionalInformation)
                {
                    sb.AppendLine($"  {item.Key}: {item.Data.Length}");
                }
            }
            if (layer.BlendingRanges?.Length > 0)
            {
                sb.AppendLine("BlendingRanges:");
                foreach (var item in layer.BlendingRanges)
                {
                    sb.AppendLine($"  {item}"); //8 bytes 
                }
            }
            sb.AppendLine("Channels:");
            foreach (var ch in layer.Channels)
            {
                sb.AppendLine($"  {ch.ID} {ch.Data.DataLength}");
            }
            if (layer.LayerMask != null)
            {
                sb.AppendLine("LayerMask:");
                var m = layer.LayerMask;
                sb.AppendLine("  ... todo");
            }
            return sb.ToString();
        }

        public enum ChannelType
        {
            Alpha = -1,
            Red = 0,
            Green = 1,
            Blue = 2,
            Mask = -2
        }

        public ByteBitmapRgba ToByteBitmap(PSDLayer src)
        {
            var bmp = new ByteBitmapRgba(src.Width(), src.Height());
            var ch_R = GetChannel(src, ChannelType.Red);
            var ch_G = GetChannel(src, ChannelType.Green);
            var ch_B = GetChannel(src, ChannelType.Blue);
            var ch_A = GetChannel(src, ChannelType.Alpha);
            var ch_M = GetChannel(src, ChannelType.Mask);
            if (src.LayerMask != null)
                throw new NotImplementedException("Mask not implemented");
            if (ch_R == null || ch_G == null || ch_B == null)
                throw new NotImplementedException("Missing RGB channels");
            var w = bmp.Width;
            var h = bmp.Height;
            Func<int, int, uint> getPixel = (int x, int y) =>
            {
                byte r = ch_R.Data[y * w + x];
                byte g = ch_G.Data[y * w + x];
                byte b = ch_B.Data[y * w + x];
                byte a = 0xFF;
                if (ch_A != null)
                    a = ch_A.Data[y * w + x];
                return ColorHelper.ColorFromRGBA(r, g, b, a);
            };
            for (var y = h - 1; y >= 0; --y)
            {
                for (var x = 0; x < w; x++)
                {
                    bmp.SetPixel(x, y, getPixel(x, y));
                }
            }
            return bmp;
        }

        public class ChannelData
        {
            public PSDLayerChannel Channel { get; set; }
            public byte[] Data { get; set; }
        }
        private ChannelData GetChannel(PSDLayer src, ChannelType type)
        {
            var ch = src.Channels.FirstOrDefault(c => c.ID == (short)type);
            if (ch == null)
                return null;
            return ToReadChannel(ch, src);
        }

        public ChannelData ToReadChannel(PSDLayerChannel ch, PSDLayer layer)
        {
            source.Seek(ch.Data.Offset, SeekOrigin.Begin);
            using (var ms = new MemoryStream())
            {
                Decode(ch.Data.Compression, source, ms, layer.Width(), layer.Height(), layer.Channels.Length);
                return new ChannelData() { Channel = ch, Data = ms.ToArray() };
            }
        }

        public void Decode(CompressionType compression, Stream src, Stream dst, int width, int height, int channelCount)
        {
            switch (compression)
            {
                case CompressionType.RawData:
                    PixelDataDecoding.DecodeRawData(src, dst, null);
                    break;
                case CompressionType.PackBits:
                    int scanlineCount = height;// * channelCount;
                    PixelDataDecoding.DecodePackBits(src, dst, scanlineCount, psd.Version == 2);
                    break;
                case CompressionType.ZipWithoutPrediction:
                    PixelDataDecoding.DecodeZip(src, dst, null);
                    break;
                case CompressionType.ZipWithPrediction:
                    PixelDataDecoding.DecodeZipPredicted(src, dst, null, psd.Depth, width);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(compression));
            }
        }
    }

    public static class PSDExtensions
    {
        public static int Width(this PSDLayer layer)
        {
            return layer.Right - layer.Left;
        }
        public static int Height(this PSDLayer layer)
        {
            return layer.Bottom - layer.Top;
        }
    }
    */

    /*
    <package id="Ntreev.Library.Psd" version="1.1.18134.1310" targetFramework="net48" />
    */
    using Ntreev.Library.Psd;
    using System.Collections;

    public class PsdReader
    {
        private readonly PsdDocument doc;

        public PsdReader(Stream source)
        {
            doc = PsdDocument.Create(source);
        }

        public IEnumerable<(ByteBitmapRgba bmp, string name, string dump)> ReadImages()
        {
            return doc.Childs
                .Select(layer => (bmp: layer.ToByteBitmap(), name: layer.Name, dump: layer.GetDump()))
                .Reverse();
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
            if (layer.LinkedLayer != null)
                info.Add($"Linked layer: {layer.LinkedLayer.Name}");
            dump.AppendLine(string.Join(", ", info));
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
            foreach (var resource in properties)
            {
                if (resource.Key == "TySh")
                    continue; // hack error in lib
                if (resource.Value is IProperties props)
                {
                    if (new[] { "lyid", "lsct", "fxrp", "iOpa" }.Contains(resource.Key) && props.Count == 1)
                    {
                        dump.AppendLine($"({resource.Key}){props.ElementAt(0).Key}:{props.ElementAt(0).Value}");
                        continue;
                    }
                    string s = props.GetPropertiesDump();
                    if (!string.IsNullOrEmpty(s))
                        dump.AppendIndented("  ", s);
                }
                var ns = typeof(Ntreev.Library.Psd.PsdDocument).Namespace;
                var emptyTypeName = ns + ".Readers.EmptyResourceReader";
                if (resource.Value.GetType().FullName == emptyTypeName)
                    continue;
                dump.AppendLine($"  {resource.Key}={resource.Value}");
                if (!new[] { typeof(string) }.Contains(resource.Value.GetType()) 
                    && resource.Value is IEnumerable collection
                    &&!(resource.Value is IProperties))
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

        public static BitmapSource GetBitmap(this IImageSource src)
        {
            if (!src.HasImage)
                return null;
            var bmp = src.ToByteBitmap();
            return bmp.ToBitmapSource();
        }

        public static ByteBitmapRgba ToByteBitmap(this IImageSource src)
        {
            var bmp = new ByteBitmapRgba(src.Width, src.Height);
            var data = src.MergeChannels(); // Channels Data for each pixel in reversed order
            var ch_R = src.Channels.FirstOrDefault(c => c.Type == ChannelType.Red);
            var ch_G = src.Channels.FirstOrDefault(c => c.Type == ChannelType.Green);
            var ch_B = src.Channels.FirstOrDefault(c => c.Type == ChannelType.Blue);
            var ch_A = src.Channels.FirstOrDefault(c => c.Type == ChannelType.Alpha);
            var ch_M = src.Channels.FirstOrDefault(c => c.Type == ChannelType.Mask);
            if (src.Depth != 8)
                throw new NotImplementedException($"Image depth {src.Depth} not implemented");
            if (src.HasMask)
                throw new NotImplementedException("Mask not implemented");
            if (ch_R == null || ch_G == null || ch_B == null)
                throw new NotImplementedException("Missing RGB channells");
            var w = src.Width;
            var h = src.Height;
            Func<int, int, uint> getPixel = (int x, int y) =>
            {
                byte r = ch_R.Data[y * w + x];
                byte g = ch_G.Data[y * w + x];
                byte b = ch_B.Data[y * w + x];
                byte a = 0xFF;
                if (ch_A != null)
                    a = ch_A.Data[y * w + x];
                return ColorHelper.ColorFromRGBA(r, g, b, a);
            };
            for (var y = h - 1; y >= 0; --y)
            {
                for (var x = 0; x < w; x++)
                {
                    bmp.SetPixel(x, y, getPixel(x, y));
                }
            }
            return bmp;
        }

        public static BitmapSource GetBitmap2(this IImageSource imageSource)
        {
            if (imageSource.HasImage == false)
                return null;

            byte[] data = imageSource.MergeChannels();
            var channelCount = imageSource.Channels.Length;
            var pitch = imageSource.Width * imageSource.Channels.Length;
            var w = imageSource.Width;
            var h = imageSource.Height;

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
