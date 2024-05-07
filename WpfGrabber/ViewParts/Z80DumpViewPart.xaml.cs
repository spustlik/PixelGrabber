﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
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
        public override void OnInitialize()
        {
            base.OnInitialize();
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
            //MessageBox.Show(addr.ToString("X4") + " " + ViewModel.GoToAddrText, "Go to");
            if (!_addressMap.TryGetValue(addr, out var line))
                return;
            //textBox.ScrollToLine(line);
            var vertOffset = (editor.TextArea.TextView.DefaultLineHeight) * line;
            editor.ScrollToVerticalOffset(vertOffset);
        }

        private Dictionary<int,int> _addressMap = new Dictionary<int, int>();
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
            while (z80.Addr < z80.Data.Length)
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
                        sb.Append(z80.Addr.ToString("X4")).Append(": ");
                    var b = z80.ReadByte();//skip byte
                    sb.Append(b.ToString("X2"));
                    sb.Append(" ");
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

    }
}
