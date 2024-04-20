using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WpfGrabber
{
    internal class BitReader
    {
        private readonly byte[] bytes;
        private int _posbit;
        public int Position
        {
            get
            {
                return _posbit / 8;
            }
            set
            {
                _posbit = value * 8;
            }
        }

        public bool ReverseByte { get; set; } = true;
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
    }
}
