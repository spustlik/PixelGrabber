using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using System.Xml.Serialization;
using WpfGrabber.Data;
using WpfGrabber.Shell;

namespace WpfGrabber.ViewParts
{
    public class ImageSheetVM : SimpleDataObject
    {

        #region Columns property
        private int _columns;
        public int Columns
        {
            get => _columns;
            set => Set(ref _columns, value);
        }
        #endregion

        #region Rows property
        private int _rows;
        public int Rows
        {
            get => _rows;
            set => Set(ref _rows, value);
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

        #region Height property
        private int _height;
        public int Height
        {
            get => _height;
            set => Set(ref _height, value);
        }
        #endregion

        #region Count property
        private int _count;
        public int Count
        {
            get => _count;
            set => Set(ref _count, value);
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

        [XmlIgnore]
        public ObservableCollection<ImageSheetImageVM> Images { get; } = new ObservableCollection<ImageSheetImageVM>();

        #region SelectedImage property
        private ImageVM _selectedImage;
        [XmlIgnore]
        public ImageVM SelectedImage
        {
            get => _selectedImage;
            set => Set(ref _selectedImage, value);
        }
        #endregion

        #region CombinatorText property
        private string _combinatorText;
        public string CombinatorText
        {
            get => _combinatorText;
            set => Set(ref _combinatorText, value);
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
    }
    public class ImageSheetImageVM : SimpleDataObject
    {

        #region Name property
        private string _name;
        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
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
        private BitmapSource _image;
        [XmlIgnore]
        public BitmapSource Image
        {
            get => _image;
            set => Set(ref _image, value);
        }
        #endregion

        #region IsSelected property
        private bool _isSelected;
        [XmlIgnore]
        public bool IsSelected
        {
            get => _isSelected;
            set => Set(ref _isSelected, value);
        }
        #endregion
    }


    public class ImageSheetViewPartBase : ViewPartDataViewer<ImageSheetVM>
    {
        public ImageSheetViewPartBase() : base() { }
    }

    public partial class ImageSheetViewPart : ImageSheetViewPartBase
    {
        public ImageSheetViewPart()
        {
            InitializeComponent();
        }

        public override void OnInitialize()
        {
            base.OnInitialize();
            ViewModel.Columns = 1;
            ViewModel.ShowLabels = true;
            ViewModel.ShellVm = ShellVm;
        }

        private void BorderSize_Changed(object sender, SizeChangedEventArgs e)
        {
            OnShowData();
        }

        protected override void OnShowData()
        {
            if (ShellVm.Offset == 0 && Path.GetExtension(ShellVm.FileName).Equals(".psd", StringComparison.InvariantCultureIgnoreCase))
            {
                if (LoadPsd())
                    return;
            }

            var img = new BitmapImage();
            using (var ms = new MemoryStream(ShellVm.Data.Skip(ShellVm.Offset).ToArray()))
            {
                img.LoadFromStream(ms);
            }

            var rgba = img.ToRgba();

            var vm = ViewModel;
            //C,R,W,H,#
            var w = vm.Width;
            var h = vm.Height;
            if (vm.Columns != 0)
                w = rgba.Width / vm.Columns;
            if (vm.Rows != 0)
                h = rgba.Height / vm.Rows;

            var maxcount = vm.Count;
            if (maxcount != 0)
            {
                if (w == 0 && h != 0)
                    w = maxcount / (rgba.Height / h);
                if (h == 0 && w != 0)
                    h = maxcount / (rgba.Width / w);
            }
            if (w == 0)
                w = rgba.Width / 8;
            if (h == 0)
                h = rgba.Height / 8;

            var result = GetCroppedImages(rgba, w, h);
            if (maxcount > 0)
                result = result.Take(maxcount);
            ViewModel.Images.AddRange(result, clear: true);
            //images are viewed by Binding to Images collection
        }

        private bool LoadPsd()
        {
            //<package id="psd-parser" version="1.1.18124.1812" targetFramework="net48" />
            return false;
        }

        private IEnumerable<ImageSheetImageVM> GetCroppedImages(ByteBitmapRgba src, int w, int h)
        {
            int counter = 0;
            for (int y = 0; y < src.Height / h; y++)
            {
                for (var x = 0; x < src.Width / w; x++)
                {
                    var bmp = new ByteBitmapRgba(w, h);

                    Graphics.DrawBitmapFunctioned(w, h,
                        (sx, sy) => src.GetPixel(x * w + sx, y * h + sy),
                        (dx, dy, c) => bmp.SetPixel(dx, dy, c));
                    var vm = new ImageSheetImageVM()
                    {
                        Name = $"#{counter}", // ! combinator
                        Image = bmp.ToBitmapSource(),
                        Description = $"Column={x}, Row={y}, Index={counter}, Width={w}, Height={h}",
                    };
                    yield return vm;
                    counter++;
                }
            }
        }

        private void OnSaveImages_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Images.Count == 0)
                return;
            var dlg = new SaveFileDialog();
            dlg.DefaultExt = ".png";
            dlg.FileName = "Select folder";
            dlg.CheckFileExists = false;
            if (dlg.ShowDialog() != true)
                return;
            int i = 0;
            foreach (var vm in ViewModel.Images)
            {
                string fileName = Path.Combine(
                    Path.GetDirectoryName(dlg.FileName),
                    $"{Path.GetFileNameWithoutExtension(ShellVm.FileName)}-{i}.png");
                vm.Image.SaveToPngFile(fileName);
                i++;
            }
        }

        private void OnMoveToSprites_Click(object sender, RoutedEventArgs e)
        {
            var vps = App.GetService<IViewPartServiceEx>();
            var vp = vps.ViewParts.OfType<ImageSpriteViewPart>().FirstOrDefault();
            if (vp == null)
            {
                var vpf = App.GetService<ViewPartFactory>();
                var def = vpf.Definitions.First(d => d.ViewPartType == typeof(ImageSpriteViewPart));
                vp = (ImageSpriteViewPart)vps.AddNewPart(def);
            }
            vp.ViewModel.Images.AddRange(ViewModel.Images
                .Select(a => new ImageVM()
                {
                    Image = a.Image,
                    Name = a.Name,
                    Description = a.Description,
                    //FileName = 
                }), clear: true);
        }

        private void OnGenerateNames_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(ViewModel.CombinatorText?.Trim()))
            {
                if (MessageBox.Show("Replace current content ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    return;
            }
            ViewModel.CombinatorText = String.Join("\n", ViewModel.Images.Select((img, i) => (i + 1) + ":" + img.Name));
        }

        private void OnPreview_Click(object sender, RoutedEventArgs e)
        {
            //c1:a,b,c
            //c2:d,e,f
            //c3:a,e,f

            //--> ada, ade, adf,  aea, aee, aef, ...
            // no duplicate letter, so ad,ade,adf,ae,aef,...
        }

        private void OnGenerate_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
