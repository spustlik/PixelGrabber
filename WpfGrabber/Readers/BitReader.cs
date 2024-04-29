using System;

namespace WpfGrabber
{
    public class BitReader
    {
        public byte[] Data { get; }
        private int _posbit;
        public int BytePosition
        {
            get => _posbit / 8;
            set => _posbit = value * 8;
        }

        /// <summary>
        /// flips each byte
        /// </summary>
        public bool FlipX { get; set; } = true;
        public int DataLength => Data.Length;

        public BitReader(byte[] bytes)
        {
            this.Data = bytes;
        }
        public byte ReadByte()
        {
            if (BytePosition >= DataLength)
                return 0;
            var b = Data[BytePosition++];
            if (FlipX)
                b = GetFlippedX(b);
            return b;
        }
        public int ReadWord16()
        {
            var b1 = ReadByte();
            var b2 = ReadByte();
            return b1 | b2 >> 8;
        }
        public Boolean ReadBit()
        {
            if (BytePosition >= Data.Length)
                return false;
            var b = Data[BytePosition];
            int shift = (_posbit % 8);
            if (FlipX)
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
