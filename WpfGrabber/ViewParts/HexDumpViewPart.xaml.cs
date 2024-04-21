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

namespace WpfGrabber.ViewParts
{
    /// <summary>
    /// Interaction logic for HexDumpViewPart.xaml
    /// </summary>
    public partial class HexDumpViewPart : ViewPart
    {
        public HexDumpViewPart()
        {
            Title = "Hex dump";
            DataContext = new VM();
            InitializeComponent();
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
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
            //TODO: read data and offset from shell/data service
        }

        public override void OnDataChanged(DataChangedArgs args)
        {
            base.OnDataChanged(args);
            var rd = new HexReader(args.Data, args.Offset);
            ViewModel.HexLines.AddRange(rd.ReadLines(
                showAddr: ViewModel.ShowAddr, 
                showAscii: ViewModel.ShowAscii, 
                showHex: ViewModel.ShowHex)
                .Take(100), clear: true);
        }

    }
}
