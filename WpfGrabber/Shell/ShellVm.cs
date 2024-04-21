using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfGrabber.Shell
{
    public class ShellVm : SimpleDataObject
    {
        public ShellVm()
        {
            Zoom = 1;       
        }

        #region FileName property
        private string _fileName;
        public string FileName
        {
            get => _fileName;
            set => Set(ref _fileName, value);
        }
        #endregion

        #region DataLength property
        private int _dataLength;
        public int DataLength
        {
            get => _dataLength;
            set => Set(ref _dataLength, value);
        }
        #endregion

        #region Offset property
        private int _offset;
        public int Offset
        {
            get => _offset;
            set => Set(ref _offset, value);
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
