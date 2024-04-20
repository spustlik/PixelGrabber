using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
                bytes = File.ReadAllBytes(ViewModel.FileName);
                ViewModel.DataLength = bytes.Length;
            }
            var reader = new BitReader(bytes);
            reader.Position = ViewModel.Offset;
            var w = ViewModel.Width;
            var total_w = GetNumber(imageBorder.ActualWidth, imageBorder.Width, Width);
            var total_h = GetNumber(imageBorder.ActualHeight, imageBorder.Height, this.Height);
            total_h = (int)(total_h / ViewModel.Zoom);
            //log.Text += $"H:{total_h},W:{total_w}, w={w}\n";

            var space = 10;
            if (w < 16)
                space = 4;
            BitmapSource bmp = CreateBitmap(reader, total_w, total_h, w, space);
            image.Source = bmp;
            image.RenderTransform = new ScaleTransform(ViewModel.Zoom, ViewModel.Zoom);
            var pos = ViewModel.Offset;
            ViewModel.HexLines.Clear();
            int rows = 0;
            while (pos < bytes.Length)
            {
                var line = new StringBuilder();
                for (int x = 0;  x < 16; x++)
                {
                    if (pos >= bytes.Length)
                        break;
                    var b = bytes[pos++];
                    line.Append(b.ToString("X2"));
                    line.Append(" ");
                    if (x == 7)
                        line.Append("| ");
                }
                ViewModel.HexLines.Add(line.ToString());
                rows++;
                if (rows > 100)
                    break;
            }
        }

        private BitmapSource CreateBitmap(BitReader reader, int total_w, int total_h, int w, int space)
        {
            var columnX = 0;
            var pixels = new uint[total_h * total_w];
            while (columnX + w <= total_w)
            {
                for (int y = 0; y < total_h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        var bit = reader.ReadBit();
                        if (reader.Position >= ViewModel.DataLength)
                            break;
                        var dest = y * total_w + x + columnX;
                        if (dest >= pixels.Length)
                            break;
                        pixels[dest] = bit ? 0xffffffff : 0xff000000;
                    }
                }
                columnX += w + space;
            }
            var bmp = BitmapSource.Create(total_w, total_h, 96, 96, PixelFormats.Pbgra32, null, pixels, total_w * 4);
            return bmp;
        }

        private int GetNumber(params double[] n)
        {
            foreach (var v in n)
            {
                if (double.IsNaN(v))
                    continue;
                if (v == 0)
                    continue;
                return (int)v;
            }
            return 0;
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
            var bmp2 = CreateBitmap(new BitReader(bytes) { Position = ViewModel.Offset },
                ViewModel.Width,
                (ViewModel.DataLength - ViewModel.Offset) / (ViewModel.Width / 8),
                ViewModel.Width, 10);
            bmp2.SaveToPngFile(Path.ChangeExtension(dlg.FileName, ".data.png"));
        }

        private void ButtonMask_Click(object sender, RoutedEventArgs e)
        {
            var w = new MaskBitmapWindow();
            w.Owner = this;
            ///w.ViewModel.FileName
            w.Show();
        }

        private void ButtonAlien_Click(object sender, RoutedEventArgs e)
        {
            var fileName = @"E:\GameWork\8bitgames\Alien8.rom";
            bytes = File.ReadAllBytes(fileName);
            //var pos = 0x4902;
            var pos = 0x466e;
            var endpos = 0x736e;
            ProcessAlien(fileName, pos, endpos);
        }

        private void ProcessAlien(string fileName, int pos, int endpos)
        {
            var ar = new AlienReader(bytes) { Position = pos };
            var images = new Dictionary<string, List<AlienImage>>();
            while (ar.Position < endpos)
            {
                var aimg = ar.ReadMaskedImage();
                var key = aimg.Width + "x" + aimg.Height;
                if (!images.TryGetValue(key, out var list))
                {
                    images.Add(key, list = new List<AlienImage>());
                }
                list.Add(aimg);
            }

            foreach (var pair in images)
            {
                var first = pair.Value[0];
                var bmp = new ByteBitmap(first.Width, first.Height * pair.Value.Count);
                int posY = 0;
                for (var i = 0; i < pair.Value.Count; i++)
                {
                    var src = pair.Value[i];
                    src.PutToBitmap(bmp, 0, posY);
                    posY += src.Height;
                }
                var bs = bmp.ToBitmapSource();
                bs.SaveToPngFile(Path.ChangeExtension(fileName, $"{first.Width}x{first.Height}.png"));
            }
        }
    }
}
