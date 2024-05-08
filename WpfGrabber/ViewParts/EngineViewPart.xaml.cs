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
        Alien
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
            var fnt = AppData.GetFont();
            var images = ReadImages();
            if (ViewModel.MaxCount != 0)
                images = images.Take(ViewModel.MaxCount);

            ViewModel.Images.Clear();
            foreach (var a in images)
            {
                var img = a.Bitmap.ToRgba();
                var s = a.Description + $"\nWidth: {img.Width}, Height: {img.Height}";
                if (img.Width == 0 || img.Height == 0)
                    img = null;
                ViewModel.Images.Add(
                    new EngineImageVM() { Name = a.Title, ImageData = a, Image = img?.ToBitmapSource(), Description = s });
            }
        }

        private IEnumerable<ImageData> ReadImages()
        {
            switch (ViewModel.EngineType)
            {
                case EngineType.Movie: return ReadMovieImages();
                case EngineType.Alien: return ReadAlienImages();
                default:
                    throw new NotImplementedException();
            }
        }

        private IEnumerable<ImageData> ReadAlienImages()
        {
            var rd = new DataReader(ShellVm.Data, ShellVm.Offset);
            var r = new AlienReader(rd) { FlipY = ViewModel.FlipVertical };
            return r
                .ReadImages(0)
                .Select(x => CreateImageData(x));
        }

        internal class MovieMark
        {
            public int offset;
            public int len;
            public int w;
            public int h;
            public int skip;
        }
        private IEnumerable<ImageData> ReadMovieImages()
        {
            var rd = new DataReader(ShellVm.Data, ShellVm.Offset);
            var r = new MovieReader(rd) { FlipVertical = ViewModel.FlipVertical, Width = ViewModel.Width };

            var eimages = r.ReadImages().GetEnumerator();
            MovieMark o(int ofs, int len = 0, int w = 0, int h = 0, int skip = 0)
            {
                return new MovieMark() { offset = ofs, len = len, w = w, h = h, skip = skip };
            }
            var offsets = new[]
            {
                o(0x3c3b),
                //o(0x3e15),
                o(0x3EB5, skip:0x447B-0x3EB5),
                o(0x447B,w:32,h:24), //flip byte
            }.ToList();
            var eoffsets = offsets.GetEnumerator();
            //eimages.Reset();
            eimages.MoveNext();
            //(eoffsets as IEnumerator).Reset();
            eoffsets.MoveNext();
            int counter = 0;
            while (!rd.IsEmpty)
            {
                if (eoffsets.Current == null ||
                    (eimages.Current != null && eimages.Current.position <= eoffsets.Current?.offset))
                {
                    if (eimages.Current.position == eoffsets.Current?.offset)
                    {
                        eoffsets.MoveNext();
                    }
                    eimages.MoveNext();
                    yield return CreateImageData(eimages.Current);
                }
                else if (eoffsets.Current != null)
                {
                    //if (eoffsets.Current.w != 0 && eoffsets.Current.h != 0)
                    //{
                    //    var bmp = new ByteBitmap8Bit(eoffsets.Current.w, eoffsets.Current.h);
                    //    eoffsets.MoveNext();
                    //    var dummy=new ReaderImageResult(bmp, eoffsets.Current.offset);
                    //    yield return CreateImageData(dummy);
                    //}
                    //else
                    {
                        var bmp = new ByteBitmap8Bit(20, 20);
                        bmp.DrawLine(0, 0, bmp.Width, bmp.Height);
                        bmp.DrawLine(bmp.Width, 0, 0, bmp.Height);
                        var len = eoffsets.Current.skip;
                        if (len == 0)
                            len = eoffsets.Current.w * eoffsets.Current.h;
                        if (len == 0)
                            len = 1;
                        var dummy = new ReaderImageResult(bmp, eoffsets.Current.offset, eoffsets.Current.offset + len);
                        eoffsets.MoveNext();
                        var data = CreateImageData(dummy);
                        data.Title = $"({data.Title})";
                        yield return data;
                    }
                }
                counter++;
                if (ViewModel.MaxCount > 0 && counter > ViewModel.MaxCount)
                    break;
            }
            //return r
            //    .ReadImages()
            //    .Select(x => CreateImageData(x));
        }

        private ImageData CreateImageData(ReaderImageResult r)
        {
            var dumpBytes = ShellVm.Data.Skip(r.position).Take(8).ToArray();
            var dump = String.Join(" ", dumpBytes.Select(b => HexReader.ToHex(b, 2)));
            return new ImageData()
            {
                Bitmap = r.bitmap,
                Addr = r.position,
                AddrEnd = r.end,
                Title = r.position.ToString("X4"),
                Description = $"Pos={r.position:X4}, End={r.end:X4}, {dump}"
            };
        }

        private void OnButtonSaveImages_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.DefaultExt = ".png";
            dlg.FileName = Path.ChangeExtension(ShellVm.FileName, ".png");
            if (dlg.ShowDialog() != true)
                return;
            foreach (var item in ViewModel.Images)
            {
                var fileName = Path.ChangeExtension(dlg.FileName, $"{item.Name}-{item.ImageData.Addr:X4}-{item.Image.Width}x{item.Image.Height}.png");
                item.Image?.SaveToPngFile(fileName);
            }
        }

        private void OnGotoEnd_Click(object sender, RoutedEventArgs e)
        {
            //originalSource as MenuItem
            if (ViewModel.SelectedImage == null)
                return;
            ShellVm.Offset = ViewModel.SelectedImage.ImageData.AddrEnd;
        }
    }


}
