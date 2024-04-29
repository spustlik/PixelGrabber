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
        public static (byte a,byte r,byte g, byte b) ColorFromUint(uint x)
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
        public static uint BlendColors(uint color1, uint color2)
        {
            throw new NotImplementedException();
            /*
            //TODO:
            var c1 = ColorFromUint(color1);
            var c2 = ColorFromUint(color2);
            //brat pomer A1 a A2, ale co kdyz nova barva chce preplacnout starou?
            var c = ColorFromUint(???, c1.r * c1.a + c2.r * c2.a,  )
            return c;
            */
        }
    }
}
