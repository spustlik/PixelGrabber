using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfGrabber
{
    public class MaskBitmapWindowViewModel : SimpleDataObject
    {

        #region Height property
        private int _height;
        public int Height
        {
            get => _height;
            set => Set(ref _height, value);
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

        #region ItemsCount property
        private int _itemsCount;
        public int ItemsCount
        {
            get => _itemsCount;
            set => Set(ref _itemsCount, value);
        }
        #endregion

        #region Zoom property
        private double _zoom;
        public double Zoom
        {
            get => _zoom;
            set => Set(ref _zoom, value);
        }
        #endregion


    }
}
