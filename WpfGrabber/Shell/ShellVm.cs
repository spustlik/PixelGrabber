﻿using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

namespace WpfGrabber.Shell
{
    public class ShellVm : SimpleDataObject
    {
        public ShellVm()
        {
            Zoom = 1;
            UiZoom = 1;
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

        #region IsProjectDirty property
        private bool _isProjectDirty;
        [XmlIgnore]
        public bool IsProjectDirty
        {
            get => _isProjectDirty;
            set => Set(ref _isProjectDirty, value);
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

        #region UiZoom property
        private double _uiZoom;
        [XmlIgnore]
        public double UiZoom
        {
            get => _uiZoom;
            set => Set(ref _uiZoom, value);
        }
        #endregion

        #region WindowWidth property
        private int _windowWidth;
        public int WindowWidth
        {
            get => _windowWidth;
            set => Set(ref _windowWidth, value);
        }
        #endregion

        #region WindowHeight property
        private int _windowHeight;
        public int WindowHeight
        {
            get => _windowHeight;
            set => Set(ref _windowHeight, value);
        }
        #endregion

        #region AutoLoadLayout property
        private bool _autoLoadLayout;
        public bool AutoLoadLayout
        {
            get => _autoLoadLayout;
            set => Set(ref _autoLoadLayout, value);
        }
        #endregion

        #region StatusBarMessage property
        private string _statusBarMessage;
        public string StatusBarMessage
        {
            get => _statusBarMessage;
            set => Set(ref _statusBarMessage, value);
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
            {
                RecentFiles.RemoveAt(i); // move to end
            }
            while (RecentFiles.Count > 30)
                RecentFiles.RemoveAt(0);
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
