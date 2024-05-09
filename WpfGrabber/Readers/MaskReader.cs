using System;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.ComponentModel;
using System.Threading;
using System.Runtime.InteropServices;

namespace WpfGrabber.Readers
{
    // always 2 bytes DATA,MASK, some Width (bytes), Height
    // var 2 - always WidthBytes Data, than WidthBytes Mask
    // var 3 - always whole image (WidthBytes*Height) Data, than whole Mask
    public enum MaskReaderType
    {
        [Description("Byte")]
        ByteDataMask,
        [Description("Line")]
        LineDataMask,
        [Description("Image data,mask")]
        ImageDataMask,
        [Description("Image mask,data")]
        ImageMaskData,
        [Description("Image and mask preambules")]
        ImageDataMaskWithPreambule,
        [Description("4 bits")]
        Bits4DataMask,
        [Description("Only data")]
        ImageData
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
        [Description("Skip 8 bits")]
        Skip8Bits,
        [Description("Skip 16 bits")]
        Skip16Bits,
        [Description("Skip 32 bits")]
        Skip32Bits,
    }

    public class MaskReader
    {
        public DataReader Reader { get; }

        public MaskReader(DataReader reader)
        {
            this.Reader = reader;
        }

        public bool FlipX { get; set; }
        public bool FlipByte { get; set; }
        public bool FlipY { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public MaskReaderType Type { get; set; }
        public MaskReaderPreambule Preambule { get; set; }
        public int ColorType { get; set; }

        private byte[] _colors;
        private static byte[][] COLORTABLE = new[]{
            new byte[] { 1, 1, 2, 0 }, //[DM]=[00,01,10,11]
            new byte[] { 1, 1, 0, 2 },
            new byte[] { 0, 2, 1, 1 },
            new byte[] { 2, 0, 1, 1 },

            new byte[] { 0, 1, 1, 2 },
            new byte[] { 2, 1, 1, 0 },
            new byte[] { 1, 0, 2, 1 },
            new byte[] { 1, 2, 0, 1 },
            };
        public ByteBitmap8Bit Read()
        {
            var width = this.Width;
            var height = this.Height;
            _colors = COLORTABLE[this.ColorType];
            ReadPreambule(ref width, ref height);
            switch (Type)
            {
                case MaskReaderType.Bits4DataMask: return ReadBits4DataMask(width, height);
                case MaskReaderType.ByteDataMask: return ReadByteDataMask(width, height);
                case MaskReaderType.LineDataMask: return ReadLineDataMask(width, height);
                case MaskReaderType.ImageData: return ReadImageDataMask(width, height, Type);
                case MaskReaderType.ImageDataMask: return ReadImageDataMask(width, height, Type);
                case MaskReaderType.ImageMaskData: return ReadImageDataMask(width, height, Type);
                case MaskReaderType.ImageDataMaskWithPreambule: return ReadImageDataMask(width, height, Type);
                default: throw new NotImplementedException($"Reading mask type {Type}");
            }
        }

        private void ReadPreambule(ref int width, ref int height)
        {
            switch (Preambule)
            {
                case MaskReaderPreambule.None:
                    break;
                case MaskReaderPreambule.Skip8Bits:
                    Reader.ReadByte();
                    break;
                case MaskReaderPreambule.Skip16Bits:
                    Reader.ReadWord16();
                    break;
                case MaskReaderPreambule.Skip32Bits:
                    Reader.ReadWord16();
                    Reader.ReadWord16();
                    break;
                case MaskReaderPreambule.WidthHeight:
                    {
                        width = Reader.ReadByte();
                        height = Reader.ReadByte();
                        break;
                    }
                case MaskReaderPreambule.WidthHeight8:
                    {
                        width = Reader.ReadByte() / 8;
                        height = Reader.ReadByte();
                        break;
                    }
                case MaskReaderPreambule.WidthHeight16:
                    {
                        width = Reader.ReadWord16() / 8;
                        height = Reader.ReadWord16();
                        break;
                    }
                case MaskReaderPreambule.HeightWidth8:
                    {
                        height = Reader.ReadByte();
                        width = (Reader.ReadByte() / 8) / 2;
                        break;
                    }
                case MaskReaderPreambule.HeightWidth16:
                    {
                        height = Reader.ReadWord16();
                        width = Reader.ReadWord16() / 8;
                        break;
                    }
                default:
                    throw new NotImplementedException($"Reading mask preambule {Preambule}");
            }
        }

        private ByteBitmap8Bit ReadImageDataMask(
            int width, 
            int height,
            MaskReaderType type)
        {
            var result = new ByteBitmap8Bit(width * 8, height);
            bool readPreambule = type == MaskReaderType.ImageDataMaskWithPreambule;
            bool swapMaskData = type == MaskReaderType.ImageMaskData;
            bool skipMask = type == MaskReaderType.ImageData;

            if (readPreambule)
            {
                ReadPreambule(ref width, ref height);
            }
            var dataBytes = ReadBytes(width * height);

            var maskBytes = skipMask ? new byte[width * height] : ReadBytes(width * height);
            if (swapMaskData)
            {
                (dataBytes, maskBytes) = (maskBytes, dataBytes);
            }
            for (var y = 0; y < height; y++)
            {
                var posY = FlipY ? height - y - 1 : y;
                for (int x = 0; x < width; x++)
                {
                    WriteByte(result, x * 8, posY, dataBytes[x + y * width], maskBytes[x + y * width]);
                }
            }
            return result;
        }

        private ByteBitmap8Bit ReadLineDataMask(int width, int height)
        {
            var result = new ByteBitmap8Bit(width * 8, height);
            for (var y = 0; y < height; y++)
            {
                var posY = FlipY ? height - y - 1 : y;

                var dataBytes = ReadBytes(width);
                var maskBytes = ReadBytes(width);

                for (int x = 0; x < width; x++)
                {
                    WriteByte(result, x * 8, posY, dataBytes[x], maskBytes[x]);
                }
            }
            return result;
        }

        private byte[] ReadBytes(int count)
        {
            var result = new byte[count];
            for (int i = 0; i < result.Length; i++)
            {
                var b = Reader.ReadByte();
                if (FlipByte)
                    b = DataReader.GetFlippedX(b);
                result[i] = b;
            }
            return result;
        }
        private void WriteByte(ByteBitmap8Bit result, int x, int y, byte data, byte mask, int size = 8)
        {
            for (var i = 0; i < size; i++)
            {
                byte b = GetDMColor(data, mask);
                var posX = x + i;
                if (FlipX)
                    posX = result.Width - posX - 1;
                result.SetPixel(posX, y, b);
                data = (byte)(data >> 1);
                mask = (byte)(mask >> 1);
            }
        }

        private byte GetDMColor(byte data, byte mask)
        {
            var dm = ((data & 1) << 1) | (mask & 1);
            var b = _colors[dm];
            return b;
        }
        private static byte GetBit0Color2(byte data, byte mask)
        {
            var colors = new byte[] { 1, 1, 2, 0 }; //[DM]=[00,01,10,11]
            var dm = ((data & 1) << 1) | (mask & 1);
            var b = colors[dm];
            return b;
        }

        private ByteBitmap8Bit ReadBits4DataMask(int width, int height)
        {
            var result = new ByteBitmap8Bit(4 * width, height);
            for (var y = 0; y < height; y++)
            {
                var posY = FlipY ? height - y - 1 : y;
                var data = ReadBytes(width);
                for (int x = 0; x < width; x++)
                {
                    var dm = data[x];
                    var d = (byte)(dm & 0xF);
                    var m = (byte)((dm >> 4) & 0x0F);
                    WriteByte(result, x * 4, posY, d, m, 4);
                }
            }
            return result;
        }

        private ByteBitmap8Bit ReadByteDataMask(int width, int height)
        {
            var result = new ByteBitmap8Bit(width * 8, height);
            for (var y = 0; y < height; y++)
            {
                var posY = FlipY ? height - y - 1 : y;

                for (int x = 0; x < width; x++)
                {
                    //var data = BitReader.ReadByte();
                    //var mask = BitReader.ReadByte();
                    var data = ReadBytes(1)[0];
                    var mask = ReadBytes(1)[0];
                    WriteByte(result, x * 8, posY, data, mask);
                }
            }
            return result;

        }

        //private static byte GetBit0Color(byte data, byte mask)
        //{
        //    if ((mask & 1) == 0)
        //        return 0; //white
        //    if ((data & 1) == 0)
        //        return 1; //transparent
        //    return 2; //black
        //}

    }

}
