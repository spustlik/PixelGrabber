using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
using WpfGrabber.ViewParts;

namespace WpfGrabber.Shell
{
    /// <summary>
    /// Interaction logic for ShellWindowMenu.xaml
    /// </summary>
    public partial class ShellWindowMenu : UserControl
    {
        public ShellVm ShellVm { get; private set; }
        public ShellWindowMenu()
        {
            InitializeComponent();
        }
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ShellVm = App.Current?.ServiceProvider?.GetService<ShellVm>();
        }

        private void MainWindow_Click(object sender, RoutedEventArgs e)
        {
            var w = new MainWindow();
            w.Show();
        }

        private void OnOpenMenu_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() != true)
                return;
            ShellVm.LoadData(dlg.FileName);
        }

        private void OnExitMenu_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OnViewHexDump_Click(object sender, RoutedEventArgs e)
        {
            var part = new HexDumpViewPart();
            var vps = App.GetService<IViewPartService>();
            vps.Add(part);
        }

        private void OnRecentMenu_Click(object sender, RoutedEventArgs e)
        {
            var mi = (MenuItem)sender;
            var fn = (string)mi.DataContext;
            ShellVm.LoadData(fn);
        }

        private void OnTestData_Click(object sender, RoutedEventArgs e)
        {
            ShellVm.FileName = "...test data...";
            ShellVm.Offset = 0;
            var data = new List<byte>();
            data.AddRange(new byte[] {1,2,4,8,16,32,64,128});
            data.AddRange(new byte[] { 0x0a, 0x05, 0xa0, 0x50 });
            data.AddRange(Enumerable.Range(0, 8).Select((i, pos) => pos % 2 == 0 ? (byte)0xaa : (byte)0x55));
            data.AddRange(Enumerable.Range(0, 255).Select(i => (byte)i));
            ShellVm.SetData(data.ToArray());
        }
    }
}
