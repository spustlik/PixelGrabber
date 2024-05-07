using System;
using System.Collections.Generic;
using System.Globalization;
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
using WpfGrabber.Shell;

namespace WpfGrabber.ViewParts
{
    public class TestViewPartVM : SimpleDataObject
    {

        #region TestEnum property
        private TestViewPartEnum _testEnum;
        public TestViewPartEnum TestEnum
        {
            get => _testEnum;
            set => Set(ref _testEnum, value);
        }
        #endregion

        #region TestBool property
        private bool _testBool;
        public bool TestBool
        {
            get => _testBool;
            set => Set(ref _testBool, value);
        }
        #endregion

    }

    public enum TestViewPartEnum
    {
        None,
        Done,
        Waiting
    }

    public class TestViewPartBase : ViewPartDataViewer<TestViewPartVM>
    {

    }
    public partial class TestViewPart : TestViewPartBase
    {
        public TestViewPart():base()
        {
            InitializeComponent();
        }

        private void TestMenu_Click(object sender, RoutedEventArgs e)
        {

        }

        protected override void OnShowData()
        {
            var (max_w, max_h) = GetDataImageSize(imageBorder);
            var rgba = new ByteBitmapRgba(max_w, max_h);
            var font = AppData.GetFont();

            font.DrawString(rgba, 0, 0, "TEST TEXT");
            var bmp = rgba.ToBitmapSource();
            image.Source = bmp;
            image.RenderTransform = new ScaleTransform(ShellVm.Zoom, ShellVm.Zoom);
        }

        private void MergeLayouts_Click(object sender, RoutedEventArgs e)
        {
            var list= ShellVm.Layouts.OrderBy(x => x).ToList();
            int i = 0; 
            while(i < list.Count)
            {
                var layout = list[i];
                if (layout.Length != 4 || 
                    !int.TryParse(layout, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var res))
                {
                    list.RemoveAt(i);
                    continue;
                }
                i++;
            }

            Clipboard.SetText(string.Join("\n", list));
        }

    }


}
