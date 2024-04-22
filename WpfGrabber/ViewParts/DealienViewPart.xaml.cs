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
using WpfGrabber.Shell;

namespace WpfGrabber.ViewParts
{
    public class DealienViewPartVM : SimpleDataObject
    {
        #region EndPos property
        private int _endPos;
        public int EndPos
        {
            get => _endPos;
            set => Set(ref _endPos, value);
        }
        #endregion
    }
    public class DealienViewPartBase : ViewPartDataViewer<DealienViewPartVM>
    {
        public DealienViewPartBase() : base("Dealien") { }
    }

    /// <summary>
    /// Interaction logic for DealienViewPart.xaml
    /// </summary>
    public partial class DealienViewPart : DealienViewPartBase
    {
        public DealienViewPart()
        {
            InitializeComponent();
        }

        private void BorderSize_Changed(object sender, SizeChangedEventArgs e)
        {
            OnShowData();
        }
        protected override void OnShowData()
        {
            var end = ViewModel.EndPos;
            if (end == 0 || end > ShellVm.DataLength)
                end = ShellVm.DataLength-1;
            var images = AlienReader.ReadList(ShellVm.Data, ShellVm.Offset, end);
            var max_h = this.GetFirstValid(imageBorder.ActualHeight, imageBorder.Height, Height, 300);
            var max_w = this.GetFirstValid(imageBorder.ActualWidth, imageBorder.Width, Width, 500);
            var rgba = new ByteBitmapRgba(max_w, max_h);
            var posX = 0;
            var posY = 0;
            const int XSPACER = 10;
            const int YSPACER = 0;
            int maxw = 0;
            foreach (var img in images)
            {
                img.PutToBitmap(rgba, posX, posY);
                posY += img.Height + YSPACER;
                maxw = Math.Max(maxw, img.Width);
                if (posY > max_h)
                {
                    posX += maxw + XSPACER;
                    posY = 0;
                    maxw = 0;
                }
                if (posX > max_w)
                    break;
            }
            var bmp = rgba.ToBitmapSource();
            image.Source = bmp;
            image.RenderTransform = new ScaleTransform(ShellVm.Zoom, ShellVm.Zoom);
        }

        private void OnButtonSaveImages_Click(object sender, RoutedEventArgs e)
        {
            var r = MessageBox.Show("YES - save images to folder,NO - all images to one", "Save options...", MessageBoxButton.YesNoCancel);
            if (r == MessageBoxResult.Yes)
            {

            }
            else if (r == MessageBoxResult.No)
            {

            }
        }


    }

}
