﻿using System;
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

        public double Zoom { get; set; }

        public static void Save(ShellVm vm)
        {
            var data = new AppConfig();
            data.RecentFiles=vm.RecentFiles.ToArray();
            data.Zoom = vm.Zoom;
            XmlHelper.SaveToConfig(AppConfigFileName, data);
        }

        public static void Load(ShellVm vm)
        {
            var data = XmlHelper.LoadFromConfig<AppConfig>(AppConfigFileName);
            if (data == null)
                return;
            vm.Zoom = data.Zoom;
            vm.RecentFiles.AddRange(data.RecentFiles, clear: true);
        }
    }
}
