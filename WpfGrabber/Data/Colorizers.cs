using System;
using System.Linq;

namespace WpfGrabber.Data
{
    public delegate uint Colorizer(uint data, uint orig);

    public static class Colorizers
    {
        public static uint GetColorCopy(uint c, uint orig)
        {
            return c;
        }

        public static uint GetColor01Gray(uint b, uint orig)
        {
            switch (b)
            {
                case 0: return 0xFFFFFFFF;
                case 1: return 0;
                default: return GetColorGray(b, orig);
            }
        }
        public static uint GetColor10Gray(uint b, uint orig)
        {
            switch (b)
            {
                case 0: return 0;
                case 1: return 0xFFFFFFFF;
                default: return GetColorGray(b, orig);
            }
        }

        public static uint GetColorBlack(uint b, uint orig)
        {
            if (b == 0) return 0;
            return 0xFF000000;
        }
        public static uint GetColorWhite(uint b, uint orig)
        {
            if (b == 0) return 0;
            return 0xFFFFFFFF;
        }
        public static uint GetColorGray(uint d, uint orig)
        {
            return 0xff000000 | d | d >> 8 | d >> 16;
        }

        public static Colorizer CreateColor(uint color)
        {
            if ((color & 0xFF000000) == 0)
                color = color | 0xFF000000;
            return (d, orig) => d == 0 ? orig : color;
        }

        public static uint GetColorZx(uint b, uint orig)
        {
            if (b <= 1) return 0; //dve cerne jako pruhledna
            return zxpalette[b & 0xF] | 0xFF000000;
        }

        private static uint zxToRgba(uint c)
        {
            //0x38->0xFF, ...
            var (r, g, b) = ((c & 0xFF0000) >> 16, (c & 0x00FF00) >> 8, c & 0x0000FF);
            r = (r * 0xFF) / 0x38;
            g = (g * 0xFF) / 0x38;
            b = (b * 0xFF) / 0x38;
            return (uint)((0xFF << 24) | (r << 16) | (g << 8) | b);
        }

        private static uint[] zxpalette = new uint[] {
                0x000000,0x000000,0x083008,0x183818,
                0x080838,0x101838,0x280808,0x103038,
                0x380808,0x381818,0x303008,0x303020,
                0x082008,0x301028,0x282828,0x383838,
            }.Select(x => zxToRgba(x)).ToArray();

    }
}
