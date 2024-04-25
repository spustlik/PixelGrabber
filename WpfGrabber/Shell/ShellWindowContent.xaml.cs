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
        private const int DEFAULT_WIDTH = 300;

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
            //fix column definitions to be same as children
            while (partsGrid.ColumnDefinitions.Count > partsGrid.Children.Count)
                partsGrid.ColumnDefinitions.Remove(partsGrid.ColumnDefinitions.Last());
            while (partsGrid.ColumnDefinitions.Count < partsGrid.Children.Count)
                partsGrid.ColumnDefinitions.Add(new ColumnDefinition() {  Width = new GridLength(DEFAULT_WIDTH) });
            //fix Width of coldef[0]
            //fix Grid.Column of control to index
            for (int i = 0; i < partsGrid.ColumnDefinitions.Count; i++)
            {
                var col = partsGrid.ColumnDefinitions[i];
                var con = partsGrid.Children[i];
                if (i == partsGrid.Children.Count-1)
                {
                    //last viewpart - star
                    col.Width = new GridLength(1, GridUnitType.Star);
                }
                else
                {
                    if(i%2==1)
                    {
                        //splitter
                        col.Width = GridLength.Auto;
                    }
                    else
                    {
                        //convert star viewpart to implicit width
                        if (col.Width.IsStar)
                            if (con is ViewPartControl vpc)
                                //if (!double.IsNaN(vpc.ActualWidth))
                                    col.Width = new GridLength(DEFAULT_WIDTH);
                    }
                }
                Grid.SetColumn(con, i);
            }
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

            //add ([splitter] if not first)[viewpart]
            if (partsGrid.Children.Count > 0)
                partsGrid.Children.Add(new GridSplitter());
            partsGrid.Children.Add(new ViewPartControl() { Content = viewPart });
            UpdateGrid();
            viewPart.OnInitialize();
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
