using System;
using System.Linq;
using System.Text;
using WpfGrabber.Readers;

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
        public bool FlipX { get; private set; }

        public DataReader(byte[] bytes, int offset, bool flipX = false)
        {
            this.Data = bytes;
            this.BytePosition = offset;
            this.FlipX = flipX;
        }
        public byte ReadByte()
        {
            var b = PeekByte();
            BytePosition++;
            return b;
        }
        public byte PeekByte()
        {
            if (IsEmpty)
                return 0;
            var b = Data[BytePosition];
            if (FlipX)
                b = GetFlippedX(b);
            return b;
        }
        public byte[] ReadBytes(int count)
        {
            var result = new byte[count];
            Array.Copy(Data, BytePosition, result, 0, Math.Min(count, DataLength - BytePosition));
            if (FlipX)
            {
                for (var i = 0; i < result.Length; i++)
                {
                    result[i] = GetFlippedX(result[i]);
                }
            }
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

        public string AsString
        {
            get
            {
                var sb = new StringBuilder();
                if (FlipX)
                    sb.AppendLine("FlipX");
                if (BytePosition > 0)
                    sb.AppendLine("Pos=" + HexReader.ToHex(BytePosition));
                int i = 0;
                while (i < DataLength)
                {
                    if (i != 0)
                        sb.AppendLine();
                    sb.Append(HexReader.ToHex(i)).Append(":");
                    for (int x = 0; x < 16; x++)
                    {
                        if (i >= DataLength)
                            break;
                        sb.Append(i == BytePosition ? ">" : " ");
                        sb.Append(HexReader.ToHex(Data[i++], 2));
                    }
                }
                return sb.ToString();
            }
        }
        public string AsBinaryString
        {
            get
            {
                var sb = new StringBuilder();
                if (FlipX)
                    sb.AppendLine("FlipX");
                if (BytePosition > 0)
                    sb.AppendLine("Pos=" + HexReader.ToHex(BytePosition));
                int i = 0;
                while (i < DataLength)
                {
                    if (i != 0)
                        sb.Append(" ");
                    sb.Append(HexReader.ToBinary(Data[i++], FlipX));
                }
                return sb.ToString();
            }
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
