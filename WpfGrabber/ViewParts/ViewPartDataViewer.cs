using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using WpfGrabber.Services;
using WpfGrabber.Shell;

namespace WpfGrabber.ViewParts
{
    public abstract class ViewPartDataViewer<TVM> : ViewPart where TVM:SimpleDataObject,new()
    {
        public ShellVm ShellVm { get; private set; }
        protected ViewPartDataViewer()
        {
            DataContext = new TVM();
        }
        public TVM ViewModel => DataContext as TVM;

        public override void OnInitialize()
        {
            base.OnInitialize();
            ShellVm = App.GetService<ShellVm>();
            ShellVm.PropertyChanged += ShellVm_PropertyChanged;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            ShowData();
        }

        protected virtual void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ShowData();
        }

        protected virtual void ShellVm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ShowData();
        }

        private bool _showingData = false;
        protected void ShowData()
        {
            if (_showingData)
                return;
            _showingData = true;
            Dispatcher.BeginInvoke((Action)InvokeShowData);
        }

        private void InvokeShowData()
        {
            this.OnShowData();
            _showingData = false;
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
