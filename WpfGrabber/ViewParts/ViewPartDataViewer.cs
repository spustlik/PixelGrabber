using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using System.Xml.Serialization;
using WpfGrabber.Services;
using WpfGrabber.Shell;

namespace WpfGrabber.ViewParts
{
    public abstract class ViewPartDataViewer<TVM> : ViewPart where TVM : SimpleDataObject, new()
    {
        private Throthler _viewer;

        public ShellVm ShellVm { get; private set; }
        protected ViewPartDataViewer()
        {
            DataContext = new TVM();
        }
        public TVM ViewModel => DataContext as TVM;

        public override void OnInitialize()
        {
            ShellVm = App.GetService<ShellVm>();
            base.OnInitialize();
            ShellVm.PropertyChanged += ShellVm_PropertyChanged;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            _viewer = Throthler.Create(TimeSpan.FromMilliseconds(100), OnShowData);
            ShowData();
        }


        protected virtual void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var xmli = ViewModel.GetType().GetProperty(e.PropertyName).GetCustomAttribute<XmlIgnoreAttribute>();
            if (xmli != null)
                return;
            App.GetService<ProjectManager>().SetDirty(true);
            ShowData();
        }

        protected virtual void ShellVm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ShowData();
        }

        protected void ShowData()
        {
            _viewer.Start();
        }

        protected virtual void OnShowData()
        {

        }

        protected (int width, int height) GetDataImageSize(FrameworkElement imageBorder)
        {
            var h = this.GetFirstValid(imageBorder.ActualHeight, imageBorder.Height, base.Height, 300);
            h = (int)(h / ShellVm.Zoom);
            var w = this.GetFirstValid(imageBorder.ActualWidth, imageBorder.Width, base.Width, 500);
            w = (int)(w / ShellVm.Zoom);
            return (width: w, height: h);
        }

        public override void OnSaveLayout(XElement ele)
        {
            ele.SaveProperties(ViewModel);
        }

        public override void OnLoadLayout(XElement ele)
        {
            ele.LoadProperties(ViewModel);
        }
    }
}
