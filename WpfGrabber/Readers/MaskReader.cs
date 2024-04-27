using System;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.ComponentModel;

namespace WpfGrabber.Readers
{
    // vzdy dva byte DATA,MASK, nejaka vyska a (sirka v bytech)
    //varianta 2 - vzdy WidthBytes Data, pak WidthBytes Mask
    //varianta 3 - vzdy cely obr (WidthBuytes*Height) Data, cely obr mask

    public enum MaskReaderType
    {
        [Description("Byte")]
        ByteDataMask,
        [Description("Line")]
        LineDataMask,
        [Description("Image")]
        ImageDataMask,
        [Description("Image and mask preambules")]
        ImageDataMaskWithPreambule,

    }

    public enum MaskReaderPreambule
    {
        None,
        [Description("Width & Height")]
        WidthHeight,
        [Description("Width & Height 8 bits")]
        WidthHeight8,
        [Description("Width & Height 16 bits")]
        WidthHeight16,
        [Description("Height & Width 8 bits")]
        HeightWidth8,
        [Description("Height & Width 16 bits")]
        HeightWidth16,

    }
    public class MaskReader
    {
        public BitReader BitReader { get; }

        public MaskReader(BitReader bitReader)
        {
            this.BitReader = bitReader;
        }

        public bool FlipX { get; set; }
        public bool FlipY { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public MaskReaderType Type { get; set; }
        public MaskReaderPreambule Preambule { get; set; }

        public ByteBitmap8Bit Read()
        {
            var width = this.Width;
            var height = this.Height;
            ReadPreambule(ref width, ref height);
            switch (Type)
            {
                case MaskReaderType.ByteDataMask: return ReadByteArrayMask(width, height);
                case MaskReaderType.LineDataMask: return ReadLineDataMask(width, height);
                case MaskReaderType.ImageDataMask: return ReadImageDataMask(width, height);
                case MaskReaderType.ImageDataMaskWithPreambule: return ReadImageDataMask(width, height, true);
                default: throw new NotImplementedException($"Reading mask type {Type}");
            }
        }

        private void ReadPreambule(ref int width, ref int height)
        {
            switch (Preambule)
            {
                case MaskReaderPreambule.None:
                    break;
                case MaskReaderPreambule.WidthHeight:
                    {
                        width = BitReader.ReadByte();
                        height = BitReader.ReadByte();
                        break;
                    }
                case MaskReaderPreambule.WidthHeight8:
                    {
                        width = BitReader.ReadByte() / 8;
                        height = BitReader.ReadByte();
                        break;
                    }
                case MaskReaderPreambule.WidthHeight16:
                    {
                        width = BitReader.ReadWord16() / 8;
                        height = BitReader.ReadWord16();
                        break;
                    }
                case MaskReaderPreambule.HeightWidth8:
                    {
                        height = BitReader.ReadByte();
                        width = (BitReader.ReadByte() / 8) / 2;
                        break;
                    }
                case MaskReaderPreambule.HeightWidth16:
                    {
                        height = BitReader.ReadWord16();
                        width = BitReader.ReadWord16() / 8;
                        break;
                    }
                default:
                    throw new NotImplementedException($"Reading mask preambule {Preambule}");
            }
        }

        private ByteBitmap8Bit ReadImageDataMask(int width, int height, bool maskPreambule = false)
        {
            var result = new ByteBitmap8Bit(width * 8, height);

            var dataBytes = ReadBytes(width * height);
            if(maskPreambule)
            {
                ReadPreambule(ref width, ref height);
            }
            var maskBytes = ReadBytes(width * height);
            for (var y = 0; y < height; y++)
            {
                var posY = FlipY ? height - y : y;
                for (int x = 0; x < width; x++)
                {
                    WriteByte(result, x, posY, dataBytes[x + y * width], maskBytes[x + y * width]);
                }
            }
            return result;
        }

        private ByteBitmap8Bit ReadLineDataMask(int width, int height)
        {
            var result = new ByteBitmap8Bit(width * 8, height);
            for (var y = 0; y < height; y++)
            {
                var posY = FlipY ? height - y : y;

                var dataBytes = ReadBytes(width);
                var maskBytes = ReadBytes(width);

                for (int x = 0; x < width; x++)
                {
                    WriteByte(result, x, posY, dataBytes[x], maskBytes[x]);
                }
            }
            return result;
        }

        private byte[] ReadBytes(int count)
        {
            var result = new byte[count];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = BitReader.ReadByte();
            }
            return result;
        }
        private void WriteByte(ByteBitmap8Bit result, int x, int y, byte data, byte mask)
        {
            for (var i = 0; i < 8; i++)
            {
                byte b = GetBit0Color2(data, mask);
                var posX = x * 8 + i;
                if (FlipX)
                    posX = result.Width - posX;
                result.SetPixel(posX, y, b);
                data = (byte)(data >> 1);
                mask = (byte)(mask >> 1);
            }
        }

        private ByteBitmap8Bit ReadByteArrayMask(int width, int height)
        {
            var result = new ByteBitmap8Bit(width * 8, height);
            for (var y = 0; y < height; y++)
            {
                var posY = FlipY ? height - y : y;

                for (int x = 0; x < width; x++)
                {
                    var data = BitReader.ReadByte();
                    var mask = BitReader.ReadByte();
                    WriteByte(result, x, posY, data, mask);
                }
            }
            return result;

        }

        private static byte GetBit0Color(byte data, byte mask)
        {
            if ((mask & 1) == 0)
                return 0; //white
            if ((data & 1) == 0)
                return 1; //transparent
            return 2; //black
        }
        private static byte GetBit0Color2(byte data, byte mask)
        {
            var colors = new byte[] { 1, 1, 2, 0 };
            var dm = ((data & 1) << 1) | (mask & 1);
            var b = colors[dm];
            return b;
        }

    }

}
