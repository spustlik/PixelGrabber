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
using System.Xml.Linq;
using WpfGrabber.Controls;
using WpfGrabber.Services;
using WpfGrabber.ViewParts;

namespace WpfGrabber.Shell
{
    public partial class ShellWindowContent : UserControl
        
    {
        //MORE IN partial classes !


        private const int DEFAULT_WIDTH = 300;
        private ViewPartFactory viewPartFactory;
        private ShellVm shellVm;

        public ShellWindowContent()
        {
            InitializeComponent();
            //there are data for design time
            partsGrid.Children.Clear();
            partsGrid.ColumnDefinitions.Clear();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            App.Current?.ServiceContainer?.AddService<IViewPartService>(this);
            viewPartFactory = App.Current?.ServiceProvider.GetService<ViewPartFactory>();
            shellVm = App.Current?.ServiceProvider.GetService<ShellVm>();
        }        
    }
}
