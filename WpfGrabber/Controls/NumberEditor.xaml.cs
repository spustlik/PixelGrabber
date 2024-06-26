﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfGrabber.Controls
{
    /// <summary>
    /// Interaction logic for NumberEditor.xaml
    /// </summary>
    public partial class NumberEditor : UserControl
    {
        public NumberEditor()
        {
            InitializeComponent();
            //DataContext = this;
        }


        #region Value
        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(int), typeof(NumberEditor),
                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        #endregion

        #region Minimum
        public int Minimum
        {
            get { return (int)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Minimum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(int), typeof(NumberEditor), new PropertyMetadata(0));

        #endregion

        #region Maximum
        public int Maximum
        {
            get { return (int)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Maximum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(int), typeof(NumberEditor),
                new PropertyMetadata(100));
        #endregion

        #region SliderWidth
        public double SliderWidth
        {
            get { return (double)GetValue(SliderWidthProperty); }
            set { SetValue(SliderWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SliderWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SliderWidthProperty =
            DependencyProperty.Register("SliderWidth", typeof(double), typeof(NumberEditor), new PropertyMetadata(0.0d));
        #endregion

        #region LargeChange
        public int LargeChange
        {
            get { return (int)GetValue(LargeChangeProperty); }
            set { SetValue(LargeChangeProperty, value); }
        }

        public static readonly DependencyProperty LargeChangeProperty =
            DependencyProperty.Register("LargeChange", typeof(int), typeof(NumberEditor), new PropertyMetadata(8));
        #endregion


        private void ButtonPageSize_Click(object sender, RoutedEventArgs e)
        {
            var mi = sender as MenuItem;
            if (mi == null)
                return;
            var s = mi.Header as string;
            if (String.IsNullOrEmpty(s))
                return;
            if (!int.TryParse(s, out var page))
                return;
            if (!(mi.Parent is ContextMenu menu))
                return;
            foreach (var item in menu.Items.OfType<MenuItem>())
            {
                item.IsChecked = s.Equals(item.Header);
            }
            LargeChange = page;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var ofs = Convert.ToInt32((e.OriginalSource as RepeatButton).CommandParameter);
            if (ofs < -1)
                ofs = -LargeChange;
            if (ofs > 1)
                ofs = LargeChange;
            var n = Value + ofs;
            if (n < 0 || n > Maximum)
                return;
            Value = n;
        }
    }
}
