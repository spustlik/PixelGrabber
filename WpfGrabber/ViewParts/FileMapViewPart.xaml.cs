using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.Xml.Serialization;
using WpfGrabber.Services;

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

    }
    public class FileMapItem : SimpleDataObject
    {

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
            this.ViewModel.Items.Add(new FileMapItem() { Address = 0, Title = "Start", Comment = "Start of file" });
            this.ViewModel.Items.Add(new FileMapItem() { Address = 1234, Title = "Some stuff", Comment = "There is something strange" });
            this.ViewModel.Items.Add(new FileMapItem() { Address = 2234, Title = "Main prog" });
            InitializeComponent();
        }
        public override void OnLoadLayout(XElement ele)
        {
            base.OnLoadLayout(ele);
        }
        public override void OnSaveLayout(XElement ele)
        {
            base.OnSaveLayout(ele);
            ele.SaveCollection(() => ViewModel.Items, (item, e) => e.SaveProperties(item));
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.Items.Add(new FileMapItem() { Address = ShellVm.Offset, Title = "at " + ShellVm.Offset });
            this.SortItems();
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
            this.ViewModel.Items.AddRange(ViewModel.Items.OrderBy(x => x.Address).ToArray(), clear: true);
        }

        private void ViewItem_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;
            var item = btn.DataContext as FileMapItem;
            if (item == null) return;
            ShellVm.Offset = item.Address;
            //TODO: update viewparts?
        }
    }
}
