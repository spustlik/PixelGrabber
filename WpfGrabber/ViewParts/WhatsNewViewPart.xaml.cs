using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.Xml.Serialization;
using WpfGrabber.Readers;
using WpfGrabber.Shell;

namespace WpfGrabber.ViewParts
{
    public class WhatsNewVM : SimpleDataObject
    {
        //[XmlIgnore]
        //public ObservableCollection<string> Lines { get; private set; } = new ObservableCollection<string>();
    }

    public class WhatsNewViewPartBase : ViewPartDataViewer<WhatsNewVM>
    {

    }

    public partial class WhatsNewViewPart : WhatsNewViewPartBase
    {
        public static ViewPartDef Def { get; } = new ViewPartDef<WhatsNewViewPart>() { Title = "What is new" };

        public WhatsNewViewPart()
        {
            InitializeComponent();
        }

        public override void OnSaveLayout(XElement ele)
        {
            //base.OnSaveLayout(ele);
        }
        public override void OnLoadLayout(XElement ele)
        {
            //base.OnLoadLayout(ele);
        }

    }
}
