using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;
using WpfGrabber.Readers;
using WpfGrabber.Shell;

namespace WpfGrabber.ViewParts
{
    public class TextViewPartVM : SimpleDataObject
    {
        #region WrapLines property
        private bool _wrapLines;
        public bool WrapLines
        {
            get => _wrapLines;
            set => Set(ref _wrapLines, value);
        }
        #endregion

        [XmlIgnore]
        public ObservableCollection<string> Lines { get; private set; } = new ObservableCollection<string>();

        public string Text
        {
            get { return string.Join("\n", Lines); }
            set { }
        }
        public TextViewPartVM()
        {
            Lines.CollectionChanged += (s, e) => DoPropertyChanged(nameof(Text));
        }
    }

    public class TextViewPartBase : ViewPartDataViewer<TextViewPartVM> { }

    public partial class TextViewPart : TextViewPartBase
    {
        public static ViewPartDef<TextViewPart> GetDef(string title)
            => new ViewPartDef<TextViewPart>() { Title = title };

        public static TextViewPart ShowPart(string title, params string[] lines)
        {
            var vps = App.Current.ServiceProvider.GetService<IViewPartServiceEx>();
            var part = vps.GetOrCreate(TextViewPart.GetDef(title));
            if (part.ViewModel.Lines.Count > 0)
            {
                if (MessageBox.Show(
                    $"There is already some text ${part.ViewModel.Lines} lines).\n" +
                    $"Do you want to replace it?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return null;
                }
            }
            if (lines.Length==1)
                lines = lines[0].Split('\n');
            vps.SetOptions(part, new ViewPartOptions() { Title = title, Width = 0 });
            part.ViewModel.Lines.AddRange(lines, clear: true);
            return part;
        }

        public TextViewPart()
        {
            InitializeComponent();
        }

    }
}
