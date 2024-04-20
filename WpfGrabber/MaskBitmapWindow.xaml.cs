using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfGrabber
{
    /// <summary>
    /// Interaction logic for MaskBitmapWindow.xaml
    /// </summary>
    public partial class MaskBitmapWindow : Window
    {
        public MaskBitmapWindowViewModel ViewModel => DataContext as MaskBitmapWindowViewModel;
        public MaskBitmapWindow()
        {
            DataContext = new MaskBitmapWindowViewModel();
            ViewModel.Zoom = 3;
            ViewModel.FileName = @"E:\GameWork\8bitgames\batman\data\batman.png";
            ViewModel.Height = 32;
            ViewModel.ItemsCount = 2;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            InitializeComponent();
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Redraw();
        }


        private void Window_Initialized(object sender, EventArgs e)
        {
            Redraw();
        }
        private void Redraw()
        {
            var src = new BitmapImage(new Uri(ViewModel.FileName));
            var bmp = ProcessMask(src, ViewModel.Height, ViewModel.ItemsCount);
            image.RenderTransform = new ScaleTransform(ViewModel.Zoom, ViewModel.Zoom);
            image.Source = bmp;
        }

        private WriteableBitmap ProcessMask(BitmapImage src, int itemHeight, int itemsCount)
        {
            int width = (int)src.Width;
            int height = (int)src.Height;
            var stride = width * 4;
            var srcpixels = new uint[width * height];
            src.CopyPixels(srcpixels, stride, 0);
            var bmp = new WriteableBitmap(width, height, 96, 96, PixelFormats.Pbgra32, null);
            var dstpixels = new uint[width * height];
            var srcposY = 0;
            var dstposY = 0;
            for (int i = 0; i < itemsCount; i++)
            {
                for (int y = 0; y < itemHeight; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        var sb = srcpixels[(srcposY + y) * width + x];
                        var sbm = srcpixels[(srcposY + itemHeight + y) * width + x];
                        if ((sbm & 0xFFFFFF) == 0)
                        {
                            dstpixels[(dstposY + y) * width + x] = sb;
                        }
                    }
                }
                srcposY += itemHeight * 2;
                dstposY += itemHeight;
            }
            bmp.WritePixels(new Int32Rect(0, 0, width, height), dstpixels, stride, 0);
            return bmp;
        }

        private void ButtonOpen_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.DefaultExt = "png";
            if (dlg.ShowDialog() != true)
                return;
            ViewModel.FileName = dlg.FileName;
            Redraw();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.DefaultExt = "png";
            dlg.FileName = Path.ChangeExtension(ViewModel.FileName, ".masked.png");
            if(dlg.ShowDialog() != true) 
                return;
            var src = new BitmapImage(new Uri(ViewModel.FileName));
            var bmp = ProcessMask(src, ViewModel.Height, ViewModel.ItemsCount);
            bmp.SaveToPngFile(ViewModel.FileName);
        }
    }
}
