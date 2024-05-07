using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System.Text.RegularExpressions;

namespace WpfGrabber.ViewParts.Z80Syntax
{
    //http://danielgrunwald.de/coding/AvalonEdit/rendering.php
    public class CustomLinkElementGenerator : VisualLineElementGenerator
    {
        private static string HEXNUM = @"[0-9A-F]{4}";
        private static Regex _regex = new Regex($"(0x{HEXNUM})|(L{HEXNUM})", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public event CustomLinkVisualLineText.CustomLinkClickHandler CustomLinkClicked;

        public static CustomLinkElementGenerator Install(TextEditor editor)
        {
            var g = new CustomLinkElementGenerator();
            editor.TextArea.TextView.ElementGenerators.Add(g);
            return g;
        }
        private Match FindMatch(int startOffset)
        {
            // fetch the end offset of the VisualLine being generated
            int endOffset = CurrentContext.VisualLine.LastDocumentLine.EndOffset;
            TextDocument document = CurrentContext.Document;
            string relevantText = document.GetText(startOffset, endOffset - startOffset);
            return _regex.Match(relevantText);
        }

        /// Gets the first offset >= startOffset where the generator wants to construct
        /// an element.
        /// Return -1 to signal no interest.
        public override int GetFirstInterestedOffset(int startOffset)
        {
            var m = FindMatch(startOffset);
            return m.Success ? (startOffset + m.Index) : -1;
        }

        public override VisualLineElement ConstructElement(int offset)
        {
            var m = FindMatch(offset);
            if (m==null || !m.Success || m.Index != 0)
            {
                return null;
            }

            var line = new CustomLinkVisualLineText(m.Value, CurrentContext.VisualLine, m.Length);
            line.CustomLinkClicked += CustomLinkClicked;
            return line;
        }

    }
}
