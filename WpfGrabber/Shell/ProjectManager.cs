using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WpfGrabber.ViewParts;

namespace WpfGrabber.Shell
{

    public class ProjectManager
    {
        private IServiceProvider serviceProvider;

        public ProjectManager(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public void LoadFile(string fileName)
        {
            var shellVm = serviceProvider.GetService<ShellVm>();
            if (shellVm.AutoLoadLayout && !string.IsNullOrEmpty(shellVm.FileName))
            {
                if (!serviceProvider.GetService<IShellWindow>().CanCloseProject())
                    return;
            }
            shellVm.LoadData(fileName);
            if (shellVm.AutoLoadLayout)
            {
                if (!LoadLayout())
                {
                    ///auto layout ???? 
                }
            }
            SetDirty(false);
        }

        public void SetDirty(bool dirty)
        {
            var shellVm = serviceProvider.GetService<ShellVm>();
            shellVm.IsProjectDirty = dirty;
        }

        public bool LoadLayout()
        {
            var layoutSvc = serviceProvider.GetService<LayoutManagerService>();
            var r = layoutSvc.LoadLayoutFile(null);
            SetDirty(false);
            return r;
        }
        public void SaveLayout()
        {
            var layoutSvc = serviceProvider.GetService<LayoutManagerService>();
            layoutSvc.SaveLayoutFile(null);
            SetDirty(false);
        }

        public void SaveNamedLayout(string name)
        {
            var layoutSvc = serviceProvider.GetService<LayoutManagerService>();
            layoutSvc.SaveLayoutFile(name);
        }

        public void LoadNamedLayout(string name)
        {
            var layoutSvc = serviceProvider.GetService<LayoutManagerService>();
            layoutSvc.LoadLayoutFile(name);
        }

        public void RemoveNamedLayout(string name)
        {
            var layoutSvc = serviceProvider.GetService<LayoutManagerService>();
            layoutSvc.RemoveLayoutFile(name);
        }
    }
}
