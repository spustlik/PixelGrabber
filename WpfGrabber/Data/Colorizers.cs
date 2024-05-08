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
    }
}
