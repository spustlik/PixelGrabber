using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfGrabber.Readers;
using WpfGrabber.Shell;

namespace WpfGrabber.ViewParts
{
    /// <summary>
    /// Interaction logic for Binary8BitViewPart.xaml
    /// </summary>
    public partial class Binary8BitViewPart : ViewPart
    {
        private ShellVm shellVm;
        public Binary8BitViewPart()
        {
            DataContext = new Binary8BitVM();
            ViewModel.Width = 64;
            ViewModel.Reversed = true;
            InitializeComponent();
        }

        public Binary8BitVM ViewModel => DataContext as Binary8BitVM;
        public class Binary8BitVM : SimpleDataObject
        {

            #region Reversed property
            private bool _reversed;
            public bool Reversed
            {
                get => _reversed;
                set => Set(ref _reversed, value);
            }
            #endregion

            #region Width property
            private int _width;
            public int Width
            {
                get => _width;
                set => Set(ref _width, value);
            }
            #endregion

        }
        public override void OnInitialize()
        {
            base.OnInitialize();
            shellVm = App.GetService<ShellVm>();
            shellVm.PropertyChanged += ShellVm_PropertyChanged;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            ShowData();
        }

        private void ShellVm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ShowData();
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ShowData();
        }
        private void BorderSize_Changed(object sender, SizeChangedEventArgs e)
        {
            ShowData();
        }

        private void ShowData()
        {
            if (shellVm.DataLength == 0)
                return;
            var reader = new BitReader(shellVm.Data);
            reader.Position = shellVm.Offset;
            var w = ViewModel.Width;
           
            var total_w = this.GetFirstValid(imageBorder.ActualWidth, imageBorder.Width, Width, 100);
            var total_h = this.GetFirstValid(imageBorder.ActualHeight, imageBorder.Height, Height,200);
            total_h = (int)(total_h / shellVm.Zoom);
            //log.Text += $"H:{total_h},W:{total_w}, w={w}\n";

            var space = w < 16 ? 4 : 10;
            var bir = new BitImageReader();
            BitmapSource bmp = bir.ReadBitmap(reader, total_w, total_h, w, space);
            image.Source = bmp;
            image.RenderTransform = new ScaleTransform(shellVm.Zoom, shellVm.Zoom);

        }

        private void OnButtonSave_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.DefaultExt = "png";
            dlg.OverwritePrompt = true;
            dlg.FileName = $"{Path.GetFileName(shellVm.FileName)}-{shellVm.Offset}-{shellVm.Offset:X4}-{ViewModel.Width}";
            if (dlg.ShowDialog(App.Current.MainWindow) != true)
                return;
            ((BitmapSource)image.Source).SaveToPngFile(dlg.FileName);        
        }

        private void OnButtonSaveData_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.DefaultExt = "png";
            dlg.OverwritePrompt = true;
            dlg.FileName = $"{Path.GetFileName(shellVm.FileName)}-{shellVm.Offset}-{shellVm.Offset:X4}-{ViewModel.Width}.data.png";
            var bir = new BitImageReader();
            var bmp2 = bir.ReadBitmap(new BitReader(shellVm.Data) { Position = shellVm.Offset },
                ViewModel.Width,
                (shellVm.DataLength - shellVm.Offset) / (ViewModel.Width / 8),
                ViewModel.Width, 10);
            bmp2.SaveToPngFile(dlg.FileName);
        }

    }
}
