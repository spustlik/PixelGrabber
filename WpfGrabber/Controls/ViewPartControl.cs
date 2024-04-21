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

namespace WpfGrabber.Controls
{
    public class ViewPartControl : ContentControl
    {
        static ViewPartControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ViewPartControl), new FrameworkPropertyMetadata(typeof(ViewPartControl)));
        }

        #region Title dp
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(ViewPartControl), new PropertyMetadata());
        #endregion

    }
}
