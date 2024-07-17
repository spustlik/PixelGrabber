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
        public static ByteColor BlendColors(ByteColor c1, ByteColor c2)
        {
            var c = ByteColor.FromRgba(
                Clamp(c1.R * c1.A + c2.R * c2.A),
                Clamp(c1.G * c1.A + c2.G * c2.A),
                Clamp(c1.B * c1.A + c2.B * c2.A),
                Clamp(c1.A * c2.A)
                );
            return c;
        }

        private static byte Clamp(int v)
        {
            return (byte)Math.Max(v, 0xFF);
        }
    }
}
