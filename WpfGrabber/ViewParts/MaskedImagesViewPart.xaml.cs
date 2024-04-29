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

        #region FlipBytes property
        private bool _flipBytes;
        public bool FlipBytes
        {
            get => _flipBytes;
            set => Set(ref _flipBytes, value);
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

        #region Colorizer property
        private MaskedImagesColorizer _colorizer;
        public MaskedImagesColorizer Colorizer
        {
            get => _colorizer;
            set => Set(ref _colorizer, value);
        }
        #endregion

    }

    public enum MaskedImagesColorizer
    {
        Color01,
        Color10,
        Color01Blue,
        Color10Blue,
        ColorA,
        ColorB,
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

            var font = AppData.GetFont();
            var posX = 0;
            var posY = 0;
            var maxW = 0;
            var colorizer = GetColorizer();
            foreach (var img in ReadImages())
            {
                rgba.DrawBitmap(img, posX, posY, colorizer);
                //font.DrawString(rgba, posX, posY, $"{img.Width}X{img.Height}", 0xFF4040FF);
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

        private ByteBitmapRgba.Colorizer GetColorizer()
        {
            switch (ViewModel.Colorizer)
            {
                case MaskedImagesColorizer.Color01:
                    return ByteBitmapRgba.GetColor01Gray;
                case MaskedImagesColorizer.Color10:
                    return ByteBitmapRgba.GetColor10Gray;
                case MaskedImagesColorizer.Color01Blue:
                    return (b, o) => b == 0 ? 0 : b == 1 ? 0xFFFFFFFF : 0xFF0000FF;
                case MaskedImagesColorizer.Color10Blue:
                    return (b, o) => b == 1 ? 0 : b == 0 ? 0xFFFFFFFF : 0xFF0000FF;
                case MaskedImagesColorizer.ColorA:
                    return (b, o) => b > 1 ? 0 : b == 0 ? 0xFFFFFFFF : 0xFF00FF00;
                case MaskedImagesColorizer.ColorB:
                    return (b, o) => b > 1 ? 0 : b == 1 ? 0xFFFFFFFF : 0xFF00FF00;
                default:
                    throw new NotImplementedException();
            }
        }

        private IEnumerable<ByteBitmap8Bit> ReadImages()
        {
            BitReader bitReader = new BitReader(ShellVm.Data)
            {
                BytePosition = ShellVm.Offset,
                FlipX = false //!!needed for preambule-reading
            };
            var rd = new MaskReader(bitReader)
            {
                FlipX = ViewModel.FlipX,
                FlipByte = ViewModel.FlipBytes,
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
