using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

}
