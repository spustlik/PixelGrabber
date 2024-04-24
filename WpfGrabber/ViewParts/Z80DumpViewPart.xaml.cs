using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
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
        #region GoToAddrText property
        private string _gotoAddrText;
        public string GoToAddrText
        {
            get => _gotoAddrText;
            set => Set(ref _gotoAddrText, value);
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
            ViewModel.DumpText = result.ToString(); //to use from TextBox Text={Binding DumpText}
            ViewModel.DumpLines.AddRange(dumpLines, clear: true);
        }

        private static string HEXNUM = @"[0-9A-F]{4}";
        private static Regex _regex = new Regex($"(0x{HEXNUM})|(L{HEXNUM})", RegexOptions.Compiled | RegexOptions.IgnoreCase);


        private void SaveText_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.FileName = Path.GetFileNameWithoutExtension(ShellVm.FileName) + ".dmp";
            if (dlg.ShowDialog() != true)
                return;
            File.WriteAllText(dlg.FileName, ViewModel.DumpText);
        }

        private void TextBoxSelection_Changed(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(textBox.SelectedText))
                return;
            var match = _regex.Match(textBox.SelectedText);
            if (!match.Success)
            {
                ViewModel.GoToAddrText = null;
                return;
            }
            //+ matches[0].Index .. +Length
            ViewModel.GoToAddrText = match.Value;
            ShowHyperLink(textBox.SelectionStart + match.Index, match);
        }
        private void TextBox_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (textBox.SelectedText.Length > 0)
                return;
            //TODO: wait for moment ?
            var pos = e.GetPosition(textBox);
            var textpos = textBox.GetCharacterIndexFromPoint(pos, snapToText: false);
            if (textpos < 0)
                return;
            var startLineTextPos = textBox.GetCharacterIndexFromPoint(new Point(0, pos.Y), snapToText: true);
            if (startLineTextPos < 0)
                return;
            var lineIndex = textBox.GetLineIndexFromCharacterIndex(textpos);
            var line = textBox.GetLineText(lineIndex);
            var mousePos = textpos - startLineTextPos;

            var matches = _regex.Match(line, mousePos);
            while (!matches.Success)
            {
                mousePos--;
                if (mousePos < 0)
                    break;
                matches = _regex.Match(line, mousePos);
            }
            if (!matches.Success)
                return;

            ViewModel.GoToAddrText = matches.Captures[0].Value;
            ShowHyperLink(startLineTextPos, matches);
        }

        private void ShowHyperLink(int startLineTextPos, Match matches)
        {
            var b1 = textBox.GetRectFromCharacterIndex(startLineTextPos + matches.Captures[0].Index);
            var b2 = textBox.GetRectFromCharacterIndex(startLineTextPos + matches.Captures[0].Index + matches.Captures[0].Length);
            var p1 = textBox.TranslatePoint(b1.TopLeft, hyperLinkCanvas);
            var p2 = textBox.TranslatePoint(b2.BottomRight, hyperLinkCanvas);
            Canvas.SetLeft(hyperLinkSimulation, p1.X);
            Canvas.SetTop(hyperLinkSimulation, p1.Y);
            hyperLinkSimulation.Width = p2.X - p1.X;
            hyperLinkSimulation.Height = p2.Y - p1.Y;
            hyperLinkCanvas.Visibility = Visibility.Visible;
        }

        private void GoToAddress_Click(object sender, RoutedEventArgs e)
        {
            var s = ViewModel.GoToAddrText;
            if (s.StartsWith("0x"))
                s = s.Substring(2);
            if (s.StartsWith("L"))
                s = s.Substring(1);
            var addr = int.Parse(s, System.Globalization.NumberStyles.HexNumber);
            //MessageBox.Show(addr.ToString("X4") + " " + ViewModel.GoToAddrText, "Go to");
        }


        private void textBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            hyperLinkSimulation.Height = 0;
        }
    }
}
