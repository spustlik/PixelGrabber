using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using WpfGrabber.Controls;
using WpfGrabber.Data;
using WpfGrabber.ViewParts;

namespace WpfGrabber.Shell
{
    /// <summary>
    /// Interaction logic for ShellWindowMenu.xaml
    /// </summary>
    public partial class ShellWindowMenu : UserControl
    {
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

            #region ShellVm property
            private ShellVm _shellVm;
            public ShellVm ShellVm
            {
                get => _shellVm;
                set => Set(ref _shellVm, value);
            }
            #endregion

        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ViewModel.ViewParts.AddRange(App.Current?.ServiceProvider?.GetService<ViewPartFactory>().Definitions);
            ViewModel.ShellVm = App.Current?.ServiceProvider?.GetService<ShellVm>();

            CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, OnOpenFileExecuted));
            CommandBindings.Add(new CommandBinding(OpenRecentCommand, OnOpenRecentFileExecuted));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, OnCloseExecuted));
            CommandBindings.Add(new CommandBinding(ShowViewPartCommand, OnShowViewPartExecuted));
        }

        private void OnShowViewPartExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var def = (ViewPartDef)e.Parameter;
            var vps = App.GetService<IViewPartService>();
            vps.AddNewPart(def);
            e.Handled = true;
        }

        private void OnCloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void LoadFile(string fn)
        {
            App.GetService<ProjectManager>().LoadFile(fn);
        }

        private void OnOpenRecentFileExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            LoadFile((string)e.Parameter);
        }

        private void OnOpenFileExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() != true)
                return;
            LoadFile(dlg.FileName);
        }

        private void OnTestData_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ShellVm.FileName = "...test data...";
            ViewModel.ShellVm.Offset = 0;
            var data = new List<byte>();
            data.AddRange(new byte[] { 1, 2, 4, 8, 16, 32, 64, 128 });
            data.AddRange(new byte[] { 0x0a, 0x05, 0xa0, 0x50 });
            data.AddRange(Enumerable.Range(0, 8).Select((i, pos) => pos % 2 == 0 ? (byte)0xaa : (byte)0x55));
            data.AddRange(Enumerable.Range(0, 255).Select(i => (byte)i));
            ViewModel.ShellVm.SetData(data.ToArray());
        }

        private void LoadLayout_Click(object sender, RoutedEventArgs e)
        {
            App.GetService<ProjectManager>().LoadLayout();
        }

        private void SaveLayout_Click(object sender, RoutedEventArgs e)
        {
            App.GetService<ProjectManager>().SaveLayout();
        }


        private void LoadNamedLayout_Click(object sender, RoutedEventArgs e)
        {
            var name = (string)((MenuItem)e.OriginalSource).CommandParameter;
            App.GetService<ProjectManager>().LoadNamedLayout(name);
        }

        private void RemoveNamedLayout_Click(object sender, RoutedEventArgs e)
        {
            var name = (string)((MenuItem)e.OriginalSource).CommandParameter;
            App.GetService<ProjectManager>().RemoveNamedLayout(name);

        }

        private void SaveNamedLayout_Click(object sender, RoutedEventArgs e)
        {
            if (InputQueryWindow.ShowModal("Enter layout name", out var name, ViewModel.ShellVm.Offset.ToString("X4")) != true)
                return;
            App.GetService<ProjectManager>().SaveNamedLayout(name);
        }

        private void OnFolderRGBA_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                DefaultExt = ".png",
                Title = "Select images to convert",
                CheckFileExists = false,
                Multiselect = true
            };
            if (dlg.ShowDialog() != true)
                return;
            if (dlg.FileNames.Length == 0)
                return;
            var files = dlg.FileNames;
            files = files.OrderBy(x => x).ToArray();
            var log = new List<string>();
            foreach (var _filePath in files)
            {
                var file = new FilePath(_filePath);
                var name = Path.GetFileNameWithoutExtension(file.Name);
                if (name.EndsWith("_a", StringComparison.OrdinalIgnoreCase))
                    continue;
                var maskFile = FilePath.FromPath(file.Directory, name + "_a" + file.Extension);
                var newFile = file.ChangeExtension(".png");
                if (!File.Exists(maskFile.FullPath))
                {
                    if (file.Extension.Equals(".png", StringComparison.OrdinalIgnoreCase))
                    {
                        log.Add($"SKIPPED:{file.Name}");
                        continue;
                    }
                    log.Add($"CONVERTED:{file.Name} => {newFile.Name}");
                    var bmp = new BitmapImage();
                    bmp.LoadFromFile(file.FullPath);
                    bmp.SaveToPngFile(newFile.FullPath);
                    continue;
                }
                var c_img = new BitmapImage();
                c_img.LoadFromFile(file.FullPath);
                var colors = c_img.ToRgba();

                var a_img = new BitmapImage();
                a_img.LoadFromFile(maskFile.FullPath);
                var alpha = a_img.ToRgba();
                //alpha.ToBitmapSource().SaveToPngFile(Path.ChangeExtension(newName, "mask.png"));

                if (!ApplyAlpha(colors, alpha))
                {
                    log.Add($"!Error applying mask: {file.Name}+{maskFile.Name} to {newFile.Name}");
                    alpha.ToBitmapSource().SaveToPngFile(newFile.ChangeExtension("mask-error.png").FullPath);
                    continue;
                }
                var r_img = colors.ToBitmapSource();
                r_img.SaveToPngFile(newFile.FullPath);
                log.Add($"MASKED: {file.Name}+{maskFile.Name} to {newFile.Name}");
            }

            TextViewPart.ShowPart("Convert BMP+A images", log.ToArray());

        }

        private static bool ApplyAlpha(ByteBitmapRgba colors, ByteBitmapRgba alpha)
        {
            for (int y = 0; y < colors.Height; y++)
            {
                for (int x = 0; x < colors.Width; x++)
                {
                    ByteColor m = alpha.GetPixel(x, y);
                    if (m.R != m.G && m.R != m.B && m.A != 0xFF)
                        return false;
                    ByteColor c = colors.GetPixel(x, y);
                    var r = ByteColor.FromRgba(c.R, c.G, c.B, m.R);
                    colors.SetPixel(x, y, r);
                }
            }
            return true;
        }

        private void OnExtractTexts_Click(object sender, RoutedEventArgs e)
        {
            var result = new List<string>();
            var i = 0;
            var last = "";
            var s = Encoding.GetEncoding("iso-8859-1").GetString(ViewModel.ShellVm.Data);
            // pascal strings - [len]...bytes
            // c strings - ...bytes\0
            // spec. strings - ends with high bit set to 1
            //max length
            //allowed chars (a-z,A-Z,space,apostrophe) ? digits, brackets? chars 32-127?
            //distinct
            var chars = new Regex(@"[A-Za-z _']");
            while (i < s.Length)
            {
                char c = s[i++];
                if (chars.IsMatch(c + ""))
                {
                    last += c;
                    continue;
                }
                //if(c=='\0')//std c end
                if (c > 'z')
                {

                }
                if (c > 127 && last.Length > 2)
                {
                    c = (char)(c - 128); //(char)(b & 0x7F); // remove high bit
                    if (chars.IsMatch(c + ""))
                    {
                        last += c; //end of string
                    }
                }
                if (!String.IsNullOrEmpty(last))
                    result.Add(last);
                last = "";
            }
            if (!String.IsNullOrEmpty(last))
                result.Add(last);
            result = result.Where(x => x.Length > 2).Distinct().ToList();
            TextViewPart.ShowPart("Extracted texts", result.ToArray());
        }
    }
}
