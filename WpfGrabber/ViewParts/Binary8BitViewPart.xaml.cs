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

    public class Binary8BitViewPartBase : ViewPartDataViewer<Binary8BitVM>
    {

    }

    public partial class Binary8BitViewPart : Binary8BitViewPartBase
    {
        private ShellVm shellVm;
        public Binary8BitViewPart()
        {
            DataContext = new Binary8BitVM();
            ViewModel.Width = 64;
            ViewModel.Reversed = true;
            InitializeComponent();
        }


        private void BorderSize_Changed(object sender, SizeChangedEventArgs e)
        {
            ShowData();
        }

        protected override void OnShowData()
        {
            if (shellVm.DataLength == 0)
                return;
            var reader = new BitReader(shellVm.Data);
            reader.BytePosition = shellVm.Offset;
            reader.ReverseByte = ViewModel.Reversed;
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
            var bmp2 = bir.ReadBitmap(new BitReader(shellVm.Data) { BytePosition = shellVm.Offset },
                ViewModel.Width,
                (shellVm.DataLength - shellVm.Offset) / (ViewModel.Width / 8),
                ViewModel.Width, 10);
            bmp2.SaveToPngFile(dlg.FileName);
        }

    }
}
