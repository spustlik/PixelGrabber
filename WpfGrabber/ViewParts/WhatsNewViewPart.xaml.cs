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

    public class WhatsNewViewPartBase : ViewPartDataViewer<HexDumpVM>
    {

    }

    public partial class WhatsNewViewPart : WhatsNewViewPartBase
    {

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

        protected override void ShellVm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Shell.ShellVm.Data)
                || e.PropertyName == nameof(Shell.ShellVm.Offset)
                )
            {
                base.ShellVm_PropertyChanged(sender, e);
            }
        }
        protected override void OnShowData()
        {
            if (ShellVm.DataLength <= 0)
                return;
            var rd = new HexReader(ShellVm.Data, ShellVm.Offset);
            var lines = rd.ReadLines(
                showAddr: ViewModel.ShowAddr,
                showAscii: ViewModel.ShowAscii,
                showHex: ViewModel.ShowHex)
                .Take(100)
                .ToArray();
            ViewModel.HexDump = string.Join("\n", lines);
            ViewModel.HexLines.AddRange(lines, clear: true);
        }

    }
}
