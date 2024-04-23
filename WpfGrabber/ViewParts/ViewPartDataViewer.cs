using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            OnShowData();
        }

        protected virtual void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnShowData();
        }

        protected virtual void ShellVm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnShowData();
        }

        protected virtual void OnShowData()
        {

        }
    }
}
