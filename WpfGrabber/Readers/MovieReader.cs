using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfGrabber.Readers
{
    //Movie game engine, but not complete - user must find sprite starts (see Marks)
    //height(16bit), data(h*WIDTH), mask reversed
    public class MovieReader
    {
        private DataReader rd;
        public bool FlipVertical { get; set; }
        public int Width { get; set; }

        public MovieReader(DataReader reader)
        {
            this.rd = reader;
        }

        public IEnumerable<ReaderImageResult> ReadImages()
        {
            while (!rd.IsEmpty)
            {
                yield return ReadImage();
            }
        }

        public ReaderImageResult ReadImage()
        {
            var pos = rd.BytePosition;
            int w = rd.ReadByte();
            if (w == 0xc0)
            {
                var unknown = rd.ReadByte(); //skip,  next is 0x8? or 0xC1
                                             //6B3D: C0 00 C1 39 
                w = rd.ReadByte();
            }

            int h = 0;
            var readmask = true;
            if ((w & 0b11110000) == 0xF0)
            {
                //TODO: ???
                // E3
                //F1,FB,F8
                readmask = false;
            }
            else if ((w & 0b11110000) == 0x80)
            {
                //highest bit(s?) can mean that there is mask & data, data otherwise
                //0x81(w=2), 0x83 BOM (w=4), 0x82(0x98FF-man)
                w = 1 + (byte)(w & 0b0111111);
                h = rd.ReadByte();
            }
            else if ((w & 0b11110000) == 0xc0)
            {
                //0xC1, 0xC0, 0xC8
                if (w == 0xC1)
                {
                    w = 1 + (byte)(w & 0b0011111);
                    h = rd.ReadByte();
                }
                else
                {
                    //???
                }
                readmask = false;
            }
            else if ((w & 0b11110000) == 0x40)
            {
                //41, 42
                //TODO: another colors?!?
                w = 1 + (w & 0b1111);
                h = rd.ReadByte();
                rd.ReadByte(); // ignore C9 8C, 9E 8A
                rd.ReadByte();
                readmask = false;
            }
            else
            {
                w = Width;
                h = rd.ReadByte();
            }
            var bmp = ReadBitmap(w, h, readmask, flipX:true, flipY:FlipVertical);
            var result = new ReaderImageResult(bmp, pos, rd.BytePosition);
            return result;
        }

        public ByteBitmap8Bit ReadBitmap(int w, int h, bool readmask, bool flipX, bool flipY)
        {
            var datar = new DataReader(rd.ReadBytes(w * h), 0, flipX:flipX);
            var maskr = readmask ? new DataReader(rd.ReadBytes(w * h), 0, flipX: false) : null;
            var bmp = new ByteBitmap8Bit(w * 8, h);
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        var d = datar.ReadBit();
                        var m = maskr?.ReadBit() ?? false;
                        var ry = y;
                        if (flipY)
                            ry = bmp.Height - y - 1;
                        //bmp.SetPixel(x * 8 + i, ry, (byte)(m ? 1 : 2));
                        bmp.SetPixel(x * 8 + i, ry, (byte)(m ? 1 : d ? 0 : 2));
                    }
                }
            }
            return bmp;
        }
    }

    public class MovieMark
    {
        public int offset;
        public int w;
        public int h;
        public bool skip;
        public bool IsImage => w == 0 && h == 0 && !skip;
        public static MovieMark[] GetMarks()
        {
            MovieMark _(int ofs, int w = 0, int h = 0, bool _skip = false)
            {
                return new MovieMark() { offset = ofs, w = w, h = h, skip = _skip };
            }
            MovieMark s(int ofs) => _(ofs, 0, 0, true);
            var result = new[]
            {
                #region offsets
                _(0x3c3b),
                s(0x3E67), //42 10 63 5C 21
                _(0x447B, w:-32, h:24),
                s(0x4778),
                _(0x4778 + 3, w:-5, h:18),
                _(0x47d5 + 2),
                s(0x4805), // F8 F5 E1 A8
                _(0x4805 + 4, w:3, h:17),
                s(0x483c), // 07 08 09 0C 00 04 0B 0A | 15 16 00 18 30 48 60 89 
                _(0x4850),
                _(0x48F4, w:2,h:8), //FA FB DE BB DD FB DD 77 | DD 8F DD EF DD EF 21 28 
                s(0x4904), //16 9E D4 60 A7 08 74 D5 | 88 A7 0A 66 A8 4C A9 21 
                _(0x493e, w:2, h:20),
                _(0x4966, w:2, h:20),
                _(0x498e, w:2, h:20),
                s(0x49B6), //7E 48 FD 7E D3 FB 79 D7 | FB C7 D6 FB DF D1 FA DF 
                _(0x49B9, w:3,h:11),
                _(0x49DA, w:2,h:18), // 9D 5B 7D 5A FB B1 E7 6D | 97 9D 77 AC F7 C3 F7 D3 )
                _(0x4F4E),
                _(0x501c, w:3,h:25),
                _(0x5067, w:3,h:25),
                _(0x50B2, w:2,h:20),
                _(0x50DA, w:2,h:17),
                _(0x5322, w:3,h:25),
                s(0x536D), //536D: 0C 00 04 45 0C 00 08 15 | 05 40 42 25 40 01 52 01 
                //53EA: 40 51 60 00 00 00 F0 03 | 00 00 00 E0 03 00 00 00 
                _(0x5476),
                s(0x54FC), // 02 16 16 D2 B4 14 B5 80 | 0A 00 06 1E 78 68 68 30 
                _(0x5503),
                s(0x5519), // 99 81 00 00 00 00 00 00 | 00 00 00 01 E0 00 00 01
                _(0x551B, w:-4,h:44),
                s(0x55CB), // FF 03 80 FF 01 00 FF 00 | 00 FE 01 00 F8 01 00 F0 
                _(0x55CC,w:3,h:15),
                s(0x55F9), // 17 FF FF 02 16 16 D2 B4 | 14 B5 80 0A 00 06 1E 78 
                _(0x5603),
                s(0x5619), // 99 81 FF F0 7F E0 3F C0 | 7F 80 3F 80 1F 00 1F 00
                _(0x561b, w:2,h:23), // h?
                s(0x5649), // C0 D0 F8 C4 C9 A8 FF FF | FF 
                _(0x5652),
                s(0x57FA), // 21 28 17 C6 D3 2D DA 08 | 88 D4 5B DA 09 16 A6 F8 
                _(0x5864),
                _(0x5B40,w:-2,h:38),
                _(0x5B8C,w:2,h:38),
                _(0x5BD8,w:-2,h:40),
                _(0x5C28,w:2,h:40),
                s(0x5C78), // 11 3B 22 25 9B 71 9B 19 | 3F 9B 8B 9B 11 38 22 BD
                _(0x5C90),
                s(0x5CF6),
                _(0x5E1b, w:-1, h:6*43), // font
                _(0x5F1D),
                s(0x6037), // 20 1F CD CD CE 20 9C 9B | 52 52 53 9C 9C 9D F5 F5 
                _(0x60C2),
                _(0x63D0,w:-2,h:37),
                s(0x641A), // FF 11 3B 22 0C A4 71 9B | 19 50 A4 8B 9B 00 00 00 
                _(0x6427,w:-2,h:59),
                _(0x649D),
                _(0x6561,w:-2,h:12),
                _(0x6579,w:-2,h:13),
                _(0x6593,w:-2,h:14),
                _(0x65AF,w:-2,h:18),
                _(0x65D3,w:-2,h:13),
                _(0x65ED,w:-2,h:14),
                _(0x6609,w:-2,h:13),
                _(0x6623,w:-2,h:16),
                _(0x6643,w:-2,h:12),
                _(0x665B,w:-2,h:13),
                _(0x6675,w:-2,h:14),
                _(0x6691,w:-2,h:31),
                _(0x66CF,w:-2,h:43),
                _(0x6725),
                _(0x67CB,w:-2,h:14),
                _(0x67E7,w:-2,h:14),
                _(0x6803,w:-2,h:15),
                _(0x6821,w:-2,h:17),
                _(0x6843,w:-2,h:12),
                _(0x685B,w:-2,h:13),
                _(0x6875,w:-2,h:15),
                _(0x6893,w:-2,h:15),
                _(0x68B1,w:-2,h:14),
                _(0x68CD,w:-2,h:15),
                _(0x68EB,w:-2,h:14),
                _(0x6907,w:-2,h:17),
                _(0x6929,w:-2,h:12),
                _(0x6941,w:-2,h:14),
                _(0x695D,w:-2,h:15),
                _(0x697B,w:-2,h:14),
                s(0x6997), // 11 28 1B A4 A4 F6 A4 0D | 46 A5 28 A6 11 28 1C A4 
                _(0x6A57),
                _(0x7091,w:-3,h:30),
                _(0x70EB),
                _(0x7111,w:-3,h:7),
                _(0x7126,w:-3,h:9), // 1F 9F 0C F8 9F 04 F8 1F | 03 F8 1F 01 FC 9F 06 FC  ???
                _(0x7141,w:-2,h:13),
                _(0x715B),
                _(0x71C3,w:-3,h:22),
                s(0x7205), //02 2B 40 
                _(0x7208,w:-3,h:23),
                _(0x724D,w:-2,h:24),
                _(0x727D,w:-2,h:18),
                _(0x72A1,w:-3,h:23),
                _(0x72E6),
                s(0x7324), // 00 06 06 81 DC 8D DC  
                _(0x732B,w:-2,h:30),
                _(0x7367,w:-2,h:30),
                _(0x73A3,w:2,h:38),
                _(0x73EF,w:-3,h:26),
                _(0x743D,w:-3,h:23),
                s(0x7482), //BF 40 
                _(0x7484),
                s(0x74CA), //F3 F9 51 B9 02 02 CB 40 | 02 EB 40 02 EB 40 02 6B 
                _(0x74CD,w:-3,h:11),
                s(0x74EE),// 70 00 00 00 00 07 AE 00 | 1F 27 80 3F DF C0 7F 8F 
                _(0x74F3,w:-3,h:20),

                _(0x752F,w:-3,h:22), //mask for prev?!?
                _(0x7571,w:-2,h:24),
                _(0x75A1,w:-2,h:24), //mask for prev?
                _(0x75D1,w:-2,h:16),
                _(0x75F1,w:-2,h:23),
                _(0x761F,w:-2,h:18), //mask for prev?
                _(0x7643,w:-3,h:22),
                _(0x7685,w:3,h:22),
                _(0x76C7,w:-2,h:31),
                _(0x7705,w:2,h:31),
                _(0x7743,w:-2,h:27),
                _(0x7779,w:2,h:26),
                _(0x77AD,w:-2,h:26),
                _(0x77E1,w:2,h:25),
                _(0x7813,w:2,h:39),
                s(0x7861), //E3 08 0D 08 06 08 20 08 | A0 01 A8 04 46 03 86 01 
                _(0x787F,w:-3,h:18),
                _(0x78B5,w:3,h:18),
                _(0x78EB,w:-2,h:15),
                _(0x7909,w:-3,h:15), //40 1F F8 C0 3E F3 80 3D | 8
                _(0x7936,w:3,h:16), //80 07 00 F8 03 00 F8 01 
                _(0x7967,w:-2,h:21),
                _(0x7991,w:-2,h:12),
                _(0x79A9,w:-2,h:16),
                s(0x79C9), // 11 00 5B 21 E0 5D 01 21 | 27 ED B0 11 00 84 21 00 
                _(0x7A07,w:-2,h:20),
                _(0x7A2F,w:-2,h:23),
                _(0x7A5D,w:-3,h:26),
                _(0x7AAB,w:3,h:26),
                _(0x7AF9,w:-3,h:27),
                _(0x7B4A,w:3,h:27),
                _(0x7B9B,w:-2,h:24),
                _(0x7BCB,w:-2,h:13),
                _(0x7BE5,w:-2,h:23),
                _(0x7C13,w:-2,h:31),
                _(0x7C51,w:-2,h:30),
                _(0x7C8D,w:-2,h:30),
                _(0x7CC9),
                _(0x7E0D,w:-2,h:26),
                s(0x7e41), // 1E 76 B0 42 C1 0A 4C AE | C4 AE 21 28 0D 56 B5 86
                _(0x8013),
                _(0x815D,w:-3,h:30),
                _(0x8631),
                s(0x864F), // 21 28 13 9E D4 60 A7 0C | 5C D5 88 A7 09 4A A8 30 
                _(0x8661),
                s(0x8BBB),// 00 44 00 44 00 12 03 92 | 07 DA 07 DA 00 92 03 12 
                _(0x8C19),
                s(0x8D9D),// 11 14 05 8E CD 98 CD 0F | 3C CD 64 CD 00 00 02 20 
                _(0x8DF5),
                _(0x8E37), // bad height ?!? 83 18 00 00 0E 00 00 00
                _(0x8F5D),
                _(0x8F8F,w:2,h:21), // 0F CA F7 AC BB 54 BD 51 | DD 6B 5D AB
                _(0x8FB9),
                s(0x9009), // 21 28 17 C8 D3 2F DA 07 | 18 D4 5D DA 0A 4C A5 2E 
                _(0x921B),
                _(0x9433,w:-2,h:66), //merged images
                _(0x94B7,w:-2,h:41),
                _(0x9509,w:-2,h:92), //merged
                s(0x95C1), //03 03 16 06 12 15 08 03 | 03 03 03 03 03 03 03 03 
                _(0x95E1),
                _(0x965B,w:-3,h:40),
                _(0x96D3,w:-3,h:25),
                _(0x971E,w:-2,h:16),
                _(0x973E,w:-3,h:25),
                _(0x9789,w:-2,h:18),
                s(0x97AD), //13 90 10 10 7E 12 | 80 7E CB C0 1E EB C0 03 
                _(0x97E0,w:-2,h:19),
                _(0x9806,w:-2,h:20),
                _(0x982E,w:-2,h:8),
                _(0x983E,w:-3,h:25),
                _(0x9889,w:-2,h:17),
                s(0x98AB), //0B F0 13 90 10 10 
                _(0x98B1,w:-3,h:12),
                _(0x98D5,w:-2,h:21),
                _(0x98FF),
                _(0x99F1,w:-2,h:9),
                _(0x9A03,w:-3,h:17),
                _(0x9A36,w:-2,h:9),
                _(0x9A48,w:-2,h:43),
                _(0x9A9E,w:-2,h:16),
                _(0x9ABE), //BOM, ..etc total 264 items
                #endregion
            };
            return result;
        }
    }
}
