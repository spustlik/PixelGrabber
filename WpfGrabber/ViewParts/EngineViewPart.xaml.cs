using Microsoft.Win32;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
        Filmation
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

        #region Columns property
        private int _columns;
        public int Columns
        {
            get => _columns;
            set => Set(ref _columns, value);
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

    }

    public class EngineImageVM
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public BitmapSource Image { get; set; }
        public ImageData ImageData { get; set; }
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
            ViewModel.Columns = 1;
            ViewModel.ShellVm = ShellVm;
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
                case EngineType.Movie: return ReadMovieImages();
                case EngineType.Alien: return ReadAlienImages();
                case EngineType.Filmation: return ReadFilmationImages();
                default:
                    throw new NotImplementedException();
            }
        }

        private IEnumerable<ImageData> ReadFilmationImages()
        {
            var rd = new DataReader(ShellVm.Data, ShellVm.Offset);
            var fr = new EngineFilmationReader(rd);
            return fr
                .ReadImages()
                .Select(x => CreateImageData(x));
        }

        private IEnumerable<ImageData> ReadAlienImages()
        {
            var rd = new DataReader(ShellVm.Data, ShellVm.Offset);
            var r = new EngineAlienReader(rd) { FlipY = ViewModel.FlipVertical };
            return r
                .ReadImages(0)
                .Select(x => CreateImageData(x));
        }



        private IEnumerable<ImageData> ReadMovieImages()
        {
            var marks = MovieMark.Marks;
            var rd = new DataReader(ShellVm.Data, ShellVm.Offset);
            var r = new EngineMovieReader(rd) { FlipVertical = ViewModel.FlipVertical, Width = ViewModel.Width };

            var fnt = AppData.GetFont();
            while (!rd.IsEmpty)
            {
                var found = marks.FirstOrDefault(a => a.offset == rd.BytePosition);
                if (found == null || found.IsImage)
                {
                    var img = r.ReadImage();
                    yield return CreateImageData(img);
                }
                else if (found.skip)
                {
                    var pos = rd.BytePosition;
                    var skip = found.skip;
                    var next = marks.NextItem(a => a.offset == found.offset);
                    var skipbytes = next.offset - pos;
                    rd.ReadBytes(skipbytes);
                    var bmp = new ByteBitmap8Bit(20, 10);
                    if (skipbytes > 8)
                        bmp = new ByteBitmap8Bit(30, 10);
                    bmp.DrawLine(0, 0, bmp.Width - 1, bmp.Height - 1);
                    bmp.DrawLine(bmp.Width - 1, 0, 0, bmp.Height - 1);
                    //bmp.DrawText(fnt, 0, 0, skipbytes.ToString(), 2);
                    var dummy = new ReaderImageResult(bmp, pos, pos + skipbytes);
                    var data = CreateImageData(dummy);
                    data.Overlay = skipbytes.ToString();
                    data.Description = $"(skip:0x{skip:X4}) {data.Description}";
                    yield return data;
                }
                else if (found.w != 0 && found.h != 0)
                {
                    var pos = rd.BytePosition;
                    var bmp = r.ReadBitmap(Math.Abs(found.w), found.h, readmask: false, flipX: found.w < 0, flipY: false);
                    var dummy = new ReaderImageResult(bmp, pos, rd.BytePosition);
                    var data = CreateImageData(dummy);
                    data.Description = $"(no header) {data.Description}";
                    yield return data;
                }
                else
                {
                    throw new NotImplementedException();
                }

            }
            //return r
            //    .ReadImages()
            //    .Select(x => CreateImageData(x));
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
                Description = $"Pos={r.Position:X4}, End={r.End:X4}, {dump}"
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
