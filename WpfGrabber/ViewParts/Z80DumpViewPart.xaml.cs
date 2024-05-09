using ICSharpCode.AvalonEdit.Rendering;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Xml.Serialization;
using WpfGrabber.Readers.Z80;
using WpfGrabber.Shell;
using WpfGrabber.ViewParts.Z80Syntax;

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
        [XmlIgnore]
        public string DumpText
        {
            get => _dumptext;
            set => Set(ref _dumptext, value);
        }
        #endregion

        [XmlIgnore]
        public ObservableCollection<string> DumpLines { get; } = new ObservableCollection<string>();

        #region MapText property
        private string _mapText;
        public string MapText
        {
            get => _mapText;
            set => Set(ref _mapText, value);
        }
        #endregion

        [XmlIgnore]
        public ObservableCollection<int> UndoLine { get; } = new ObservableCollection<int>();
        [XmlIgnore]
        public ObservableCollection<int> RedoLine { get; } = new ObservableCollection<int>();
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
        public override void OnInitialize()
        {
            base.OnInitialize();
            ViewModel.ShowAddr = true;
            Z80SyntaxHighlighter.Init();
            editor.SyntaxHighlighting = Z80SyntaxHighlighter.GetDefinition();
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var gen = CustomLinkElementGenerator.Install(editor);
            gen.CustomLinkClicked += Gen_CustomLinkClicked;
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
        protected override void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.ViewModel_PropertyChanged(sender, e);
            if (e.PropertyName == nameof(Z80DumpVM.DumpText))
            {
                editor.Text = ViewModel.DumpText;
            }
        }
        private void Gen_CustomLinkClicked(string s)
        {
            if (s.StartsWith("0x"))
                s = s.Substring(2);
            if (s.StartsWith("L"))
                s = s.Substring(1);
            var addr = int.Parse(s, System.Globalization.NumberStyles.HexNumber);
            GoToAddress(addr);
        }

        private void GoToAddress(int addr)
        {
            int firstLine = GetCurrentLine();
            if (ViewModel.UndoLine.LastOrDefault() != firstLine
                && ViewModel.RedoLine.LastOrDefault() != firstLine)
            {
                ViewModel.UndoLine.Push(firstLine);
            }
            if (!_addressMap.TryGetValue(addr, out var lineIndex))
                return;
            ScrollToLine(lineIndex);

        }

        private int GetCurrentLine()
        {
            var textView = editor.TextArea.TextView;
            int firstLine = textView.GetDocumentLineByVisualTop(textView.ScrollOffset.Y).LineNumber - 1;
            return firstLine;
        }

        private void ScrollToLine(int lineIndex)
        {
            //textBox.ScrollToLine(line);
            var vertOffset = editor.TextArea.TextView.DefaultLineHeight * lineIndex;
            editor.ScrollToVerticalOffset(vertOffset);
        }

        private Dictionary<int, int> _addressMap = new Dictionary<int, int>();
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
            _addressMap.Clear();
            while (z80.DataPosition < z80.Data.Length)
            {
                var sb = new StringBuilder();
                try
                {
                    var instr = z80.ReadInstruction();
                    var line = new StringBuilder();
                    if (ViewModel.ShowAddr)
                        sb.Append(instr.Start.ToString("X4")).Append(": ");
                    if (ViewModel.ShowOpcodes)
                    {
                        var code = z80.Data.GetRange(instr.Start, instr.Len);
                        var os = code.ToHex();
                        sb.Append(os)
                          .Append(new string(' ', 3 * (6 - code.Length)))
                          .Append(" ");
                    }
                    sb.Append(instr.ToString());
                    _addressMap[instr.Start] = dumpLines.Count;
                }
                catch (Exception e)
                {
                    if (ViewModel.ShowAddr)
                        sb.Append(z80.DataPosition.ToString("X4")).Append(": ");
                    if (z80.DataPosition < z80.Data.Length)
                    {
                        var b = z80.ReadByte();//skip byte
                        sb.Append(b.ToString("X2"));
                        sb.Append(" ");
                    }
                    sb.Append(">> ");
                    sb.Append(e.Message);
                }
                dumpLines.Add(sb.ToString());
                result.AppendLine(sb.ToString());
            }
            ViewModel.DumpText = result.ToString();
            ViewModel.DumpLines.AddRange(dumpLines, clear: true);
        }

        private void SaveText_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.FileName = Path.GetFileNameWithoutExtension(ShellVm.FileName) + ".dmp";
            if (dlg.ShowDialog() != true)
                return;
            File.WriteAllText(dlg.FileName, ViewModel.DumpText);
        }

        private void BtnUndoRedo_Click(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is ButtonBase btn))
                return;
            void doUndoRedo(ObservableCollection<int> src, ObservableCollection<int> dst)
            {
                if (src.Count == 0)
                    return;
                var last = src.Pop();
                //if (dst.LastOrDefault() != last)
                //    dst.Push(last);
                var current = GetCurrentLine();
                if (dst.LastOrDefault() != current)
                    dst.Push(current);
                ScrollToLine(last);
            }
            var dir = Convert.ToInt32(btn.CommandParameter);
            if (dir < 0)
            {
                doUndoRedo(ViewModel.UndoLine, ViewModel.RedoLine);
            }
            else if (dir > 0)
            {
                doUndoRedo(ViewModel.RedoLine, ViewModel.UndoLine);
            }
        }

        private void DumpSNA_Click(object sender, RoutedEventArgs e)
        {
            var rd = new DataReader(ShellVm.Data, 0);
            var sna = new SnaData(rd);
            var sb = new StringBuilder();
            foreach (var pi in sna.GetType().GetProperties())
            {
                var v = pi.GetValue(sna);
                sb.Append(pi.Name).Append("= ");
                if (v is int i)
                    sb.Append(i.ToString("X4"));
                else
                    sb.Append(v);
                sb.AppendLine();
            }
            editor.Text = sb.ToString();
            if (ShellVm.Offset == 0)
                ShellVm.Offset = rd.BytePosition;
        }
    }
}
