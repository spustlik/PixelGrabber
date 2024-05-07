using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
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
            ViewModel.Images.Clear();

            foreach (var a in images)
            {
                var img = ByteBitmapRgba.FromBitmap(a.Bitmap);
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
            throw new NotImplementedException();
        }

        private IEnumerable<ImageData> ReadMovieImages()
        {
            //Movie game engine, but not complete - user must find sprite starts
            //height(16bit), data(h*WIDTH), mask reversed
            var rd = new DataReader(ShellVm.Data, ShellVm.Offset);
            //var w = ViewModel.Width;
            var counter = 0;
            while (!rd.IsEmpty)
            {
                var pos = rd.BytePosition;

                int w = rd.ReadByte();
                if (w == 0xc0)
                {
                    var unknown = rd.ReadByte(); //skip,  next is 0x8? or 0xC1
                    //6B3D: C0 00 C1 39 
                    w = rd.ReadByte();
                }

                int h = 0;
                var readmask = true;
                if ((w & 0b11000000) == 0b10000000)
                {
                    //highest bit(s?) can mean that there is mask & data, data otherwise
                    //0x81(w=2), 0x83 BOM (w=4), 0x82(0x98FF-man)
                    w = 1 + (byte)(w & 0b0111111);
                    h = rd.ReadByte();
                }
                else if ((w & 0b11000000) == 0b11000000)
                {
                    //0xC1, 0xC0
                    
                    if (w == 0xC1)
                    {
                        w = 1 + (byte)(w & 0b0011111);
                        h = rd.ReadByte();
                    }
                    else
                    {
                        //???
                        // E3
                        //F1,FB,F8
                    }
                    readmask = false;
                }
                else
                {
                    w = ViewModel.Width;
                    h = rd.ReadByte();
                }
                var bmp = new ByteBitmap8Bit(w * 8, h);
                var datar = new DataReader(rd.ReadBytes(w * h), 0, flipX: true);
                var maskr = new DataReader(rd.ReadBytes(w * h), 0, flipX: false);
                for (int y = 0; y < bmp.Height; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            var d = datar.ReadBit();
                            var m = readmask ? maskr.ReadBit() : false;
                            var ry = y;
                            if (ViewModel.FlipVertical)
                                ry = bmp.Height - y - 1;
                            //bmp.SetPixel(x * 8 + i, ry, (byte)(m ? 1 : 2));
                            bmp.SetPixel(x * 8 + i, ry, (byte)(m ? 1 : d ? 0 : 2));
                        }
                    }
                }
                var dumpBytes = rd.Data.Skip(pos).Take(8).ToArray();
                var dump = String.Join(" ", dumpBytes.Select(b => HexReader.ToHex(b, 2)));
                yield return new ImageData()
                {
                    Bitmap = bmp,
                    Addr = pos,
                    AddrEnd = rd.BytePosition,
                    Title = pos.ToString("X4"),
                    Description = $"Pos={pos:X4}, End={rd.BytePosition:X4}, {dump}"
                };
                counter++;
                if (ViewModel.MaxCount != 0 && counter >= ViewModel.MaxCount)
                    break;
            }
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
