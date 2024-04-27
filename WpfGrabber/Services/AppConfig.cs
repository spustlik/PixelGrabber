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

        public string[] RecentFiles { get; set; }
        public string LastFile { get; set; }
        public bool OpenLastFile { get; set; } = true;
        public double Zoom { get; set; }
        public bool AutoLoadLayout { get; set; } = true;

        public static void Save(ShellVm vm)
        {
            var data = new AppConfig();
            data.RecentFiles = vm.RecentFiles.ToArray();
            data.Zoom = vm.Zoom;
            data.LastFile = vm.FileName;
            data.AutoLoadLayout = vm.AutoLoadLayout;
            XmlHelper.SerializeToFile(AppConfigFileName, data);
            //string[] is not supported
            //XmlHelper.SaveToFile(Path.ChangeExtension(AppConfigFileName,".config"), data);
        }

        public static void Load(ShellVm vm)
        {
            var data = XmlHelper.SerializeFromFile<AppConfig>(AppConfigFileName);
            if (data == null)
                return;
            vm.Zoom = data.Zoom;
            vm.AutoLoadLayout = data.AutoLoadLayout;
            vm.RecentFiles.AddRange(data.RecentFiles, clear: true);
            if (!string.IsNullOrEmpty(data.LastFile) && data.OpenLastFile)
                vm.LoadData(data.LastFile);
            //var data2 = XmlHelper.LoadFromFile<AppConfig>(Path.ChangeExtension(AppConfigFileName, ".config"));
        }
    }
}
