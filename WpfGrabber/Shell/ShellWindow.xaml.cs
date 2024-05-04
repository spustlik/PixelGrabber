using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfGrabber.Services;
using static WpfGrabber.Shell.ShellWindowMenu;

namespace WpfGrabber.Shell
{
    /// <summary>
    /// Interaction logic for ShellWindow.xaml
    /// </summary>
    public partial class ShellWindow : Window
    {
        public ShellVm ViewModel => DataContext as ShellVm;

        public ProjectManager ProjectManager { get; }

        public ShellWindow()
        {
            DataContext = new ShellVm();
            App.Current.ServiceContainer.AddService(ViewModel);
            App.Current.ServiceContainer.AddService(new ViewParts.ViewPartFactory());
            App.Current.ServiceContainer.AddService(ProjectManager = new ProjectManager(App.Current.ServiceProvider));
            InitializeComponent();
            ExceptionWindow.StartHandlingExceptions();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            var cfg = AppConfig.Load(this.ViewModel);
            if (cfg != null && !string.IsNullOrEmpty(cfg.LastFile) && cfg.OpenLastFile)
            {
                ProjectManager.LoadFile(cfg.LastFile);
            }

            _configSaver = Throthler.Create(TimeSpan.FromMilliseconds(50), () =>
            {
                AppConfig.Save(ViewModel);
            });
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private Throthler _configSaver;
        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _configSaver.Start();
        }

        private void OnWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!ViewModel.IsChanged)
                return;
            var r = MessageBox.Show(this, "Do you want to save layout?", "Close application", MessageBoxButton.YesNoCancel);
            if (r == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            if (r == MessageBoxResult.Yes)
            {
                ProjectManager.SaveLayout();
            }
        }
    }
}
