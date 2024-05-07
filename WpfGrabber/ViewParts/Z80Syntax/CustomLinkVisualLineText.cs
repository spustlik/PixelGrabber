using ICSharpCode.AvalonEdit.Rendering;
using System.Windows.Input;
using System.Windows;
using System.Windows.Media.TextFormatting;

namespace WpfGrabber.ViewParts.Z80Syntax
{
    /// <summary>
    /// VisualLineElement that represents a piece of text and is a clickable link.
    /// https://stackoverflow.com/questions/28379206/custom-hyperlinks-using-avalonedit
    /// </summary>
    public class CustomLinkVisualLineText : VisualLineText
    {

        public delegate void CustomLinkClickHandler(string link);

        public event CustomLinkClickHandler CustomLinkClicked;

        private string Link { get; set; }

        public CustomLinkVisualLineText(string theLink, VisualLine parentVisualLine, int length)
            : base(parentVisualLine, length)
        {
            Link = theLink;
        }

        public override TextRun CreateTextRun(int startVisualColumn, ITextRunConstructionContext context)
        {
            //TextRunProperties.SetForegroundBrush(Brushes.GreenYellow);
            //TextRunProperties.SetTextDecorations(TextDecorations.Underline);

            this.TextRunProperties.SetForegroundBrush(context.TextView.LinkTextForegroundBrush);
            this.TextRunProperties.SetBackgroundBrush(context.TextView.LinkTextBackgroundBrush);
            if (context.TextView.LinkTextUnderline)
                this.TextRunProperties.SetTextDecorations(TextDecorations.Underline);

            return base.CreateTextRun(startVisualColumn, context);
        }

        private bool IsLinkClickable()
        {
            if (string.IsNullOrEmpty(Link))
                return false;
            //if (RequireControlModifierForClick)
            //    return (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            //else
                return true;
        }

        protected override void OnQueryCursor(QueryCursorEventArgs e)
        {
            if (IsLinkClickable())
            {
                e.Handled = true;
                e.Cursor = Cursors.Hand;
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && !e.Handled && IsLinkClickable())
            {
                if (CustomLinkClicked != null)
                {
                    CustomLinkClicked(Link);
                    e.Handled = true;
                }
            }
        }

        protected override VisualLineText CreateInstance(int length)
        {
            var a = new CustomLinkVisualLineText(Link, ParentVisualLine, length)
            {
                //RequireControlModifierForClick = RequireControlModifierForClick,
                CustomLinkClicked = CustomLinkClicked,
            };
            return a;
        }
    }
}
