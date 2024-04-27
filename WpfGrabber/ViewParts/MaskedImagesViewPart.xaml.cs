using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfGrabber.Readers;
using WpfGrabber.Shell;

namespace WpfGrabber.ViewParts
{
    public class MaskedImagesVM : SimpleDataObject
    {
        #region FlipX property
        private bool _flipX;
        public bool FlipX
        {
            get => _flipX;
            set => Set(ref _flipX, value);
        }
        #endregion

        #region FlipY property
        private bool _flipY;
        public bool FlipY
        {
            get => _flipY;
            set => Set(ref _flipY, value);
        }
        #endregion

        #region Height property
        private int _height = 32;
        public int Height
        {
            get => _height;
            set => Set(ref _height, value);
        }
        #endregion

        #region Width property
        private int _width = 3;
        public int Width
        {
            get => _width;
            set => Set(ref _width, value);
        }
        #endregion

        #region ReaderType property
        private MaskReaderType _readerType;
        public MaskReaderType ReaderType
        {
            get => _readerType;
            set => Set(ref _readerType, value);
        }
        #endregion

        #region Preambule property
        private MaskReaderPreambule _preambule;
        public MaskReaderPreambule Preambule
        {
            get => _preambule;
            set => Set(ref _preambule, value);
        }
        #endregion

    }

    public class MaskedImagesViewPartBase : ViewPartDataViewer<MaskedImagesVM>
    {
        public MaskedImagesViewPartBase() : base() { }
    }

    public partial class MaskedImagesViewPart : MaskedImagesViewPartBase
    {
        public MaskedImagesViewPart()
        {
            InitializeComponent();
        }

        private void BorderSize_Changed(object sender, SizeChangedEventArgs e)
        {
            OnShowData();
        }

        protected override void OnShowData()
        {
            var (max_w, max_h) = GetDataImageSize(imageBorder);
            var rgba = new ByteBitmapRgba(max_w, max_h);
            const int XSPACER = 10;
            const int YSPACER = 4;

            //var font = AppFont.Get();
            var posX = 0;
            var posY = 0;
            var maxW = 0;
            foreach (var img in ReadImages())
            {
                rgba.DrawBitmap(img, posX, posY, ByteBitmapRgba.GetColor01Gray);
                //font.DrawString(rgba, posX, posY, $"{img.Width}x{img.Height}");
                posY += img.Height + YSPACER;
                maxW = Math.Max(img.Width, maxW);
                if (posY >= max_h)
                {
                    posY = 0;
                    posX += maxW + XSPACER;
                    maxW = 0;
                }
            }
            var bmp = rgba.ToBitmapSource();
            image.Source = bmp;
            image.RenderTransform = new ScaleTransform(ShellVm.Zoom, ShellVm.Zoom);
        }

        private IEnumerable<ByteBitmap8Bit> ReadImages()
        {
            BitReader bitReader = new BitReader(ShellVm.Data)
            {
                BytePosition = ShellVm.Offset,
                //FlipX = ReverseByte
            };
            var rd = new MaskReader(bitReader)
            {
                FlipX = ViewModel.FlipX,
                FlipY = ViewModel.FlipY,
                Height = ViewModel.Height,
                Width = ViewModel.Width <= 0 ? 1 : ViewModel.Width,
                Type = ViewModel.ReaderType,
                Preambule = ViewModel.Preambule
            };
            while (rd.BitReader.BytePosition < rd.BitReader.DataLength)
            {
                var img = rd.Read();
                yield return img;
            }
        }
        private void OnButtonSaveImages_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.DefaultExt = ".png";
            dlg.FileName = "Select folder";
            if (dlg.ShowDialog() != true)
                return;
            int i = 0;
            foreach (var img in ReadImages())
            {
                if (img.Width <= 0 || img.Height <= 0 || img.Width >= 256 || img.Height >= 128)
                    continue;
                var rgba = ByteBitmapRgba.FromBitmap(img);
                string fileName = Path.Combine(
                    Path.GetDirectoryName(dlg.FileName),
                    $"{Path.GetFileNameWithoutExtension(dlg.FileName)}-{i:00}.png");
                rgba.ToBitmapSource().SaveToPngFile(fileName);
                i++;
            }
            ((BitmapSource)image.Source).SaveToPngFile(dlg.FileName);
        }

        private void OnButtonSave_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.DefaultExt = ".png";
            dlg.FileName = $"{Path.GetFileName(ShellVm.FileName)}-data-{ShellVm.Offset:X4}.{dlg.DefaultExt}";
            if (dlg.ShowDialog() != true)
                return;
            ((BitmapSource)image.Source).SaveToPngFile(dlg.FileName);
        }
    }
}
