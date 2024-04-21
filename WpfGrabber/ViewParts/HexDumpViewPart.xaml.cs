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
using WpfGrabber.Readers;
using WpfGrabber.Shell;

namespace WpfGrabber.ViewParts
{
    /// <summary>
    /// Interaction logic for HexDumpViewPart.xaml
    /// </summary>
    public partial class HexDumpViewPart : ViewPart
    {
        private ShellVm shellVm;

        public HexDumpViewPart()
        {
            Title = "Hex dump";
            DataContext = new VM();
            InitializeComponent();
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }
        public override void OnInitialize()
        {
            base.OnInitialize();
            shellVm = App.GetService<ShellVm>();
            shellVm.PropertyChanged += ShellVm_PropertyChanged;
            ShowData();
        }

        public VM ViewModel => DataContext as VM;
        public class VM : SimpleDataObject
        {

            #region ShowAddr property
            private bool _showAddr;
            public bool ShowAddr
            {
                get => _showAddr;
                set => Set(ref _showAddr, value);
            }
            #endregion

            #region ShowAscii property
            private bool _showAscii;
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

            public ObservableCollection<string> HexLines { get; private set; } = new ObservableCollection<string>();
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ShowData();
        }
        private void ShellVm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ShellVm.Data)
                || e.PropertyName == nameof(ShellVm.Offset)
                )
            {
                ShowData();
            }
        }


        private void ShowData()
        {
            if (shellVm.DataLength <= 0)
                return;
            var rd = new HexReader(shellVm.Data, shellVm.Offset);
            ViewModel.HexLines.AddRange(rd.ReadLines(
                showAddr: ViewModel.ShowAddr,
                showAscii: ViewModel.ShowAscii,
                showHex: ViewModel.ShowHex)
                .Take(100), clear: true);
        }

        public override void OnDataChanged(DataChangedArgs args)
        {
            base.OnDataChanged(args);
        }

    }
}
