using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Xml.Serialization;
using WpfGrabber.Readers.Z80;
using WpfGrabber.Shell;

namespace WpfGrabber.ViewParts
{
    public class Z80DumpVM : SimpleDataObject
    {

        #region ShowAddr property
        private bool _showAddr;
        public bool ShowAddr
        {
            get => _showAddr;
            set => Set(ref _showAddr, value);
        }
        #endregion

        #region ShowOpcodes property
        private bool _showOpcodes;
        public bool ShowOpcodes
        {
            get => _showOpcodes;
            set => Set(ref _showOpcodes, value);
        }
        #endregion


        #region DumpText property
        private string _dumptext;
        public string DumpText
        {
            get => _dumptext;
            set => Set(ref _dumptext, value);
        }
        #endregion

        [XmlIgnore]
        public ObservableCollection<string> DumpLines { get; private set; } = new ObservableCollection<string>();
    }

    public class Z80DumpViewPartBase : ViewPartDataViewer<Z80DumpVM>
    {

    }

    public partial class Z80DumpViewPart : Z80DumpViewPartBase
    {

        public Z80DumpViewPart()
        {
            InitializeComponent();
        }

        protected override void ShellVm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Shell.ShellVm.Data)
                || e.PropertyName == nameof(Shell.ShellVm.Offset)
                )
            {
                base.ShellVm_PropertyChanged(sender, e);
            }
        }
        protected override void OnShowData()
        {
            if (ShellVm.DataLength <= 0)
            {
                ViewModel.DumpText = "";
                return;
            }
            var result = new StringBuilder();
            var dumpLines = new List<string>();
            var z80 = new Z80Reader(ShellVm.Data, ShellVm.Offset);
            while (z80.Addr < z80.Data.Length)
            {
                var sb = new StringBuilder();
                try
                {
                    var instr = z80.ReadInstruction();
                    var line = new StringBuilder();
                    if (ViewModel.ShowAddr)
                        sb.Append(instr.Start.ToString("X4")).Append(":\t");
                    if (ViewModel.ShowOpcodes)
                    {
                        var code = z80.Data.GetRange(instr.Start, instr.Len);
                        var os = code.ToHex();
                        sb.Append(os)
                          .Append(new string(' ', 3 * (6 - code.Length)))
                          .Append("\t");
                    }
                    sb.Append(instr.ToString());
                }
                catch (Exception e)
                {
                    if (ViewModel.ShowAddr)
                        sb.Append(z80.Addr.ToString("X4")).Append(":\t");
                    var b = z80.ReadByte();//skip byte
                    sb.Append(b.ToString("X2"));
                    sb.Append(" ");
                    sb.Append(">> ");
                    sb.Append(e.Message);
                }
                dumpLines.Add(sb.ToString());
                result.AppendLine(sb.ToString());
            }
            ViewModel.DumpText = result.ToString(); //to use from TextBox Text={Binding DumpText}
            ViewModel.DumpLines.Clear();
            ViewModel.DumpLines.AddRange(dumpLines);

            if (richTextBox.IsVisible)
            {
                FlowDocument doc = richTextBox.Document ?? new FlowDocument();
                var para = new Paragraph();
                foreach (var line in ViewModel.DumpLines)
                {
                    para.Inlines.AddRange(CreateLineInlines(line));
                    para.Inlines.Add(new LineBreak());
                }
                doc.Blocks.Clear();
                doc.Blocks.Add(para);
                richTextBox.Document = doc;

                //var position = doc.ContentStart;
                //position.getpo

            }
        }

        private static Regex _regex = new Regex(@"(0x[0-9a-f]{4})|(L[0-9a-f]{4})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private IEnumerable<Inline> CreateLineInlines(string line)
        {
            //TODO:find 0x1234 and L1234 patterns and make them clickable
            //para.Inlines.Add(new Run(line));
            yield return new Run(line);
        }

        private void RichTextBoxVisible_Changed(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (richTextBox.IsVisible)
                ShowData();
        }

        private void SaveText_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.FileName = Path.GetFileNameWithoutExtension(ShellVm.FileName) + ".dmp";
            if (dlg.ShowDialog() != true)
                return;
            File.WriteAllText(dlg.FileName, ViewModel.DumpText);
        }

    }
}
