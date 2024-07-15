using ImageMagick;
using Ntreev.Library.Psd;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using WpfGrabber.Data;
using MediaPixelFormats = System.Windows.Media.PixelFormats;

namespace WpfGrabber.Readers
{
    /*
  <package id="Magick.NET.Core" version="13.9.1" targetFramework="net48" />
  <package id="Magick.NET-Q8-AnyCPU" version="13.9.1" targetFramework="net48" />
     */
    public class XcfReader
    {
        private MagickImageCollection collection;

        public XcfReader(MemoryStream ms)
        {
            var settings = new MagickReadSettings();
            //img = new MagickImage(ms, settings);
            collection = new MagickImageCollection(ms, settings);
        }

        public class LayerData
        {
            public string Name { get; set; }
            public ByteBitmapRgba Data { get; set; }
            public string Dump { get; set; }
        }

        public IEnumerable<LayerData> ReadImages()
        {
            return collection.Select(img => new LayerData()
            {
                Name = img.Label,
                Data = GetImgPixels(img),
                Dump = GetDump(img)
            });
        }

        private static string GetDump(IMagickImage<byte> img)
        {
            var sb = new StringBuilder();
            sb.AppendLine(MagickFormatInfo.Create(img.Format).ToString());
            if (img.ArtifactNames.Any())
            {
                sb.AppendLine("Artifacts:");
                foreach (var name in img.ArtifactNames)
                {
                    sb.Append($"  {name}");
                    var val = img.GetArtifact(name);
                    if (val != null)
                    {
                        sb.Append($" = {val}");
                    }
                    sb.AppendLine();
                }
            }
            if (img.AttributeNames.Any())
            {
                sb.AppendLine("Attributes:");
                foreach (var name in img.AttributeNames)
                {
                    sb.Append($"  {name}");
                    var val = img.GetAttribute(name);
                    if (val != null)
                    {
                        sb.Append($" = {val}");
                    }
                    sb.AppendLine();
                }
            }
            sb.AppendLine($"BoundingBox: {img.BoundingBox}");
            sb.AppendLine($"Channels: {String.Join(",", img.Channels)}");
            sb.AppendLine($"Color space, depth: {img.ColorSpace}, {img.Depth}");
            sb.AppendLine($"Density: {img.Density}");
            sb.AppendLine($"Gamma: {img.Gamma}");
            sb.AppendLine($"Compose: {img.Compose}");
            if (!String.IsNullOrEmpty(img.Comment))
                sb.AppendLine($"Comment:{img.Comment}");
            return sb.ToString();
        }

        private ByteBitmapRgba GetImgPixels(IMagickImage<byte> src)
        {
            if (src.ColorSpace != ImageMagick.ColorSpace.sRGB)
            {
                using (var image = src.Clone())
                {
                    image.ColorSpace = ImageMagick.ColorSpace.sRGB;
                    return GetImgPixels(image);
                }
            }

            var mapping = "RGB";
            var format = MediaPixelFormats.Rgb24;
            if (src.HasAlpha)
            {
                mapping = "BGRA";
                format = MediaPixelFormats.Bgra32;
            }

            var step = format.BitsPerPixel / 8;
            var stride = src.Width * step;
            var pixels = src.GetPixelsUnsafe();
            var data = pixels.ToByteArray(mapping);
            ByteColor getRgb24(int x, int y)
            {
                var ofs = y * stride + x * step;
                var r = data[ofs++];
                var g = data[ofs++];
                var b = data[ofs++];
                return ByteColor.FromRgba(r, g, b);
            };
            ByteColor getBgra32(int x, int y)
            {
                var ofs = y * stride + x * step;
                var b = data[ofs++];
                var g = data[ofs++];
                var r = data[ofs++];
                var a = data[ofs++];
                return ByteColor.FromRgba(r, g, b, a);
            };

            var result = new ByteBitmapRgba(src.Width, src.Height);
            Func<int, int, ByteColor> valueGetter; 
            if (format == MediaPixelFormats.Rgb24)
                valueGetter = getRgb24;
            else if (format == MediaPixelFormats.Bgra32)
                valueGetter = getBgra32;
            else
                throw new FormatException($"Invalid pixel format {format}");
            for (int y = 0; y < src.Height; y++)
            {
                for (int x = 0; x < src.Width; x++)
                {
                    result.SetPixel(x, y, valueGetter(x, y));
                }
            }
            return result;
        }
    }
}
