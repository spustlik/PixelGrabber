using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfGrabber.Readers;

namespace WpfGrabber
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private byte[] bytes;
        public MainWindowViewModel ViewModel => this.DataContext as MainWindowViewModel;
        public MainWindow()
        {
            ExceptionWindow.StartHandlingExceptions();
            this.DataContext = new MainWindowViewModel();
            ViewModel.FileName = @"E:\GameWork\FEUD\FEUD1.COM";
            ViewModel.FileName = @"E:\GameWork\FEUD\FEUD.RAM";
            ViewModel.FileName = @"E:\GameWork\sord\STEPUP.rom";
            //ViewModel.FileName = @"E:\GameWork\8bitgames\SOROBO.COM";
            //ViewModel.FileName = @"E:\GameWork\8bitgames\KNIGHT.COM";
            Properties.Settings.Default.GetRecentFileNames(ViewModel.RecentFileNames);
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            this.Initialized += MainWindow_Initialized;
            InitializeComponent();
        }

        private void MainWindow_Initialized(object sender, EventArgs e)
        {
            DrawData();
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.FileName))
            {
                bytes = null;
            }
            DrawData();
        }

        private void DrawData()
        {
            if (bytes == null)
            {
                if (!File.Exists(ViewModel.FileName))
                {
                    ViewModel.RemoveRecent(ViewModel.FileName);
                }
                bytes = File.ReadAllBytes(ViewModel.FileName);
                ViewModel.AddRecent(ViewModel.FileName);
                ViewModel.DataLength = bytes.Length;
                Properties.Settings.Default.SetRecentFileNames(ViewModel.RecentFileNames);
                Properties.Settings.Default.Save();
            }
            var reader = new BitReader(bytes);
            reader.BytePosition = ViewModel.Offset;
            var w = ViewModel.Width;
            var total_w = this.GetFirstValid(imageBorder.ActualWidth, imageBorder.Width, Width);
            var total_h = this.GetFirstValid(imageBorder.ActualHeight, imageBorder.Height, this.Height);
            total_h = (int)(total_h / ViewModel.Zoom);
            //log.Text += $"H:{total_h},W:{total_w}, w={w}\n";

            var space = 10;
            if (w < 16)
                space = 4;
            var bir = new BitImageReader();
            BitmapSource bmp = bir.ReadBitmap(reader, total_w, total_h, w, space);
            image.Source = bmp;
            image.RenderTransform = new ScaleTransform(ViewModel.Zoom, ViewModel.Zoom);
            var pos = ViewModel.Offset;
            ViewModel.HexLines.Clear();
            DumpToHexLines(pos);
        }

        private void DumpToHexLines(int pos)
        {
            var r =  new HexReader(bytes, pos);
            ViewModel.HexLines.AddRange(r.ReadLines().Take(100));
            return;
        }

        private void ButtonOpen_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() != true)
                return;
            bytes = null;
            ViewModel.FileName = dlg.FileName;
            ViewModel.Offset = 0;
            DrawData();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.DefaultExt = "png";
            dlg.FileName = $"{Path.GetFileName(ViewModel.FileName)}-{ViewModel.Offset}-{ViewModel.Offset:X4}-{ViewModel.Width}";
            dlg.OverwritePrompt = true;
            if (dlg.ShowDialog(this) != true)
                return;
            ((BitmapSource)image.Source).SaveToPngFile(dlg.FileName);
            var bir = new BitImageReader();
            var bmp2 = bir.ReadBitmap(new BitReader(bytes) { BytePosition = ViewModel.Offset },
                ViewModel.Width,
                (ViewModel.DataLength - ViewModel.Offset) / (ViewModel.Width / 8),
                ViewModel.Width, 10);
            bmp2.SaveToPngFile(Path.ChangeExtension(dlg.FileName, ".data.png"));
        }






    }
}
