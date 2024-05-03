using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace WpfGrabber.Controls
{
    public partial class InputQueryWindow : Window
    {
        public InputQueryWindow()
        {
            InitializeComponent();
        }

        public static bool ShowModal(string prompt, out string result, string def = null, string title = null)
        {
            var w = new InputQueryWindow();
            w.Owner = App.Current.MainWindow;
            w.Title = title ?? w.Title;
            w.prompt.Content = prompt;
            result = def;
            w.text.Text = def;
            var r = w.ShowDialog();
            if (r != true)
                return false;
            result = w.text.Text;
            return true;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
