using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfGrabber
{
    public class MainWindowViewModel : SimpleDataObject
    {
        public MainWindowViewModel()
        {
            Zoom = 1;
            Offset = 0;
            Width = 64;
            DataLength = 10000;
            RecentFileNames.Add(@"E:\GameWork\FEUD\FEUD1.COM");
            RecentFileNames.Add(@"E:\GameWork\FEUD\FEUD.RAM");
            RecentFileNames.Add(@"E:\GameWork\sord\STEPUP.rom");
            RecentFileNames.Add(@"E:\GameWork\8bitgames\SOROBO.COM");
            RecentFileNames.Add(@"E:\GameWork\8bitgames\KNIGHT.COM");
            RecentFileNames.Add(@"E:\GameWork\8bitgames\alien8.rom");
            RecentFileNames.Add(@"E:\GameWork\8bitgames\Bat-obr.com");
            RecentFileNames.Add(@"E:\GameWork\8bitgames\Batman.com");
            RecentFileNames.Add(@"E:\GameWork\8bitgames\Batman (1986)(Ocean).cas");
            RecentFileNames.Add(@"E:\GameWork\8bitgames\pentagram.dsk");
            RecentFileNames.Add(@"E:\GameWork\8bitgames\_test.bin");

        }

        #region Width property
        private int _width;
        public int Width
        {
            get => _width;
            set => Set(ref _width, value);
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

        #region DataLength property
        private int _dataLength;
        public int DataLength
        {
            get => _dataLength;
            set => Set(ref _dataLength, value);
        }
        #endregion

        #region Zoom property
        private double __zoom;
        public double Zoom
        {
            get => __zoom;
            set => Set(ref __zoom, value);
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


        public ObservableCollection<string> RecentFileNames { get; private set; }
            = new ObservableCollection<string>();

        public ObservableCollection<string> HexLines { get; private set; }
            = new ObservableCollection<string>();

        public void AddRecent(string fileName)
        {
            if (RecentFileNames.Any(x => String.Compare(x, fileName, StringComparison.CurrentCultureIgnoreCase) == 0))
                return;
            RecentFileNames.Add(fileName);
        }

        public void RemoveRecent(string fileName)
        {
            var f = RecentFileNames.FirstOrDefault(x => String.Compare(x, fileName, StringComparison.CurrentCultureIgnoreCase) == 0);
            if (!string.IsNullOrEmpty(f))
                RecentFileNames.Remove(f);
        }
    }
}
