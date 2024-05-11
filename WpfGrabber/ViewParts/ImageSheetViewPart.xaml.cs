using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using System.Xml.Serialization;
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
        public override void OnLoadLayout(XElement ele)
        {
            base.OnLoadLayout(ele);
        }
        public override void OnSaveLayout(XElement ele)
        {
            base.OnSaveLayout(ele);
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

            //var bs = new BitmapSource();
            ////bs.lo
            //var bmp = new ByteBitmapRgba();
            //    bmp.lo
            //images are viewed by Binding to Images collection
        }

    }
}
