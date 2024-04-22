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
using System.Windows.Shapes;

namespace WpfGrabber._Legacy
{
    /// <summary>
    /// Interaction logic for DropDownTestWindow.xaml
    /// </summary>
    public partial class DropDownTestWindow : Window
    {
        public DropDownTestWindow()
        {
            InitializeComponent();
        }

        private void btnOpenCtx_Click(object sender, RoutedEventArgs e)
        {
            dd.ContextMenu.IsOpen = true;
        }
    }
}
