using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WpfGrabber.Shell
{
    
    public class ProjectManager
    {
        private IServiceProvider serviceProvider;

        public ProjectManager(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        internal void LoadFile(string fileName)
        {
            var shellVm = serviceProvider.GetService<ShellVm>();
            shellVm.LoadData(fileName);
            if (shellVm.AutoLoadLayout)
            {
                LoadLayout();
            }
        }

        public void LoadLayout()
        {
            var layoutSvc = serviceProvider.GetService<LayoutManagerService>();
            layoutSvc.LoadLayoutFile(null);
        }
        public void SaveLayout()
        {
            var layoutSvc = serviceProvider.GetService<LayoutManagerService>();
            layoutSvc.SaveLayoutFile(null);
        }

        public void SaveOffsetLayout()
        {
            var layoutSvc = serviceProvider.GetService<LayoutManagerService>();
            var shellVm = serviceProvider.GetService<ShellVm>();
            layoutSvc.SaveLayoutFile(shellVm.Offset.ToString("X4"));
        }

        public void LoadOffsetLayout(string name)
        {
            var layoutSvc = serviceProvider.GetService<LayoutManagerService>();
            layoutSvc.LoadLayoutFile(name);
        }

        internal void RemoveOffsetLayout()
        {
            var layoutSvc = serviceProvider.GetService<LayoutManagerService>();
            var shellVm = serviceProvider.GetService<ShellVm>();
            layoutSvc.RemoveLayoutFile(shellVm.Offset.ToString("X4"));
        }
    }
}
