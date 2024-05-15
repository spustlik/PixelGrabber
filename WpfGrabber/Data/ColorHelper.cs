using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WpfGrabber.Data
{
    public static class ColorHelper
    {
        public static (byte a, byte r, byte g, byte b) ColorFromUint(uint x)
        {
            byte a, r, g, b;
            b = (byte)(x & 0xFF);
            x >>= 8;
            g = (byte)(x & 0xFF);
            x >>= 8;
            r = (byte)(x & 0xFF);
            x >>= 8;
            a = (byte)(x & 0xFF);
            return (a, r, g, b);
        }

        public static uint ColorFromRGBA(byte r, byte g, byte b, byte a = 0xFF)
        {
            //ARGB
            return (uint)((a << 24) | (r << 16) | (g << 8) | b);
        }

        public static uint BlendColors(uint color1, uint color2)
        {
            var c1 = ColorFromUint(color1);
            var c2 = ColorFromUint(color2);
            var c = ColorFromRGBA(
                Clamp(c1.r * c1.a + c2.r * c2.a),
                Clamp(c1.g * c1.a + c2.g * c2.a),
                Clamp(c1.b * c1.a + c2.b * c2.a),
                Clamp(c1.a * c2.a)
                );
            return c;
        }

        private static byte Clamp(int v)
        {
            return (byte)Math.Max(v, 0xFF);
        }
    }
}
