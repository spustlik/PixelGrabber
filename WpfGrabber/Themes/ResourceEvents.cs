using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace WpfGrabber.Themes
{
    public partial class ResourceEvents : ResourceDictionary
    {
        public ResourceEvents()
        {
            InitializeComponent();
        }

        public void ShowContextMenu_Click(object sender, RoutedEventArgs e)
        {
            ButtonBase b = ((ButtonBase)sender);
            //datacontext is null?!?!
            //var ctx = b.GetValue(ButtonBase.DataContextProperty);
            //b.ContextMenu.DataContext = b.DataContext;
            b.ContextMenu.PlacementTarget = b;
            b.ContextMenu.IsOpen = true;
        }
    }
}
