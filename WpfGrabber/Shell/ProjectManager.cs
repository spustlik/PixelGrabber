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
            var layoutSvc = serviceProvider.GetService<ILayoutManagerService>();
            layoutSvc.LoadLayoutFile();
        }
        public void SaveLayout()
        {
            var layoutSvc = serviceProvider.GetService<ILayoutManagerService>();
            layoutSvc.SaveLayoutFile();
        }
    }
}
