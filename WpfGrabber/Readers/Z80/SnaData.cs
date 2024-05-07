using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfGrabber.Readers.Z80
{
    //ZX-spectrum memory snapshot data file
    //https://sinclair.wiki.zxnet.co.uk/wiki/SNA_format
    public class SnaData
    {
        public SnaData(DataReader reader)
        {
            I = reader.ReadByte();
            HL_EX = reader.ReadWord16();
            DE_EX = reader.ReadWord16();
            BC_EX = reader.ReadWord16();
            AF_EX = reader.ReadWord16();
            HL = reader.ReadWord16();
            DE = reader.ReadWord16();
            BC = reader.ReadWord16();
            IY = reader.ReadWord16();
            IX = reader.ReadWord16();
            IFF2 = reader.ReadByte();
            R = reader.ReadByte();
            AF = reader.ReadWord16();
            SP = reader.ReadWord16();
            IM = reader.ReadByte();
            BorderColour = reader.ReadByte();
        }
        public int I { get; }
        public int HL_EX { get; }
        public int DE_EX { get; }
        public int BC_EX { get; }
        public int AF_EX { get; }

        public int HL { get; }
        public int DE { get; }
        public int BC { get; }
        public int IY { get; }
        public int IX { get; }
        public int IFF2 { get; }
        public int R { get; }
        public int AF { get; }
        public int SP { get; }
        public int IM { get; }
        public int BorderColour { get; }
    }
}
