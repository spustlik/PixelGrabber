using ImageMagick;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml.Linq;
using System.Xml.Serialization;
using WpfGrabber.Services;
using WpfGrabber.Shell;

namespace WpfGrabber.ViewParts
{
    public class FileMapVM : SimpleDataObject
    {
        [XmlIgnore]
        public ObservableCollection<FileMapItem> Items { get; } = new ObservableCollection<FileMapItem>();

        #region SelectedItem property
        private FileMapItem _selectedItem;
        [XmlIgnore]
        public FileMapItem SelectedItem
        {
            get => _selectedItem;
            set => Set(ref _selectedItem, value);
        }
        #endregion

        #region SyncViewPart property
        private bool _syncViewPart = true;
        public bool SyncViewPart
        {
            get => _syncViewPart;
            set => Set(ref _syncViewPart, value);
        }
        #endregion


    }

    public class FileMapItem : SimpleDataObject
    {
        private static ViewPartDef[] definitions;
        public FileMapItem()
        {
            if (definitions == null)
                definitions = App.GetService<ViewPartFactory>().Definitions.ToArray();
        }

        #region Address property
        private int _address;
        public int Address
        {
            get => _address;
            set => Set(ref _address, value);
        }
        #endregion

        #region Size property
        private int _size;
        public int Size
        {
            get => _size;
            set => Set(ref _size, value);
        }
        #endregion

        #region Title property
        private string _title;
        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }
        #endregion

        #region Comment property
        private string _comment;
        public string Comment
        {
            get => _comment;
            set => Set(ref _comment, value);
        }
        #endregion

        #region ViewPartId property
        private string _viewPartId;
        public string ViewPartId
        {
            get => _viewPartId;
            set => Set(ref _viewPartId, value);
        }
        #endregion

        #region ViewPartLayout property
        private XElement _viewPartLayout = new XElement(nameof(ViewPartLayout));

        public XElement ViewPartLayout
        {
            get => _viewPartLayout;
            set => Set(ref _viewPartLayout, value);
        }
        #endregion

        #region ViewPartTitle property
        private string _viewPartTtile;
        [XmlIgnore]
        public string ViewPartTitle
        {
            get => _viewPartTtile;
            private set => Set(ref _viewPartTtile, value);
        }

        private void UpdateViewPartTitle()
        {
            ViewPartTitle = definitions.Where(x => x.TypeId == ViewPartId).Select(x => x.Title).FirstOrDefault();
        }

