using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfGrabber
{
    public struct Interval
    {
        public int Min { get; private set; }
        public int Max { get; private set; }
        public Interval(int min, int max)
        {
            Min = min;
            Max = max;
        }

        public bool Contains(int value)
        {
            return value>=Min && value <= Max;
        }
    }
}
