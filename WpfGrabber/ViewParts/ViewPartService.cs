using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfGrabber.ViewParts
{
    public interface IViewPartService
    {
        void Add(ViewPart viewPart);
        void Remove(ViewPart viewPart);

    }
}