        #endregion
        protected override void DoPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if(propertyName == nameof(ViewPartId))
            {
                UpdateViewPartTitle();
            }
            base.DoPropertyChanged(propertyName);
        }

    }

    public class FileMapViewPartBase : ViewPartDataViewer<FileMapVM>
    {
    }

    /// <summary>
    /// Interaction logic for FileMapViewPart.xaml
    /// </summary>
    public partial class FileMapViewPart : FileMapViewPartBase
    {

        public FileMapViewPart()
        {
            AllViewParts = App.GetService<ViewPartFactory>().Definitions.ToArray();
            UpdateViewParts();
            InitializeComponent();
        }

        public ViewPartDef[] AllViewParts { get; }
        public ObservableCollection<ViewPartDef> ViewParts { get; } = new ObservableCollection<ViewPartDef>();

        private void UpdateViewParts()
        {
            //if used collectionview, it is automatically applied to all itemssource of AllItems!
            //ViewParts = CollectionViewSource.GetDefaultView(AllViewParts);
            //ViewParts.Filter = o => IsViewPartVisible(o as ViewPartDef);
            var visible = App.GetService<IViewPartServiceEx>().ViewParts;
            bool IsViewPartVisible(ViewPartDef def)
            {
                if (def.ViewPartType == GetType())
                    return false;
                if (ViewModel.SelectedItem?.ViewPartId == def.TypeId)
                    return true;
                return visible.Any(x => x.GetType() == def.ViewPartType);
            }
            ViewParts.Replace(AllViewParts.Where(IsViewPartVisible));
        }
        protected override void OnShowData()
        {
            base.OnShowData();
            UpdateViewParts();
        }
        protected override void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.SelectedItem))
            {
                UpdateViewParts();
                if (ViewModel.SelectedItem == null)
                    return;
                //combo first bind value, than source
                ViewModel.SelectedItem.RaisePropertyChange(nameof(ViewModel.SelectedItem.ViewPartId));
                ShellVm.Offset = ViewModel.SelectedItem.Address;
                if (ViewModel.SyncViewPart)
                    LoadItemViewPart();
            }
            base.ViewModel_PropertyChanged(sender, e);
        }

        public override void OnLoadLayout(XElement ele)
        {
            base.OnLoadLayout(ele);
            ele.LoadCollection(() => ViewModel.Items, (e) => e.LoadProperties(new FileMapItem()));
        }
        public override void OnSaveLayout(XElement ele)
        {
            base.OnSaveLayout(ele);
            ele.SaveCollection(() => ViewModel.Items, (item, e) => e.SaveProperties(item));
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var item = new FileMapItem() { Address = ShellVm.Offset, Title = "at " + ShellVm.Offset };
            this.ViewModel.Items.Add(item);
            this.SortItems();
            ViewModel.SelectedItem = item;
        }
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var i = ViewModel.Items.IndexOf(ViewModel.SelectedItem);
            ViewModel.Items.Remove(ViewModel.SelectedItem);
            if (i >= ViewModel.Items.Count)
                i = ViewModel.Items.Count - 1;
            ViewModel.SelectedItem = ViewModel.Items[i];
        }

        private void AddrFromOffset_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.SelectedItem.Address = ShellVm.Offset;
            this.SortItems();
        }

        private void SizeFromNext_Click(object sender, RoutedEventArgs e)
        {
            var next = ViewModel.Items.OrderBy(x => x.Address).FirstOrDefault(x => x.Address > this.ViewModel.SelectedItem.Address);
            if (next != null)
                this.ViewModel.SelectedItem.Size = next.Address - ViewModel.SelectedItem.Address;
            else
                this.ViewModel.SelectedItem.Size = ShellVm.DataLength - ViewModel.SelectedItem.Address;
        }

        private void Sort_Click(object sender, RoutedEventArgs e)
        {
            SortItems();
        }

        private void SortItems()
        {
            this.ViewModel.Items.Replace(ViewModel.Items.OrderBy(x => x.Address));
        }


        private void SaveViewPart_Click(object sender, RoutedEventArgs e)
        {
            SaveItemViewPart();
        }
        private void LoadViewPart_Click(object sender, RoutedEventArgs e)
        {
            LoadItemViewPart();
        }


        private ViewPart GetItemViewPart(FileMapItem item, bool create)
        {
            var def = AllViewParts.FirstOrDefault(p => p.TypeId == item.ViewPartId);
            if (def == null)
                return null;
            var part = App.GetService<IViewPartServiceEx>().ViewParts.FirstOrDefault(p => p.GetType() == def.ViewPartType);
            if (part == null && create)
                part = App.GetService<IViewPartServiceEx>().AddNewPart(def);
            return part;
        }
        private void SaveItemViewPart()
        {
            var part = GetItemViewPart(ViewModel.SelectedItem, create: false);
            if (part == null)
                return;
            part.OnSaveLayout(ViewModel.SelectedItem.ViewPartLayout);
        }

        private void LoadItemViewPart()
        {
            var item = ViewModel.SelectedItem;
            if (item == null || String.IsNullOrEmpty(item.ViewPartId))
                return;
            var part = GetItemViewPart(item, create: true);
            if (part == null)
                return;
            part.OnLoadLayout(item.ViewPartLayout);
        }


        private void AssignViewPart_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedItem == null)
                return;
            var mnu = sender as MenuItem;
            var def = mnu.DataContext as ViewPartDef;
            if (ViewModel.SelectedItem.ViewPartId == def.TypeId)
                return;
            ViewModel.SelectedItem.ViewPartId = def.TypeId;
            if (!ViewModel.SyncViewPart)
                return;
            SaveItemViewPart();
        }

        private const string SPACER = "» Space «";
        private void AddSpaces_Click(object sender, RoutedEventArgs e)
        {
            var i = 0;
            var lastPos = 0;
            var nosize = 0;
            ViewModel.Items.RemoveAll(x => x.Title == SPACER);

            //pridam space s velikosti minuly+minuly.velikost - aktualni
            //pokud je mezera 0, preskocim ji

            while (i < ViewModel.Items.Count)
            {
                var item = ViewModel.Items[i++];
                if (item.Size == 0)
                {
                    nosize++;
                }
                var space = item.Address - lastPos;
                if (space > 0 && space != item.Size)
                {
                    var newItem = new FileMapItem()
                    {
                        Address = lastPos,
                        Size = space,
                        Title = SPACER
                    };
                    ViewModel.Items.Insert(i-1, newItem);
                    //i++;
                }

                lastPos = item.Address + item.Size;
            }
            if (nosize > 0)
            {
                MessageBox.Show($"{nosize} items has no size", "Add Sizes", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

    }
}
