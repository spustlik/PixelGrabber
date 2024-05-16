using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WpfGrabber.Data
{
    public struct ByteColor
    {
        public byte A { get; set; }
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        public static ByteColor FromRgba(byte r, byte g, byte b, byte a = 0xFF)
        {
            return new ByteColor() { A = a, R = r, G = g, B = b };
        }
        public uint ToUInt()
        {
            return (uint)((A << 24) | (R << 16) | (G << 8) | B);
        }
        public static ByteColor FromUint(uint x)
        {
            byte a, r, g, b;
            b = (byte)(x & 0xFF);
            x >>= 8;
            g = (byte)(x & 0xFF);
            x >>= 8;
            r = (byte)(x & 0xFF);
            x >>= 8;
            a = (byte)(x & 0xFF);
            return FromRgba(r, g, b, a);
        }

        public static implicit operator ByteColor(uint c) => FromUint(c);
        public static implicit operator uint(ByteColor c) => c.ToUInt();
    }
    public static class ColorHelper
    {

        public static uint BlendColors(uint color1, uint color2)
        {
            var c1 = ByteColor.FromUint(color1);
            var c2 = ByteColor.FromUint(color2);
            var c = ByteColor.FromRgba(
                Clamp(c1.R * c1.A + c2.R * c2.A),
                Clamp(c1.G * c1.A + c2.G * c2.A),
                Clamp(c1.B * c1.A + c2.B * c2.A),
                Clamp(c1.A * c2.A)
                ).ToUInt();
            return c;
        }

        private static byte Clamp(int v)
        {
            return (byte)Math.Max(v, 0xFF);
        }
    }
}
