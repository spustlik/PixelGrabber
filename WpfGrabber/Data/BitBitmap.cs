using System.Text;
using WpfGrabber.Readers;

namespace WpfGrabber
{
    public class BitBitmap
    {
        public int WidthPixels { get; }
        public int WidthBytes { get; }
        public int Height { get; }
        public byte[] Data { get; }
        public bool ReverseByte { get; }

        public BitBitmap(int width, int height, bool reverse = false)
        {
            WidthPixels = width;
            WidthBytes = 1 + ((width - 1) / 8);  //0:0, 1-8:1, 9-16:2, 17-24:3, ...
            Height = height;
            ReverseByte = reverse;
            Data = new byte[WidthBytes * Height];
        }

        public void SetPixel(int x, int y, bool value)
        {
            if (x < 0 || x >= WidthPixels || y < 0 || y >= Height)
                return;
            byte b = GetPixelByte(x, y, out var bitIndex, out var pos);
            if (value)
            {
                b = (byte)(b | (1 << bitIndex));
            }
            else
            {
                b = (byte)(b & ~(1 << bitIndex));
            }
            Data[pos] = b;
        }

        public bool GetPixel(int x, int y)
        {
            if (x < 0 || x >= WidthPixels || y < 0 || y >= Height)
                return false;
            byte b = GetPixelByte(x, y, out var bitIndex, out var pos);
            b = (byte)(b >> bitIndex);
            return (b & 1) == 1;
        }

        private byte GetPixelByte(int x, int y, out int bitIndex, out int pos)
        {
            bitIndex = (x % 8);
            if (ReverseByte)
                bitIndex = 8 - bitIndex;
            pos = (x / 8) + y * WidthBytes;
            return Data[pos];
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < WidthBytes; x++)
                {
                    if (x != 0)
                        sb.Append(" ");
                    GetPixelByte(x * 8, y, out _, out var pos);
                    sb.Append(HexReader.ToHex(Data[pos], 2));
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
