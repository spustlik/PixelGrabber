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
    public partial class ShellWindowFooter : UserControl
    {
        public ShellWindowFooter()
        {
            InitializeComponent();
        }

        public ShellVm ShellVm { get; private set; }
        public double[] ZoomList { get; private set; } = new double[] { 
                    0.25,
                    0.50,
                    0.75,
                    1.00,
                    1.25,
                    1.50,
                    2.00,
                    3.00,
                    4.00,
                    5.00 };

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ShellVm = App.Current.ServiceProvider.GetService<ShellVm>();
        }
        private void ZoomMinus_Click(object sender, RoutedEventArgs e)
        {
            var i = ZoomList.FindIndex(x => x == ShellVm.Zoom);
            if (i > 0)
            {
                ShellVm.Zoom = ZoomList[i - 1];
                return;
            }
            ShellVm.Zoom = ShellVm.Zoom / 1.5;
        }

        private void ZoomPlus_Click(object sender, RoutedEventArgs e)
        {
            var i = ZoomList.FindIndex(x => x == ShellVm.Zoom);
            if (i>0 && i+1 < ZoomList.Length)
            {
                ShellVm.Zoom = ZoomList[i + 1];
                return;
            }
            ShellVm.Zoom = ShellVm.Zoom * 1.5;
        }
    }
}
