using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using WpfGrabber.Data;
using WpfGrabber.Readers;

namespace WpfGrabber.ViewParts
{
    public class FontBinaryVM : SimpleDataObject
    {

        #region FlipX property
        private bool _flipX;
        public bool FlipX
        {
            get => _flipX;
            set => Set(ref _flipX, value);
        }
        #endregion

        #region FlipY property
        private bool _flipY;
        public bool FlipY
        {
            get => _flipY;
            set => Set(ref _flipY, value);
        }
        #endregion

        #region SpaceX property
        private int _spaceX;
        public int SpaceX
        {
            get => _spaceX;
            set => Set(ref _spaceX, value);
        }
        #endregion

        #region Height property
        private int _height;
        public int Height
        {
            get => _height;
            set => Set(ref _height, value);
        }
        #endregion

        #region TestText property
        private string _testText;
        public string TestText
        {
            get => _testText;
            set => Set(ref _testText, value);
        }
        #endregion

        #region FontCharacters property
        private string _fontCharacters;
        public string FontCharacters
        {
            get => _fontCharacters;
            set => Set(ref _fontCharacters, value);
        }
        #endregion


    }

    public class FontBinaryViewPartBase : ViewPartDataViewer<FontBinaryVM>
    {

    }

    public partial class FontBinaryViewPart : FontBinaryViewPartBase
    {
        public FontBinaryViewPart()
        {
            InitializeComponent();
            ViewModel.Height = 8;
            ViewModel.FlipX = true;
            ViewModel.FontCharacters = CharRange('0', '9') + CharRange('A', 'Z');
            ViewModel.TestText = "HELLO WORLD";
        }
    
            
        private static string CharRange(char start, char end)
        {
            return String.Join("", Enumerable.Range((int)start, (int)end - (int)start + 1).Select(x => (char)x));
        }

        private void BorderSize_Changed(object sender, SizeChangedEventArgs e)
        {
            OnShowData();
        }

        private FontData CreateFont()
        {
            var br = new BitReader(ShellVm.Data) { BytePosition = ShellVm.Offset, FlipX = ViewModel.FlipX };
            var fr = new FontReader(ViewModel.Height)
            {
                FlipY = ViewModel.FlipY,
            };
            var letters = fr.ReadImages(br, ViewModel.FontCharacters.Length).ToArray();
            var font = new FontData(letters, ViewModel.SpaceX, ViewModel.FontCharacters);
            return font;
        }
        protected override void OnShowData()
        {
            var font = CreateFont();
            var (max_w, max_h) = GetDataImageSize(imageBorder);
            var rgba = new ByteBitmapRgba(max_w, max_h);
            font.DrawString(rgba, 10, 10, ViewModel.TestText);
            for (int i = 0; i < font.Letters.Length; i++)
            {
                rgba.DrawBitmap(font.Letters[i], i * 8, 40, ByteBitmapRgba.GetColorBlack);
            }
            var bmp = rgba.ToBitmapSource();
            image.Source = bmp;
            image.RenderTransform = new ScaleTransform(ShellVm.Zoom, ShellVm.Zoom);
        }

        private void OnButtonSaveImage_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.DefaultExt = "png";
            dlg.FileName = Path.GetFileNameWithoutExtension(ShellVm.FileName) + "-font.png";
            if (dlg.ShowDialog() != true)
                return;
            var font = CreateFont();
            var bmp = new ByteBitmapRgba(font.Letters.First().Width, font.Letters.Sum(l => l.Height));
            var y = 0;
            for (var i = 0; i < font.Letters.Count(); i++)
            {
                var letter = font.Letters[i];
                font.DrawLetter(bmp, 0, y, letter, 0xFFFFFFFF);
                y += letter.Height;
            }

            bmp.ToBitmapSource().SaveToPngFile(dlg.FileName);
        }
        private void OnButtonSaveSampleTextImage_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.DefaultExt = "png";
            dlg.FileName = Path.GetFileNameWithoutExtension(ShellVm.FileName) + "-text.png";
            if (dlg.ShowDialog() != true)
                return;
            var font = CreateFont();
            var bmp = new ByteBitmapRgba(ViewModel.TestText.Length * font.Letters.First().Width, font.Letters.Max(x=>x.Height));
            font.DrawString(bmp, 0, 0, ViewModel.TestText, 0xFFFFFFFF);
            bmp.ToBitmapSource().SaveToPngFile(dlg.FileName);

        }

        private void SetFontCharsAscii_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.FontCharacters = CharRange(' ', (char)127);
        }

        private void SetFontCharsSimple_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.FontCharacters = CharRange('0', '9') + CharRange('A', 'Z');
        }

        private void SetAsciiTestText_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.TestText = CharRange(' ', (char)127);
        }

        private void SetLoremTestText_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.TestText = "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Nulla pulvinar eleifend sem.";
        }


    }
}
