using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
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

        #region PosAddr property
        private int _posAddr;
        [XmlIgnore]
        public int PosAddr
        {
            get => _posAddr;
            set => Set(ref _posAddr, value);
        }
        #endregion

        #region PostOffset property
        private int _posOffset;
        public int PosOffset
        {
            get => _posOffset;
            set => Set(ref _posOffset, value);
        }
        #endregion

    }
    public class Binary8BitViewPartBase : ViewPartDataViewer<Binary8BitVM>
    {

    }

    public partial class Binary8BitViewPart : Binary8BitViewPartBase
    {
        public BitAddressFunction AddressFn { get; private set; }

        public Binary8BitViewPart()
        {
            DataContext = new Binary8BitVM();
            ViewModel.Width = 64;
            ViewModel.Reversed = true;
            InitializeComponent();
        }


        private void BorderSize_Changed(object sender, SizeChangedEventArgs e)
        {
            OnShowData();
        }

        protected override void OnShowData()
        {
            if (ShellVm.DataLength == 0)
                return;
            var reader = GetBitReader();

            var w = ViewModel.Width;
            var (max_w, max_h) = GetDataImageSize(imageBorder);

            var space = w < 16 ? 4 : 10;
            var bir = new BitImageReader();
            BitmapSource bmp = bir.ReadBitmap(reader, max_w, max_h, w, space);
            image.Source = bmp;
            image.RenderTransform = new ScaleTransform(ShellVm.Zoom, ShellVm.Zoom);
            AddressFn = bir.GetAddressFunction(max_w, max_h, w, space);
        }

        private DataReader GetBitReader()
        {
            var reader = new DataReader(ShellVm.Data, ShellVm.Offset, flipX: ViewModel.Reversed);
            return reader;
        }

        private BitmapSource ReadDataImage()
        {
            var reader = GetBitReader();
            var bir = new BitImageReader();
            var bmp = bir.ReadBitmap(reader,
                ViewModel.Width,
                (ShellVm.DataLength - ShellVm.Offset) / (ViewModel.Width / 8),
                ViewModel.Width, 10);
            return bmp;
        }

        private void OnButtonSave_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.DefaultExt = "png";
            dlg.OverwritePrompt = true;
            dlg.FileName = $"{Path.GetFileName(ShellVm.FileName)}-{ShellVm.Offset}-{ShellVm.Offset:X4}-{ViewModel.Width}";
            if (dlg.ShowDialog(App.Current.MainWindow) != true)
                return;
            ((BitmapSource)image.Source).SaveToPngFile(dlg.FileName);
        }

        private void OnButtonSaveData_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.DefaultExt = "png";
            dlg.OverwritePrompt = true;
            dlg.FileName = $"{Path.GetFileName(ShellVm.FileName)}-{ShellVm.Offset}-{ShellVm.Offset:X4}-{ViewModel.Width}.data.png";
            if (dlg.ShowDialog() != true)
                return;
            BitmapSource bmp = ReadDataImage();
            bmp.SaveToPngFile(dlg.FileName);
        }


        private void OnButtonCopyClipboard_Click(object sender, RoutedEventArgs e)
        {
            var bmp = ReadDataImage();
            Clipboard.SetImage(bmp);
        }

        private void OnImageBorder_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var pt = e.GetPosition(image);
            pt = new Point(
                (int)Math.Floor(pt.X * image.Source.Width / image.ActualWidth),
                (int)Math.Floor(pt.Y * image.Source.Height / image.ActualHeight));
            if (AddressFn == null)
                return;
            ViewModel.PosOffset = AddressFn((int)pt.X, (int)pt.Y);
            ViewModel.PosAddr = ShellVm.Offset + ViewModel.PosOffset;
        }

        private void OnImageBorder_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ViewModel.PosAddr == -1)
                return;
            ShellVm.Offset = ViewModel.PosAddr;
        }
    }
}
