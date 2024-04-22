using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
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


        #region FlipVertical property
        private bool _flipVertical;
        public bool FlipVertical
        {
            get => _flipVertical;
            set => Set(ref _flipVertical, value);
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
            //0x4902
            var images = AlienReader.ReadList(ShellVm.Data, ShellVm.Offset,
                ViewModel.GetEndPosSafe(ShellVm.DataLength), flipY: ViewModel.FlipVertical);
            var max_h = this.GetFirstValid(imageBorder.ActualHeight, imageBorder.Height, Height, 300);
            max_h = (int)(max_h / ShellVm.Zoom);
            var max_w = this.GetFirstValid(imageBorder.ActualWidth, imageBorder.Width, Width, 500);
            max_w = (int)(max_w / ShellVm.Zoom);
            var rgba = new ByteBitmapRgba(max_w, max_h);
            var posX = 0;
            var posY = 0;
            const int XSPACER = 10;
            const int YSPACER = 0;
            int maxw = 0;
            foreach (var img in images.Select(a=>a.Bitmap))
            {
                if (posY + img.Height > max_h)
                {
                    posX += maxw + XSPACER;
                    posY = 0;
                    maxw = 0;
                }
                img.PutToBitmap(rgba, posX, posY);
                posY += img.Height + YSPACER;
                maxw = Math.Max(maxw, img.Width);
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
            var images = AlienReader.ReadList(ShellVm.Data, ShellVm.Offset, ViewModel.GetEndPosSafe(ShellVm.DataLength), flipY:ViewModel.FlipVertical);
            var id = 0;
            foreach ( var item in images)
            { 
                var img = item.Bitmap;
                var bmp = new ByteBitmapRgba(img.Width, img.Height);
                img.PutToBitmap(bmp, 0, 0);
                var bs = bmp.ToBitmapSource();
                var fileName = Path.ChangeExtension(dlg.FileName, $"{id:00}-{item.Position:X4}-{img.Width}x{img.Height}.png");
                bs.SaveToPngFile(fileName);
                id++;
            }
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

    }

}
