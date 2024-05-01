using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

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

        #region Zoom100 property
        [XmlIgnore]
        public double Zoom100
        {
            get => Zoom * 100;
            set => Zoom = value / 100;
        }
        #endregion
        protected override void DoPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.DoPropertyChanged(propertyName);
            if (propertyName == nameof(Zoom))
                base.DoPropertyChanged(nameof(Zoom100));
        }

        #region AutoLoadLayout property
        private bool _autoLoadLayout;
        public bool AutoLoadLayout
        {
            get => _autoLoadLayout;
            set => Set(ref _autoLoadLayout, value);
        }
        #endregion

        [XmlIgnore]
        public byte[] Data { get; private set; }
        public void LoadData(string fileName)
        {
            if (!File.Exists(fileName))
            {
                RemoveRecentFile(fileName);
                Data = new byte[0];
            }
            else
            {
                FileName = fileName;
                AddRecentFile(fileName);
                Data = File.ReadAllBytes(fileName);
            }
            Offset = 0;
            DataLength = Data.Length;
            DoPropertyChanged(nameof(Data));
        }

        public void SetData(byte[] data)
        {
            Offset = 0;
            Data = data;
            DataLength = data.Length;
            DoPropertyChanged(nameof(Data));
        }

        private void AddRecentFile(string fileName)
        {
            var i = RecentFiles.FindIndex(x => String.Compare(x, fileName, StringComparison.OrdinalIgnoreCase) == 0);
            if (i >= 0)
                return;
            //while (RecentFiles.Count > 20)
            //    RecentFiles.RemoveAt(0);
            RecentFiles.Add(fileName);
        }
        private void RemoveRecentFile(string fileName)
        {
            var i = RecentFiles.FindIndex(x => String.Compare(x, fileName, StringComparison.OrdinalIgnoreCase) == 0);
            if (i < 0)
                return;
            RecentFiles.RemoveAt(i);
        }
        public ObservableCollection<string> RecentFiles { get; private set; } = new ObservableCollection<string>();
        [XmlIgnore]
        public ObservableCollection<string> Layouts { get; private set; } = new ObservableCollection<string>();
    }
}
