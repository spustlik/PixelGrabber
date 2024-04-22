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
        public TestViewPart():base("Test view")
        {
            InitializeComponent();
        }
    }

    public class TestViewPartVM : SimpleDataObject
    {

    }
}
