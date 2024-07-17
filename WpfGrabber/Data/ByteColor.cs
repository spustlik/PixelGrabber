using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Documents;

namespace WpfGrabber.Data
{
    /// <summary>
    /// representation of RGBA color.
    /// WARNING: for mistake uint Value is ARGB, not RGBA as usually (in css colors)
    /// </summary>
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
        public static ByteColor FromString(string s)
        {
            if (string.IsNullOrEmpty(s))
                throw new FormatException($"Cannot convert empty string to color");
            if (s.Contains(","))
            {
                var parts = s.Split(',');
                if (parts.Length != 3 && parts.Length != 4)
                    throw new FormatException($"Cannot convert R,G,B[,A] string '{s}' to color");
                var b = parts.Select(p => Byte.Parse(p)).ToArray();
                var result = ByteColor.FromRgba(b[0], b[1], b[2], b.Length > 3 ? b[3] : (byte)255);
                return result;
            }
            return FromHex(s);
        }

        public static ByteColor FromHex(string str)
        {
            //# RGB        // The three-value syntax
            //# RGBA       // The four-value syntax
            //# RRGGBB     // The six-value syntax
            //# RRGGBBAA   // The eight-value syntax

            if (string.IsNullOrEmpty(str))
                throw new FormatException($"Cannot convert empty string to color");
            var s = str.TrimStart('#');
            if (s.Length <= 4)
            {
                //FED -> FFEEDD, 123F -> 112233FF
                s = String.Join("", s.Select(ch => "" + ch + ch));
            }
            if (s.Length == 6)
                s = s + "FF";
            if (!uint.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value))
            {
                throw new FormatException($"Cannot convert hex string '{str}' to color");
            }
            //!!! RGBA -> ARGB
            return FromRgba((byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value);
        }

        public uint ToUInt() => Value;
        public override string ToString() => $"#{Value:X2}";
        public string ToString(string format)
        {
            if ("rgb" == format)
                return $"{R},{G},{B}";
            if ("rgba" == format)
                return $"{R},{G},{B},{A}";
            if ("RGB" == format)
                return $"R={R:X2},G={G:X2},B={B:X2}";
            if ("RGBA" == format)
                return $"R={R:X2},G={G:X2},B={B:X2},A={A:X2}";
            if ("hex" == format || "HEX" == format)
                return ToString();
            throw new InvalidOperationException($"Invalid format '{format}' of ToString operation");
        }
        public static ByteColor FromUint(uint v) => new ByteColor() { Value = v };

        public static implicit operator ByteColor(uint c) => FromUint(c);
        public static implicit operator uint(ByteColor c) => c.ToUInt();
    }
}
