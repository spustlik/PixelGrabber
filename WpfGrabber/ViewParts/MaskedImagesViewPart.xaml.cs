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
    public class MaskedImagesVM : SimpleDataObject
    {

    }

    public class MaskedImagesViewPartBase : ViewPartDataViewer<MaskedImagesVM>
    {
        public MaskedImagesViewPartBase() : base("Binary masked image") { }
    }

    public partial class MaskedImagesViewPart : MaskedImagesViewPartBase
    {
        public MaskedImagesViewPart()
        {
            InitializeComponent();
        }

        private void BorderSize_Changed(object sender, SizeChangedEventArgs e)
        {
            OnShowData();
        }

        protected override void OnShowData()
        {
            //TODO: use Maskreadr
        }
    }
}
