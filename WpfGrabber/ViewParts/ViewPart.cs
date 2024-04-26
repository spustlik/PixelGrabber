using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using WpfGrabber.Controls;

namespace WpfGrabber.ViewParts
{
    public abstract class ViewPart : UserControl
    {
        static ViewPart()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(ViewPartControl), new FrameworkPropertyMetadata(typeof(ViewPartControl)));
        }
        public ViewPart()
        {
        }
        
        public virtual void OnInitialize()
        {

        }
        public virtual void OnClose()
        {

        }

        public virtual void OnSaveLayout(XElement ele)
        {
        }

        public virtual void OnLoadLayout(XElement ele)
        {
        }
    }

}
