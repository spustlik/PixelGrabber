using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfGrabber._Legacy;
using WpfGrabber.ViewParts;

namespace WpfGrabber.Shell
{
    /// <summary>
    /// Interaction logic for ShellWindowMenu.xaml
    /// </summary>
    public partial class ShellWindowMenu : UserControl
    {
        public ShellVm ShellVm { get; private set; }
        public static RoutedUICommand OpenRecentCommand = new RoutedUICommand();
        public static RoutedUICommand ShowViewPartCommand = new RoutedUICommand();
        public ShellWindowMenu()
        {
            DataContext = new VM();
            InitializeComponent();
        }

        public VM ViewModel => DataContext as VM;
        public class VM : SimpleDataObject
        {
            public ObservableCollection<ViewPartDef> ViewParts { get; } = new ObservableCollection<ViewPartDef>();
        }
        public abstract class ViewPartDef
        {
            public string Title { get; set; }
            public virtual Type ViewPartType { get; }
            public abstract ViewPart Create();
        }
        public class ViewPartDef<T>: ViewPartDef where T:ViewPart,new ()
        {
            public override Type ViewPartType => typeof(T);
            public override ViewPart Create() => new T();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ViewModel.ViewParts.AddRange(new ViewPartDef[]
            {
                new ViewPartDef<Binary8BitViewPart>(){Title = "Binary 8bit" },
                //new ViewPartDef(){Title = "Binary 8bit mask" },
                new ViewPartDef<HexDumpViewPart>(){Title = "Hex dump" },
                new ViewPartDef<DealienViewPart>(){Title = "Dealien"},
                new ViewPartDef<TestViewPart>(){Title = "Test"}
            });
            ShellVm = App.Current?.ServiceProvider?.GetService<ShellVm>();
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, OnOpenFileExecuted));
            CommandBindings.Add(new CommandBinding(OpenRecentCommand, OnOpenRecentFileExecuted));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, OnCloseExecuted));
            CommandBindings.Add(new CommandBinding(ShowViewPartCommand, OnShowViewPartExecuted));
        }

        private void OnShowViewPartExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var vp = e.Parameter as ViewPartDef;
            var part = vp.Create();
            var vps = App.GetService<IViewPartService>();
            vps.Add(part);
        }

        private void OnCloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OnOpenRecentFileExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var fn = (string)e.Parameter;
            ShellVm.LoadData(fn);
        }

        private void OnOpenFileExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() != true)
                return;
            ShellVm.LoadData(dlg.FileName);
        }

        private void MainWindow_Click(object sender, RoutedEventArgs e)
        {
            var w = new MainWindow();
            w.Show();
        }

        private void OnTestData_Click(object sender, RoutedEventArgs e)
        {
            ShellVm.FileName = "...test data...";
            ShellVm.Offset = 0;
            var data = new List<byte>();
            data.AddRange(new byte[] { 1, 2, 4, 8, 16, 32, 64, 128 });
            data.AddRange(new byte[] { 0x0a, 0x05, 0xa0, 0x50 });
            data.AddRange(Enumerable.Range(0, 8).Select((i, pos) => pos % 2 == 0 ? (byte)0xaa : (byte)0x55));
            data.AddRange(Enumerable.Range(0, 255).Select(i => (byte)i));
            ShellVm.SetData(data.ToArray());
        }

        private void OnTestDropdown_Click(object sender, RoutedEventArgs e)
        {
            var wnd = new DropDownTestWindow();
            wnd.Show();
        }
    }
}
