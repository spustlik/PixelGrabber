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
using System.Xml.Serialization;
using WpfGrabber.Readers;
using WpfGrabber.Shell;

namespace WpfGrabber.ViewParts
{
    public class HexDumpVM : SimpleDataObject
    {

        #region ShowAddr property
        private bool _showAddr = true;
        public bool ShowAddr
        {
            get => _showAddr;
            set => Set(ref _showAddr, value);
        }
        #endregion

        #region ShowAscii property
        private bool _showAscii = true;
        public bool ShowAscii
        {
            get => _showAscii;
            set => Set(ref _showAscii, value);
        }
        #endregion

        #region ShowHex property
        private bool _showHex = true;
        public bool ShowHex
        {
            get => _showHex;
            set => Set(ref _showHex, value);
        }
        #endregion

        [XmlIgnore]
        public ObservableCollection<string> HexLines { get; private set; } = new ObservableCollection<string>();


        #region HexDump property
        private string _hexDump;
        [XmlIgnore]
        public string HexDump
        {
            get => _hexDump;
            set => Set(ref _hexDump, value);
        }
        #endregion

    }

    public class HexDumpViewPartBase : ViewPartDataViewer<HexDumpVM>
    {

    }

    public partial class HexDumpViewPart : HexDumpViewPartBase
    {

        public HexDumpViewPart()
        {
            InitializeComponent();
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
            var br = new DataReader(ShellVm.Data, ShellVm.Offset, flipX: false);
            var hexrd = new HexReader();
            var lines = hexrd.ReadLines(
                br,
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
