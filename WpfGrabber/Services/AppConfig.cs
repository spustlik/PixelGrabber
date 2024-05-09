using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Xml;
using System.Xml.Serialization;
using WpfGrabber.Shell;

namespace WpfGrabber.Services
{
    public class AppConfig
    {
        public static string AppConfigFileName => Path.ChangeExtension(Assembly.GetExecutingAssembly().Location, ".cfg");

        public string[] RecentFiles { get; private set; }
        public string LastFile { get; private set; }
        public bool OpenLastFile { get; private set; } = true;
        public double Zoom { get; private set; } = 1;
        public double UiZoom { get; private set; } = 1.0;
        public bool AutoLoadLayout { get; private set; } = true;

        public static void Save(ShellVm vm)
        {
            var data = new AppConfig();
            data.RecentFiles = vm.RecentFiles.ToArray();
            data.Zoom = vm.Zoom;
            data.UiZoom = vm.UiZoom;
            data.LastFile = vm.FileName;
            data.AutoLoadLayout = vm.AutoLoadLayout;
            XmlHelper.SerializeToFile(AppConfigFileName, data);
            //string[] is not supported
            //XmlHelper.SaveToFile(Path.ChangeExtension(AppConfigFileName,".config"), data);
        }

        public static AppConfig Load(ShellVm vm)
        {
            var data = XmlHelper.SerializeFromFile<AppConfig>(AppConfigFileName);
            if (data == null)
                return null;
            vm.Zoom = data.Zoom;
            vm.UiZoom = data.UiZoom;
            vm.AutoLoadLayout = data.AutoLoadLayout;
            vm.RecentFiles.AddRange(data.RecentFiles, clear: true);
            return data;
        }
    }
}
