using System;
using System.Collections.Generic;
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

namespace WpfGrabber.ViewParts
{
    /// <summary>
    /// Interaction logic for TestViewPart.xaml
    /// </summary>
    public partial class TestViewPart : ViewPartDataViewer<TestViewPartVM>
    {
        public TestViewPart():base()
        {
            InitializeComponent();
        }

        private void TestMenu_Click(object sender, RoutedEventArgs e)
        {

        }
    }

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
}
