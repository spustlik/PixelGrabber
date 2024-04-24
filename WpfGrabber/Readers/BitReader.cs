using System;

namespace WpfGrabber
{
    public class BitReader
    {
        private readonly byte[] bytes;
        private int _posbit;
        public int BytePosition
        {
            get => _posbit / 8;
            set => _posbit = value * 8;
        }

        public bool ReverseByte { get; set; } = true;
        public int DataLength => bytes.Length;

        public BitReader(byte[] bytes)
        {
            this.bytes = bytes;
        }

        public Boolean ReadBit()
        {
            if (_posbit / 8 >= bytes.Length)
                return false;
            var b = bytes[_posbit / 8];
            int shift = (_posbit % 8);
            if (ReverseByte)
                shift = 7 - shift; 
            var bit = (b >> shift) & 1;
            _posbit++;
            return bit == 1;
        }

        public static byte GetFlippedX(byte b)
        {
            byte r = 0;
            for (int i = 0; i < 8; i++)
            {
                r = (byte)(r << 1);
                if ((b & 1) == 1)
                {
                    r = (byte)(r | 1);
                }
                b = (byte)(b >> 1);
            }
            return r;
        }

    }
}
