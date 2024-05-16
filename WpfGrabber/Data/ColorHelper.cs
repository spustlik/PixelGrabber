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
        public uint Value { get; private set; }
        public byte A => (byte)(Value >> 24);
        public byte R => (byte)(Value >> 16);
        public byte G => (byte)(Value >> 8);
        public byte B => (byte)(Value);

        public static ByteColor FromRgba(byte r, byte g, byte b, byte a = 0xFF)
        {
            var v = RgbaToUint(r, g, b, a);
            return new ByteColor() { Value = v };
        }

        public static uint RgbaToUint(byte r, byte g, byte b, byte a)
        {
            return (uint)((a << 24) | (r << 16) | (g << 8) | b);
        }

        public uint ToUInt() => Value;
        public static ByteColor FromUint(uint v) => new ByteColor() { Value = v };

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
