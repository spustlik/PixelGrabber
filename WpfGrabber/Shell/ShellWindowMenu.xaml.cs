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
    }
}
