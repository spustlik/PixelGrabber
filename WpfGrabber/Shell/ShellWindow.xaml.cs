using System;
using System.IO;
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
            var def = new ViewPartDef<WhatsNewViewPart>() { Title = "What is new"};
            var vp = vps.AddNewPart(def);
            vps.SetOptions(vp, new ViewPartOptions() { Width = 300 });

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
                $"Do you want to save layout of {Path.GetFileName( ViewModel.FileName)}?", 
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
