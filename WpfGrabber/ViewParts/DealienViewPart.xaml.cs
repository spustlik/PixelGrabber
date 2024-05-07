using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

        public int GetEndPosSafe()
        {
            var end = EndPos;
            if (end == 0 || end > EndPosMax)
                end = EndPosMax - 1;
            return end;
        }

        #region EndPosMax property
        private int _endPosMax;
        public int EndPosMax
        {
            get => _endPosMax;
            set => Set(ref _endPosMax, value);
        }
        #endregion

        #region MaxCount property
        private int _maxCount;
        public int MaxCount
        {
            get => _maxCount;
            set => Set(ref _maxCount, value);
        }
        #endregion

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
        public DealienViewPartBase() : base() { }
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

        public override void OnInitialize()
        {
            base.OnInitialize();
            ViewModel.EndPosMax = ShellVm.DataLength;
        }
        private void BorderSize_Changed(object sender, SizeChangedEventArgs e)
        {
            OnShowData();
        }
        protected override void OnShowData()
        {
            //0x4902
            //0x466E
            if (ViewModel.EndPos < ShellVm.Offset)
                ViewModel.EndPos = ShellVm.Offset;
            if (ViewModel.EndPosMax > ShellVm.DataLength)
                ViewModel.EndPosMax -= ShellVm.DataLength;
            var images = ReadImages();
            var (max_w, max_h) = GetDataImageSize(imageBorder);
            var rgba = new ByteBitmapRgba(max_w, max_h);
            var posX = 0;
            var posY = 0;
            const int XSPACER = 10;
            const int YSPACER = 0;
            int maxw = 0;
            foreach (var img in images.Select(a => a.bitmap))
            {
                if (posY + img.Height > max_h)
                {
                    posX += maxw + XSPACER;
                    posY = 0;
                    maxw = 0;
                }
                rgba.DrawBitmap(img, posX, posY);
                posY += img.Height + YSPACER;
                maxw = Math.Max(maxw, img.Width);
                if (posX > max_w)
                    break;
            }
            var bmp = rgba.ToBitmapSource();
            image.Source = bmp;
            image.RenderTransform = new ScaleTransform(ShellVm.Zoom, ShellVm.Zoom);
        }

        private IEnumerable<(ByteBitmap8Bit bitmap, int position)> ReadImages()
        {
            var rd = new DataReader(ShellVm.Data, ShellVm.Offset);
            var r = new AlienReader(rd);
            var result = r.ReadImages(ViewModel.GetEndPosSafe(),flipY: ViewModel.FlipVertical);
            if (ViewModel.MaxCount > 0)
                result = result.Take(ViewModel.MaxCount);
            return result;
        }

        private void OnButtonSaveImages_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.DefaultExt = ".png";
            dlg.FileName = Path.ChangeExtension(ShellVm.FileName, ".png");
            if (dlg.ShowDialog() != true)
                return;
            var images = ReadImages();
            var id = 0;
            foreach (var item in images)
            {
                var img = item.bitmap;
                var bmp = new ByteBitmapRgba(img.Width, img.Height);
                bmp.DrawBitmap(img, 0, 0);
                var bs = bmp.ToBitmapSource();
                var fileName = Path.ChangeExtension(dlg.FileName, $"{id:00}-{item.position:X4}-{img.Width}x{img.Height}.png");
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
