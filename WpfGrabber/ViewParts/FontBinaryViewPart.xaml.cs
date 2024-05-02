using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Xml;
using WpfGrabber.Data;
using WpfGrabber.Readers;
using WpfGrabber.Services;

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

        #region IsTestTextMultiLine property
        private bool _isTestTextMultiLine;
        public bool IsTestTextMultiLine
        {
            get => _isTestTextMultiLine;
            set => Set(ref _isTestTextMultiLine, value);
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

        #region Border property
        private int _border;
        public int Border
        {
            get => _border;
            set => Set(ref _border, value);
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
            if (ViewModel.Border > 0)
            {
                letters = letters.Select(ltr => AddBorder(ltr, ViewModel.Border, ViewModel.Border)).ToArray();
            }
            var font = new FontData(letters, ViewModel.SpaceX, ViewModel.FontCharacters);
            return font;
        }

        private static BitBitmap AddBorder(BitBitmap src, int bx, int by)
        {
            var r = new BitBitmap(src.WidthPixels + 2 * bx, src.Height + 2 * by);
            for (int y = 0; y < src.Height; y++)
            {
                for (int x = 0; x < src.WidthPixels; x++)
                {
                    r.SetPixel(bx + x, by + y, src.GetPixel(x, y));
                }
            }
            return r;
        }

        protected override void OnShowData()
        {
            var font = CreateFont();
            var (max_w, max_h) = GetDataImageSize(imageBorder);
            var rgba = new ByteBitmapRgba(max_w, max_h);
            font.DrawString(rgba, 10, 10, ViewModel.TestText);
            var posX = 0;
            var posY = 40;
            var maxX = max_w - 40;
            for (int i = 0; i < font.Letters.Length; i++)
            {
                BitBitmap letter = font.Letters[i];
                rgba.DrawBitmap(letter, posX, posY, ByteBitmapRgba.GetColorBlack);
                if (posX >= maxX)
                {
                    posX = 0;
                    posY += font.Letters.First().Height + 4;
                }
                else
                {
                    posX += font.Letters.First().WidthPixels;
                }
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
            var bmp = new ByteBitmapRgba(font.Letters.First().WidthPixels, font.Letters.Sum(l => l.Height));
            var y = 0;
            for (var i = 0; i < font.Letters.Count(); i++)
            {
                var letter = font.Letters[i];
                font.DrawLetter(bmp, 0, y, letter, 0xFFFFFFFF);
                y += letter.Height;
            }
            bmp.ToBitmapSource().SaveToPngFile(dlg.FileName);
        }

        private void OnButtonSaveBinary_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.DefaultExt = "bin";
            if (dlg.ShowDialog() != true)
                return;
            var font = CreateFont();
            using (var st = new FileStream(dlg.FileName, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                font.WriteToStream(st);
            }
        }

        private void OnButtonSaveSampleTextImage_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.DefaultExt = "png";
            dlg.FileName = Path.GetFileNameWithoutExtension(ShellVm.FileName) + "-text.png";
            if (dlg.ShowDialog() != true)
                return;
            var font = CreateFont();
            var bmp = new ByteBitmapRgba(ViewModel.TestText.Length * font.Letters.First().WidthPixels, font.Letters.Max(x=>x.Height));
            font.DrawString(bmp, 0, 0, ViewModel.TestText, 0xFFFFFFFF);
            bmp.ToBitmapSource().SaveToPngFile(dlg.FileName);
        }

        private void SetFontCharsAscii_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.FontCharacters = CharRange(' ', (char)126); // \u007f is some invisible char which is hardly to delete from string
        }

        private void SetFontCharsSimple_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.FontCharacters = CharRange('0', '9') + CharRange('A', 'Z');
        }

        private void SetAsciiTestText_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.TestText = CharRange(' ', (char)126);
        }

        private void SetLoremTestText_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.TestText = "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Nulla pulvinar eleifend sem.";
        }

    }
}
