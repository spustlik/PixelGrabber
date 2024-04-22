using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfGrabber.Readers;

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
            var src = new BitmapImage();
            src.LoadFromFile(ViewModel.FileName);
            var p = new MaskReader();
            var bmp = p.ProcessMask(src, ViewModel.Height, ViewModel.ItemsCount);
            image.RenderTransform = new ScaleTransform(ViewModel.Zoom, ViewModel.Zoom);
            image.Source = bmp;
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
            var rd = new MaskReader();
            var bmp = rd.ProcessMask(src, ViewModel.Height, ViewModel.ItemsCount);
            bmp.SaveToPngFile(dlg.FileName);
        }
    }
}
