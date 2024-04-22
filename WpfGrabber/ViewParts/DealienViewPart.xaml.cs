using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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

        public int GetEndPosSafe(int dataLen)
        {
            var end = EndPos;
            if (end == 0 || end > dataLen)
                end = dataLen - 1;
            return end;
        }
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
            //487a
            var images = AlienReader.ReadList(ShellVm.Data, ShellVm.Offset, ViewModel.GetEndPosSafe(ShellVm.DataLength));
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
            var dlg = new SaveFileDialog();
            dlg.DefaultExt = ".png";
            dlg.FileName = Path.ChangeExtension(ShellVm.FileName, ".png");
            if (dlg.ShowDialog() != true)
                return;
            var images = AlienReader.ReadList(ShellVm.Data, ShellVm.Offset, ViewModel.GetEndPosSafe(ShellVm.DataLength));
            SaveAlienDataGroupedBySize(dlg.FileName, images);
        }

        private void OnButtonSave_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.DefaultExt = ".png";
            dlg.FileName = Path.ChangeExtension(ShellVm.FileName, "-data.png");
            if (dlg.ShowDialog() != true)
                return;
            ((BitmapSource)image.Source).SaveToPngFile(dlg.FileName);
        }

        private void SaveAlienDataGroupedBySize(string fileName, List<ByteBitmap8Bit> images)
        {
            foreach (var g in images.GroupBy(img => img.Width + "x" + img.Height))
            {
                var first = g.First();
                var bmp = new ByteBitmapRgba(first.Width, first.Height * g.Count());
                int posY = 0;
                for (var i = 0; i < g.Count(); i++)
                {
                    var src = g.Skip(i).First();
                    src.PutToBitmap(bmp, 0, posY);
                    posY += src.Height;
                }
                var bs = bmp.ToBitmapSource();
                bs.SaveToPngFile(Path.ChangeExtension(fileName, $"{first.Width}x{first.Height}-{g.Count()}.png"));
            }
        }
    }

}
