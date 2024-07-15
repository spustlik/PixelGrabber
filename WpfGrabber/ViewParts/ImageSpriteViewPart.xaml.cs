using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Linq;
using System.Xml.Serialization;
using WpfGrabber.Data;
using WpfGrabber.Services;
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

        #region Animate property
        private bool _animate;
        public bool Animate
        {
            get => _animate;
            set => Set(ref _animate, value);
        }
        #endregion

        #region AnimationSpeed property
        private int _animationSpeed;
        public int AnimationSpeed
        {
            get => _animationSpeed;
            set => Set(ref _animationSpeed, value);
        }
        #endregion

        #region Stretch property
        private bool _stretch;
        public bool Stretch
        {
            get => _stretch;
            set => Set(ref _stretch, value);
        }
        #endregion


        [XmlIgnore]
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


    public class ImageSpriteViewPartBase : ViewPartDataViewer<ImageSpritesVM>
    {
        public ImageSpriteViewPartBase() : base() { }
    }

    public partial class ImageSpriteViewPart : ImageSpriteViewPartBase
    {
        public static ViewPartDef<ImageSpriteViewPart> Def { get; } = new ViewPartDef<ImageSpriteViewPart>() { Title = "Image sprite sheet" };
        public DispatcherTimer AnimationTimer { get; private set; }

        public static RoutedUICommand CommandMoveUp = new RoutedUICommand("Up", nameof(CommandMoveUp), typeof(ImageSpriteViewPart), new InputGestureCollection() { new KeyGesture(Key.Left, ModifierKeys.Control) });
        public static RoutedUICommand CommandMoveDown = new RoutedUICommand("Down", nameof(CommandMoveDown), typeof(ImageSpriteViewPart), new InputGestureCollection() { new KeyGesture(Key.Right, ModifierKeys.Control) });
        public static RoutedUICommand CommandRemove = new RoutedUICommand("Remove", nameof(CommandRemove), typeof(ImageSpriteViewPart), new InputGestureCollection() { new KeyGesture(Key.Delete) });

        public ImageSpriteViewPart()
        {
            InitializeComponent();
        }

        public override void OnInitialize()
        {
            base.OnInitialize();
            CommandBindings.Add(new CommandBinding(CommandMoveUp, OnCommandMoveUp_Executed));
            CommandBindings.Add(new CommandBinding(CommandMoveDown, OnCommandMoveDown_Executed));
            CommandBindings.Add(new CommandBinding(CommandRemove, OnCommandRemove_Executed, (s, e) => e.CanExecute = ViewModel.SelectedImage != null));
            ViewModel.Columns = 1;
            ViewModel.ShowLabels = true;
            ViewModel.ShellVm = ShellVm;
            ViewModel.Stretch = true;
            ViewModel.AnimationSpeed = 25;
            AnimationTimer = new DispatcherTimer(DispatcherPriority.Normal, Dispatcher);
            AnimationTimer.Tick += AnimationTimer_Tick;
            //foreach (var file in Directory.GetFiles(@"E:\GameWork\8bitgames\alien8", "alien8.*.png"))
            //foreach (var file in Directory.GetFiles(@"E:\GameWork\_AssetsIso\kenney_isometric-roads\png\roads", "*.png"))
            //{
            //    var img = new BitmapImage();
            //    img.LoadFromFile(file);
            //    CreateImageVMFromFile(img, file);
            //}
        }

        public override void OnLoadLayout(XElement ele)
        {
            base.OnLoadLayout(ele);
            ele.LoadCollection(() => ViewModel.Images, (e) => e.LoadProperties(new ImageVM()));
            foreach (var img in ViewModel.Images)
            {
                if (img.Image != null)
                    continue;
                if (!File.Exists(img.FileName))
                    continue;
                CreateOrUpdateImageVMFromFile(img.FileName);
            }
        }
        public override void OnSaveLayout(XElement ele)
        {
            base.OnSaveLayout(ele);
            ele.SaveCollection(() => ViewModel.Images, (img, e) => e.SaveProperties(img));
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

            if (ViewModel.Animate)
            {
                var secs = 1.0 / ViewModel.AnimationSpeed;
                AnimationTimer.Interval = TimeSpan.FromSeconds(secs);
                AnimationTimer.Start();
            }
            else
            {
                AnimationTimer.Stop();
            }
        }
        private int animFrame = 0;
        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            var imgs = ViewModel.Images.Where(a => a.IsSelected).ToArray();
            if (imgs.Length <= 1)
                imgs = ViewModel.Images.ToArray();
            if (animFrame >= imgs.Length)
                animFrame = 0;
            ViewModel.SelectedImage = imgs[animFrame];
            animFrame++;
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
        private void OnAddImages_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Multiselect = true,
                DefaultExt = ".png",
                Filter = "*.png|*.png|*.jpg|*.jpg|*.*|*.*"
            };
            if (dlg.ShowDialog() != true)
                return;
            foreach (var file in dlg.FileNames)
            {
                CreateOrUpdateImageVMFromFile(file);
            }
        }
        private void OnReloadImages_Click(object sender, RoutedEventArgs e)
        {
            foreach (var file in ViewModel.Images.Select(a => a.FileName))
            {
                CreateOrUpdateImageVMFromFile(file);
            }
        }

        private ImageVM CreateOrUpdateImageVMFromFile(string file)
        {
            var imageVm = this.ViewModel.Images
                .FirstOrDefault(a => String.Compare(a.FileName, file, StringComparison.InvariantCultureIgnoreCase) == 0);
            if (imageVm == null)
            {
                imageVm = new ImageVM()
                {
                    FileName = file,
                    Name = Path.GetFileNameWithoutExtension(file)
                };
                ViewModel.Images.Add(imageVm);
            }
            var bmp = new BitmapImage();
            bmp.LoadFromFile(file);

            imageVm.Image = bmp;
            imageVm.Description = $"{imageVm.Name}\n{imageVm.FileName}\n{imageVm.Image.Width} x {imageVm.Image.Height}";
            return imageVm;
        }

        private void OnSave_Click(object sender, RoutedEventArgs e)
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

            var width = ViewModel.Width; //ViewModel.Images.Max(a => a.Image.PixelWidth)
            var height = ViewModel.Height;
            var bmpwidth = width;
            var bmpheight = height * ViewModel.Images.Count;
            if (ViewModel.Columns != 0)
            {
                bmpwidth = ViewModel.Columns * width;
                bmpheight = height * ViewModel.Images.Count / ViewModel.Columns;
            }

            var rgba = new ByteBitmapRgba(bmpwidth, bmpheight);

            int posX = 0;
            int posY = 0;
            var sources = new List<string>();
            foreach (var img in ViewModel.Images)
            {
                if (File.Exists(img.FileName))
                    sources.Add(img.FileName);
                var image = img.Image.ToRgba();
                rgba.DrawBitmap(image, posX * width, posY, Colorizers.GetColorCopy);
                posX++;
                if (posX >= ViewModel.Columns)
                {
                    posX = 0;
                    posY += ViewModel.Height;
                }
            }

            var result = rgba.ToBitmapSource();
            result.SaveToPngFile(dlg.FileName);
            var cols = ViewModel.Columns;
            if (cols == 0)
                cols = 1;
            var opts = $"W={width}, H={height}, Columns={cols}, Rows={ViewModel.Images.Count / cols}, Count={ViewModel.Images.Count}";
            if (MessageBox.Show(
                $"\n{opts}\nCopy to clipboard?",
                "Saved to spritesheet",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information) == MessageBoxResult.Yes)
            {
                Clipboard.SetText(opts);
            }
            if (sources.Count > 0)
            {
                if (MessageBox.Show(
                    $"Do you want to delete source images?",
                    "Confirmation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    foreach (var file in sources)
                    {
                        File.Delete(file);
                    }
                }
            }
        }

        private void OnCommandMoveUp_Executed(object sender, RoutedEventArgs e)
        {
            for (int i = 1; i < ViewModel.Images.Count; i++)
            {
                var img = ViewModel.Images[i];
                if (!img.IsSelected)
                    continue;
                ViewModel.Images.Move(i, i - 1);
            }
        }

        private void OnCommandMoveDown_Executed(object sender, RoutedEventArgs e)
        {
            for (int i = ViewModel.Images.Count - 2; i >= 0; i--)
            {
                var img = ViewModel.Images[i];
                if (!img.IsSelected)
                    continue;
                ViewModel.Images.Move(i, i + 1);
            }
        }

        private void OnCommandRemove_Executed(object sender, RoutedEventArgs e)
        {
            foreach (var img in ViewModel.Images.Where(x => x.IsSelected).ToArray())
            {
                ViewModel.Images.Remove(img);
            }
        }

        private void OnButtonClear_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Do you really want to remove all images?", "Question",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;
            this.ViewModel.Images.Clear();
        }

        private void OnSortBySize_Click(object sender, RoutedEventArgs e)
        {
            var sorted = ViewModel
                .Images
                .OrderBy(a => a.Image.Width)
                .ThenBy(a => a.Image.Height)
                .ThenBy(a => a.Name)
                .ToArray();
            ViewModel.Images.AddRange(sorted, clear: true);
        }

    }
}
