using Microsoft.Win32;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using WpfGrabber.Data;
using WpfGrabber.Readers;
using WpfGrabber.Shell;

namespace WpfGrabber.ViewParts
{
    public enum EngineType
    {
        Movie,
        Alien,
        Filmation,
        [Description("Gunfright (Filmation)")]
        Gunfright,
        Feud
    }
    public class EngineViewPartVM : SimpleDataObject
    {

        #region EngineType property
        private EngineType _engineType;
        public EngineType EngineType
        {
            get => _engineType;
            set => Set(ref _engineType, value);
        }
        #endregion

        #region Width property
        private int _width = 1;
        //only Movie
        public int Width
        {
            get => _width;
            set => Set(ref _width, value);
        }
        #endregion

        #region MaxCount property
        private int _maxCount;
        public int MaxCount
        {
            get => _maxCount;
            set => Set(ref _maxCount, value);
        }
        #endregion

        #region FlipVertical property
        private bool _flipVertical;
        //olny Movie and Alien
        public bool FlipVertical
        {
            get => _flipVertical;
            set => Set(ref _flipVertical, value);
        }
        #endregion

        [XmlIgnore]
        public ObservableCollection<EngineImageVM> Images { get; private set; } = new ObservableCollection<EngineImageVM>();

        #region SelectedImage property
        private EngineImageVM _selectedImage;
        [XmlIgnore]
        public EngineImageVM SelectedImage
        {
            get => _selectedImage;
            set => Set(ref _selectedImage, value);
        }
        #endregion

        #region ShellVm property
        private ShellVm _shellVm;
        [XmlIgnore]
        public ShellVm ShellVm
        {
            get => _shellVm;
            set => Set(ref _shellVm, value);
        }
        #endregion

        #region ShowLabels property
        private bool _showLabels;
        public bool ShowLabels
        {
            get => _showLabels;
            set => Set(ref _showLabels, value);
        }
        #endregion


        #region IsWidthVisible property
        private bool _isWidthVisible;
        public bool IsWidthVisible
        {
            get => _isWidthVisible;
            private set => Set(ref _isWidthVisible, value);
        }
        #endregion

        #region IsFlipVVisible property
        private bool _isFlipVVisible;
        public bool IsFlipVVisible
        {
            get => _isFlipVVisible;
            private set => Set(ref _isFlipVVisible, value);
        }
        #endregion

        protected override void DoPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.DoPropertyChanged(propertyName);
            if (propertyName == nameof(EngineType))
            {
                IsWidthVisible = EngineType == EngineType.Movie;
                IsFlipVVisible = EngineType == EngineType.Movie || EngineType == EngineType.Alien;
            }
        }
    }

    public class EngineImageVM
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public BitmapSource Image { get; set; }
        public ImageData ImageData { get; set; }
        public bool IsSelected { get; set; }
    }
    public class EngineViewPartBase : ViewPartDataViewer<EngineViewPartVM>
    {
        public EngineViewPartBase() : base() { }
    }

    public class ImageData
    {
        public ByteBitmap8Bit Bitmap { get; set; }
        public int Addr { get; set; }
        public int AddrEnd { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Overlay { get; set; }
    }

    public partial class EngineViewPart : EngineViewPartBase
    {
        public EngineViewPart()
        {
            InitializeComponent();
        }

        public override void OnInitialize()
        {
            base.OnInitialize();
            ViewModel.ShellVm = ShellVm;
            ViewModel.MaxCount = 1;
        }
        private void BorderSize_Changed(object sender, SizeChangedEventArgs e)
        {
            OnShowData();
        }
        protected override void OnShowData()
        {
            if (ViewModel.EngineType == EngineType.Movie && ShellVm.Offset == 1)
            {
                ShellVm.Offset = 0x3c3b;
                return;
            }

            var images = ReadImages();
            if (ViewModel.MaxCount != 0)
                images = images.Take(ViewModel.MaxCount);
            var fnt = AppData.GetFont();
            ViewModel.Images.Clear();
            foreach (var img in images)
            {
                var bmp = img.Bitmap.ToRgba(Colorizers.GetColor01Gray);
                if (img.Overlay != null)
                    bmp.DrawText(fnt, 1, 1, img.Overlay, Colorizers.GetColor(0x4040FF));
                var s = img.Description + $"\nWidth: {bmp.Width}, Height: {bmp.Height}";
                if (bmp.Width == 0 || bmp.Height == 0)
                    bmp = null;
                ViewModel.Images.Add(
                    new EngineImageVM() { Name = img.Title, ImageData = img, Image = bmp?.ToBitmapSource(), Description = s });
            }
        }

        private IEnumerable<ImageData> ReadImages()
        {
            switch (ViewModel.EngineType)
            {
                case EngineType.Movie: return ReadEngineImages(r => new EngineMovieReader(r) { FlipVertical = ViewModel.FlipVertical, Width = ViewModel.Width });
                case EngineType.Alien: return ReadEngineImages((r) => new EngineAlienReader(r) { FlipY = ViewModel.FlipVertical });
                case EngineType.Filmation: return ReadEngineImages((r) => new EngineFilmationReader(r));
                case EngineType.Gunfright: return ReadEngineImages((r) => new EngineGunfrightReader(r));
                case EngineType.Feud: return ReadEngineImages((r) => new EngineFeudReader(r) { });
                default:
                    throw new NotImplementedException();
            }
        }

        private IEnumerable<ImageData> ReadEngineImages<TR>(Func<DataReader, TR> factory) where TR : EngineReader
        {
            var rd = new DataReader(ShellVm.Data, ShellVm.Offset);
            var r = factory(rd);
            return r.ReadImages()
                .Select(x => CreateImageData(x));
        }

        private ImageData CreateImageData(ReaderImageResult r)
        {
            var dumpBytes = ShellVm.Data.Skip(r.Position).Take(8).ToArray();
            var dump = String.Join(" ", dumpBytes.Select(b => HexReader.ToHex(b, 2)));
            return new ImageData()
            {
                Bitmap = r.Bitmap,
                Addr = r.Position,
                AddrEnd = r.End,
                Title = r.Position.ToString("X4"),
                Overlay = r.Overlay,
                Description = (r.Description == null ? "" : r.Description + " ") + $"Pos={r.Position:X4}, End={r.End:X4}, {dump}"
            };
        }

        private void OnButtonSaveImages_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.DefaultExt = ".png";
            dlg.FileName = Path.GetFileName(Path.ChangeExtension(ShellVm.FileName, ".png"));
            if (dlg.ShowDialog() != true)
                return;
            foreach (var item in ViewModel.Images)
            {
                if (item.Image == null)
                    continue;
                var n2 = $"{item.ImageData.Addr:X4}";
                if (n2 == item.Name)
                    n2 = "";
                else
                    n2 = "-" + n2;
                var fileName = Path.ChangeExtension(dlg.FileName, $"{item.Name}{n2}-{item.Image.Width}x{item.Image.Height}.png");
                item.Image?.SaveToPngFile(fileName);
            }
        }


        private void OnGotoEnd_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedImage == null)
                return;
            ShellVm.Offset = ViewModel.SelectedImage.ImageData.AddrEnd;
        }

        private void OnGoto_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedImage == null)
                return;
            ShellVm.Offset = ViewModel.SelectedImage.ImageData.Addr;


        }
    }


}
