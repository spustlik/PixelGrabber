using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Xml.Serialization;
using WpfGrabber.Data;
using WpfGrabber.Readers;
using WpfGrabber.Shell;

namespace WpfGrabber.ViewParts
{
    public class ImageSpritesVM : SimpleDataObject
    {

        #region Columns property
        private int _columns;
        public int Columns
        {
            get => _columns;
            set => Set(ref _columns, value);
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

        #region Width property
        private int _width;
        public int Width
        {
            get => _width;
            set => Set(ref _width, value);
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

        public ObservableCollection<ImageVM> Images { get; } = new ObservableCollection<ImageVM>();

        #region SelectedImage property
        private ImageVM _selectedImage;
        [XmlIgnore]
        public ImageVM SelectedImage
        {
            get => _selectedImage;
            set => Set(ref _selectedImage, value);
        }
        #endregion
    }
    public class ImageVM : SimpleDataObject
    {
        #region Name property
        private string _name;
        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }
        #endregion

        #region FileName property
        private string _fileName;
        public string FileName
        {
            get => _fileName;
            set => Set(ref _fileName, value);
        }
        #endregion

        #region Description property
        private string _description;
        public string Description
        {
            get => _description;
            set => Set(ref _description, value);
        }
        #endregion

        #region Image property
        private BitmapImage _image;
        [XmlIgnore]
        public BitmapImage Image
        {
            get => _image;
            set => Set(ref _image, value);
        }
        #endregion
    }


    public class ImageSpriteViewPartBase : ViewPartDataViewer<ImageSpritesVM>
    {
        public ImageSpriteViewPartBase() : base() { }
    }

    public partial class ImageSpriteViewPart : ImageSpriteViewPartBase
    {
        public ImageSpriteViewPart()
        {
            InitializeComponent();
        }

        public override void OnInitialize()
        {
            base.OnInitialize();
            ViewModel.Columns = 1;
            ViewModel.ShowLabels = true;
            //foreach (var file in Directory.GetFiles(@"E:\GameWork\8bitgames\alien8", "alien8.*.png"))
            //foreach (var file in Directory.GetFiles(@"E:\GameWork\_AssetsIso\kenney_isometric-roads\png\roads", "*.png"))
            //{
            //    var img = new BitmapImage();
            //    img.LoadFromFile(file);
            //    CreateImageVMFromFile(img, file);
            //}
        }
        private void BorderSize_Changed(object sender, SizeChangedEventArgs e)
        {
            OnShowData();
        }

        protected override void OnShowData()
        {
            var (max_w, max_h) = GetDataImageSize(imageBorder);

            if (ViewModel.Width == 0 && ViewModel.Images.Count > 0)
            {
                ViewModel.Width = ViewModel.Images.Max(a => a.Image.PixelWidth);
                return;
            }
            if (ViewModel.Height == 0 && ViewModel.Images.Count > 0)
            {
                ViewModel.Height = ViewModel.Images.Max(a => a.Image.PixelHeight);
                return;
            }

            //images are viewed by Binding to Images collection
        }

        private BitmapImage TryGetBitmapFromData()
        {
            try
            {
                var dataBs = new BitmapImage();
                using (var s = new MemoryStream(ShellVm.Data, ShellVm.Offset, ShellVm.DataLength - ShellVm.Offset))
                {
                    //BitmapDecoder will also throw exceptions
                    dataBs.LoadFromStream(s);
                }
                return dataBs;
            }
            catch (Exception)
            {

            }
            return null;
        }

        private void OnSetMax_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Width = ViewModel.Images.Max(a => a.Image.PixelWidth);
            ViewModel.Height = ViewModel.Images.Max(a => a.Image.PixelHeight);
        }
        private void OnButtonAddImages_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Multiselect = true;
            dlg.DefaultExt = ".png";
            dlg.Filter = "*.png|*.png|*.jpg|*.jpg|*.*|*.*";
            if (dlg.ShowDialog() != true)
                return;
            foreach (var file in dlg.FileNames)
            {
                var img = new BitmapImage();
                img.LoadFromFile(file);
                CreateImageVMFromFile(img, file);
            }
        }

        private ImageVM CreateImageVMFromFile(BitmapImage img, string file)
        {
            var image = this.ViewModel.Images
                .FirstOrDefault(a => String.Compare(a.FileName, file, StringComparison.InvariantCultureIgnoreCase) == 0);
            if (image == null)
            {
                image = new ImageVM()
                {
                    FileName = file,
                    Name = Path.GetFileNameWithoutExtension(file)
                };
                ViewModel.Images.Add(image);
            }
            image.Image = img;
            image.Description = $"{image.Name}\n{image.FileName}\n{image.Image.Width} x {image.Image.Height}";
            return image;
        }

        private void OnButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Images.Count == 0)
                return;
            var dlg = new SaveFileDialog();
            dlg.DefaultExt = ".png";
            //dlg.InitialDirectory = Path.GetDirectoryName(ViewModel.FileName);
            //dlg.FileName = $"{Path.GetFileName(ShellVm.FileName)}-data-{ShellVm.Offset:X4}.{dlg.DefaultExt}";
            if (dlg.ShowDialog() != true)
                return;
            //save sheet
            var width = (ViewModel.Columns * ViewModel.Images.Max(a => a.Image.PixelWidth));
            var height = ViewModel.Height * ViewModel.Images.Count / ViewModel.Columns;

            var rgba = new ByteBitmapRgba(width, height);

            int posX = 0;
            int posY = 0;
            foreach (var img in ViewModel.Images)
            {
                var image = img.Image.ToRgba();
                rgba.DrawBitmap(image, posX, posY, Colorizers.GetColorCopy);
                posX++;
                if (posX >= ViewModel.Columns)
                {
                    posX = 0;
                    posY += ViewModel.Height;
                }
            }

            var result = rgba.ToBitmapSource();
            result.SaveToPngFile(dlg.FileName);
        }

        private void OnButtonUp_Click(object sender, RoutedEventArgs e)
        {
            var i = ViewModel.Images.FindIndex(x => x == ViewModel.SelectedImage);
            if (i <= 0)
                return;
            ViewModel.Images.Move(i, i - 1);
        }

        private void OnButtonDown_Click(object sender, RoutedEventArgs e)
        {
            var i = ViewModel.Images.FindIndex(x => x == ViewModel.SelectedImage);
            if (i < 0 || i + 1 >= ViewModel.Images.Count)
                return;
            ViewModel.Images.Move(i, i + 1);
        }

        private void OnButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            var i = ViewModel.Images.FindIndex(x => x == ViewModel.SelectedImage);
            if (i < 0)
                return;
            ViewModel.Images.RemoveAt(i);
        }

        private void OnButtonClear_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Do you really remove all images?", "Question", MessageBoxButton.YesNoCancel) != MessageBoxResult.Yes)
                return;
            this.ViewModel.Images.Clear();
        }
    }
}
