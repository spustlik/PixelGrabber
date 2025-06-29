using System;
using System.IO;
using System.Linq;
using System.Windows;
using WpfGrabber.Services;
using WpfGrabber.ViewParts;

namespace WpfGrabber.Shell
{
    /// <summary>
    /// Interaction logic for ShellWindow.xaml
    /// </summary>
    public partial class ShellWindow : Window, IShellWindow
    {
        public ShellVm ViewModel => DataContext as ShellVm;

        public ProjectManager ProjectManager { get; }

        public ShellWindow()
        {
            DataContext = new ShellVm();
            App.Current.ServiceContainer.AddService<IShellWindow>(this);
            App.Current.ServiceContainer.AddService(ViewModel);
            App.Current.ServiceContainer.AddService(new ViewParts.ViewPartFactory());
            App.Current.ServiceContainer.AddService(ProjectManager = new ProjectManager(App.Current.ServiceProvider));
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ExceptionWindow.StartHandlingExceptions();
            var cfg = AppConfig.Load(this.ViewModel);
            if (cfg != null && !string.IsNullOrEmpty(cfg.LastFile) && cfg.OpenLastFile)
            {
                ProjectManager.LoadFile(cfg.LastFile);
            }
            var vps = App.Current.ServiceProvider.GetService<IViewPartServiceEx>();
            if (vps.ViewParts.Count() == 0)
            {
                var vp = vps.AddNewPart(WhatsNewViewPart.Def);
                vps.SetOptions(vp, new ViewPartOptions() { Width = 300 });
            }
            _configSaver = Throthler.Create(TimeSpan.FromMilliseconds(50), () =>
            {
                AppConfig.Save(ViewModel);
            });
            if (ViewModel.WindowWidth > 100 && ViewModel.WindowHeight > 50)
            {
                this.Width = ViewModel.WindowWidth;
                this.Height = ViewModel.WindowHeight;
            }
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            this.SizeChanged += WindowSize_Changed;
        }

        private void WindowSize_Changed(object sender, SizeChangedEventArgs e)
        {
            ViewModel.WindowWidth = (int)e.NewSize.Width;
            ViewModel.WindowHeight = (int)e.NewSize.Height;
        }

        private Throthler _configSaver;
        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _configSaver.Start();
            if (!XmlHelper.IsPropertyIgnored(ViewModel, e.PropertyName))
                ProjectManager.SetDirty(true);
        }

        private void OnWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !CanCloseProjectPrompt();
        }

        bool IShellWindow.CanCloseProject()
        {
            return CanCloseProjectPrompt();
        }

        private bool CanCloseProjectPrompt()
        {
            if (!ViewModel.IsProjectDirty)
                return true;
            var r = MessageBox.Show(
                this,
                $"Do you want to save layout of {Path.GetFileName(ViewModel.FileName)}?",
                "Question",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);
            if (r == MessageBoxResult.Cancel)
            {
                return false;
            }
            if (r == MessageBoxResult.Yes)
            {
                ProjectManager.SaveLayout();
            }
            return true;
        }
    }
}
