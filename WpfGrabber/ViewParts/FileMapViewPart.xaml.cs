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
        [XmlIgnore]
        public ICollectionView ItemsView => CollectionViewSource.GetDefaultView(Items);
        public FileMapVM()
        {
            ItemsView.SortDescriptions.Add(new SortDescription(nameof(FileMapItem.Address), ListSortDirection.Ascending));
        }
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
            InitializeComponent();
        }
        public override void OnSaveLayout(XElement ele)
        {
            base.OnSaveLayout(ele);
            ele.SaveCollection(() => ViewModel.Items, (item, e) => e.SaveProperties(item));
        }
    }
}
