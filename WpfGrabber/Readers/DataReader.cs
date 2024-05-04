using System;

namespace WpfGrabber
{
    public class DataReader
    {
        public byte[] Data { get; }
        public int DataLength => Data.Length;
        public int BitPosition { get; private set; }
        public int BytePosition
        {
            get => BitPosition / 8;
            set => BitPosition = value * 8;
        }

        public bool IsEmpty => this.BytePosition >= DataLength;


        /// <summary>
        /// flips each byte
        /// </summary>
        public bool FlipX { get; set; } = true;
        

        public DataReader(byte[] bytes, int offset = 0)
        {
            this.Data = bytes;
            this.BytePosition = offset;
        }
        public byte ReadByte()
        {
            if (IsEmpty)
                return 0;
            var b = Data[BytePosition++];
            if (FlipX)
                b = GetFlippedX(b);
            return b;
        }
        public byte[] ReadBytes(int count)
        {
            var result=new byte[count];
            Array.Copy(Data, BytePosition, result, 0, count);
            BytePosition = Math.Min(Data.Length, BytePosition + count);
            return result;
        }
        public int ReadWord16()
        {
            var b1 = ReadByte();
            var b2 = ReadByte();
            return b1 | b2 >> 8;
        }
        public Boolean ReadBit()
        {
            if (IsEmpty)
                return false;
            var b = Data[BytePosition];
            int shift = (BitPosition % 8);
            if (FlipX)
                shift = 7 - shift; 
            var bit = (b >> shift) & 1;
            BitPosition++;
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
