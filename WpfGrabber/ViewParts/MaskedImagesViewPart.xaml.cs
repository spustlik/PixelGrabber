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
using WpfGrabber.Readers;

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

        #region WidthBytes property
        private int _widthBytes = 3;
        public int WidthBytes
        {
            get => _widthBytes;
            set => Set(ref _widthBytes, value);
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
            var posX = 0;
            var posY = 0;
            const int XSPACER = 10;
            const int YSPACER = 4;

            var rd = new MaskReader(ShellVm.Data)
            {
                FlipX = ViewModel.FlipX,
                FlipY = ViewModel.FlipY,
                Height = ViewModel.Height,
                WidthBytes = ViewModel.WidthBytes <=0 ? 1 : ViewModel.WidthBytes,
                Position = ShellVm.Offset
            };
            while (rd.Position < rd.DataLength)
            {
                var img = rd.Read();
                rgba.DrawBitmap(img, posX, posY, ByteBitmapRgba.GetColor01Gray);
                posY += img.Height + YSPACER;
                if(posY>=max_h)
                {
                    posY = 0;
                    posX += img.Width + XSPACER;
                }
            }
            var bmp = rgba.ToBitmapSource();
            image.Source = bmp;
            image.RenderTransform = new ScaleTransform(ShellVm.Zoom, ShellVm.Zoom);
        }

        private void OnButtonSaveImages_Click(object sender, RoutedEventArgs e)
        {
            //TODO:
        }

        private void OnButtonSave_Click(object sender, RoutedEventArgs e)
        {
            //TODO:
        }
    }
}
