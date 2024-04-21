using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WpfGrabber
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static new App Current => Application.Current as App;

        public IServiceContainer ServiceContainer { get; private set; } = new ServiceContainer();
        public IServiceProvider ServiceProvider => ServiceContainer;

        public static T GetService<T>() where T:class
        {
            return Current?.ServiceProvider?.GetService<T>();
        }
    }
}
