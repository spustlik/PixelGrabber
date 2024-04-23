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
using WpfGrabber.Controls;
using WpfGrabber.ViewParts;

namespace WpfGrabber.Shell
{
    /// <summary>
    /// Interaction logic for ShellWindowContent.xaml
    /// </summary>
    public partial class ShellWindowContent : UserControl, IViewPartService
    {
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
        }

        #region IViewPartService
        private void UpdateGrid()
        {
            for (int i = 0; i < partsGrid.ColumnDefinitions.Count; i++)
            {
                var c = partsGrid.ColumnDefinitions[i];
                if (i == 0)
                    c.Width = new GridLength(1, GridUnitType.Star);
            }
            var last = partsGrid.ColumnDefinitions.LastOrDefault();
            //if(last!=null && last.Width.Value!=)
        }

        public IEnumerable<ViewPart> ViewParts => partsGrid.Children.OfType<ViewPartControl>().Select(x => x.Content).OfType<ViewPart>();
        private int GetViewPartControlIndex(ViewPart viewPart)
        {
            for (int i = 0; i < partsGrid.Children.Count; i++)
            {
                var vpc = partsGrid.Children[i] as ViewPartControl;
                if (vpc?.Content == viewPart)
                    return i;
            }
            return -1;
        }

        void IViewPartService.Add(ViewPart viewPart)
        {
            if (GetViewPartControlIndex(viewPart) >= 0)
                return;
            var vpc = new ViewPartControl();
            vpc.Content = viewPart;
            void add(Control c, GridLength width)
            {
                partsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = width });
                Grid.SetColumn(c, partsGrid.Children.Count);
                partsGrid.Children.Add(c);
            }
            add(new ViewPartControl() { Content = viewPart }, new GridLength(300));
            add(new GridSplitter(), GridLength.Auto);
            viewPart.OnInitialize();
            UpdateGrid();
        }

        void IViewPartService.Remove(ViewPart viewPart)
        {
            RemoveViewPart(viewPart);
        }

        private void RemoveViewPart(ViewPart viewPart)
        {
            var i = GetViewPartControlIndex(viewPart);
            if (i < 0)
                return;
            viewPart.OnClose();
            partsGrid.Children.RemoveAt(i); //remove viewPartControl
            partsGrid.Children.RemoveAt(i); //remove splitter
            UpdateGrid();
        }

        void IViewPartService.RemoveAll()
        {
            foreach (var vp in ViewParts.Reverse())
            {
                RemoveViewPart(vp);
            }
        }
        void IViewPartService.SetOptions(ViewPart viewPart, string title)
        {
            var i = GetViewPartControlIndex(viewPart);
            if (i < 0)
                return;
            var vpc = partsGrid.Children[i] as ViewPartControl;
            if (vpc == null)
                return;
            vpc.Title = title;
        }

        #endregion
    }
}
