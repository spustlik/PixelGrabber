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

namespace WpfGrabber.Shell
{
    /// <summary>
    /// Interaction logic for ShellWindowFooter.xaml
    /// </summary>
    public partial class ShellWindowFooter : UserControl
    {
        public ShellWindowFooter()
        {
            InitializeComponent();
        }

        public ShellVm ShellVm { get; private set; }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ShellVm = App.Current.ServiceProvider.GetService<ShellVm>();
        }
        private void ZoomMinus_Click(object sender, RoutedEventArgs e)
        {
            ShellVm.Zoom = ShellVm.Zoom / 2;
        }

        private void ZoomPlus_Click(object sender, RoutedEventArgs e)
        {
            ShellVm.Zoom = ShellVm.Zoom * 2;
        }
    }
}
