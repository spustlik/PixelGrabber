using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfGrabber.Shell;
using WpfGrabber.ViewParts;

namespace WpfGrabber.Controls
{
    public class ViewPartControl : ContentControl
    {
        public static RoutedCommand CommandClose = new RoutedCommand();
        //public static RoutedCommand CommandMove = new RoutedCommand();
        static ViewPartControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ViewPartControl), new FrameworkPropertyMetadata(typeof(ViewPartControl)));
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            //CommandBindings.Add(new CommandBinding(CommandMove, MoveCommand_Executed));
            CommandBindings.Add(new CommandBinding(CommandClose, CloseCommand_Executed));
        }


        private void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var vps = App.Current.ServiceProvider.GetService<IViewPartService>();
            vps.Remove(this.Content as ViewPart);
        }

        #region Title dp
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(ViewPartControl), new PropertyMetadata());
        #endregion

    }
}
