using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfGrabber.Shell;

namespace WpfGrabber.ViewParts
{
    public enum EngineType
    {
        Movie
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
    }
    public class EngineViewPartBase : ViewPartDataViewer<EngineViewPartVM>
    {
        public EngineViewPartBase() : base() { }
    }

    /// <summary>
    /// Interaction logic for DealienViewPart.xaml
    /// </summary>
    public partial class EngineViewPart : EngineViewPartBase
    {
        public EngineViewPart()
        {
            InitializeComponent();
        }

        public override void OnInitialize()
        {
            base.OnInitialize();
        }
        private void BorderSize_Changed(object sender, SizeChangedEventArgs e)
        {
            OnShowData();
        }
        protected override void OnShowData()
        {
            var fnt = AppData.GetFont();
            var images = ReadImages();
            var (max_w, max_h) = GetDataImageSize(imageBorder);
            var rgba = new ByteBitmapRgba(max_w, max_h);
            var posX = 0;
            var posY = 0;
            const int XSPACER = 10;
            const int YSPACER = 8;
            int maxw = 0;
            foreach (var a in images)
            {
                var img = a.Bitmap;
                if (posY + img.Height > max_h)
                {
                    posX += maxw + XSPACER;
                    posY = 0;
                    maxw = 0;
                }
                rgba.DrawBitmap(img, posX, posY);
                posY += img.Height;
                fnt.DrawString(rgba, posX, posY, a.Description);
                posY += YSPACER;

                maxw = Math.Max(maxw, img.Width);
                if (posX > max_w)
                    break;
            }
            var bmp = rgba.ToBitmapSource();
            image.Source = bmp;
            image.RenderTransform = new ScaleTransform(ShellVm.Zoom, ShellVm.Zoom);
        }

        private IEnumerable<(ByteBitmap8Bit Bitmap, int Addr, string Description)> ReadImages()
        {
            switch (ViewModel.EngineType)
            {
                case EngineType.Movie: return ReadMovieImages();
                default:
                    throw new NotImplementedException();
            }
        }

        private IEnumerable<(ByteBitmap8Bit Bitmap, int Addr, string Description)> ReadMovieImages()
        {
            //Movie game engine, but not complete - user must find sprite starts
            //height(16bit), data(h*WIDTH), mask reversed
            var rd = new DataReader(ShellVm.Data, ShellVm.Offset);
            //var w = ViewModel.Width;
            var counter = 0;
            while (!rd.IsEmpty)
            {
                var pos = rd.BytePosition;

                int w = rd.ReadByte();//0x81(w=2), 0x83 BOM (w=4)
                if ((w & 0b10000000) == 0b10000000)
                {
                    w = 1 + (byte)(w & 0b0111111);
                    //highest bit can mean that there is mask & data, data otherwise
                }
                else
                {
                    w = ViewModel.Width;
                }
                var h = rd.ReadByte();
                var bmp = new ByteBitmap8Bit(w * 8, h);
                var datar = new DataReader(rd.ReadBytes(w * h), 0, flipX: true);

                var maskr = new DataReader(rd.ReadBytes(w * h), 0, flipX: false);
                for (int y = 0; y < h+1; y++)
                {
                    for (int x = 0; x < w * 8; x++)
                    {
                        var d = datar.ReadBit();
                        var m = maskr.ReadBit();
                        var ry = y;
                        if (ViewModel.FlipVertical)
                            ry = h - y - 1;
                        bmp.SetPixel(x, ry, d ? (byte)0 : m ? (byte)1 : (byte)2);
                    }
                }
                yield return (Bitmap: bmp, Addr: pos, Description: $"Pos={pos:X4}, End={rd.BytePosition:X4}");
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
            /*
            var images = ReadImages();
            var id = 0;
            foreach (var item in images)
            {
                var img = item.Bitmap;
                var bmp = new ByteBitmapRgba(img.Width, img.Height);
                bmp.DrawBitmap(img, 0, 0);
                var bs = bmp.ToBitmapSource();
                var fileName = Path.ChangeExtension(dlg.FileName, $"{id:00}-{item.Position:X4}-{img.Width}x{img.Height}.png");
                bs.SaveToPngFile(fileName);
                id++;
            }
            */
        }

        private void OnButtonSave_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.DefaultExt = ".png";
            dlg.FileName = Path.ChangeExtension(ShellVm.FileName, "-data.png");
            if (dlg.ShowDialog() != true)
                return;
            ((BitmapSource)image.Source).SaveToPngFile(dlg.FileName);
        }

    }

}
