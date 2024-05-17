using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using WpfGrabber.Controls;
using WpfGrabber.ViewParts;

namespace WpfGrabber.Shell
{

    partial class ShellWindowContent : IViewPartServiceEx
    {
        #region IViewPartService
        public IEnumerable<ViewPart> ViewParts => partsGrid.Children.OfType<ViewPartControl>().Select(x => x.Content).OfType<ViewPart>();

        public void Add(ViewPart viewPart)
        {
            if (GetViewPartControl(viewPart).index >= 0)
                return;

            //add ([splitter] if not first)[viewpart]
            if (partsGrid.Children.Count > 0)
                partsGrid.Children.Add(new GridSplitter());
            partsGrid.Children.Add(new ViewPartControl() { Content = viewPart });
            FixGridLayout();
            viewPart.OnInitialize();
            projectManager.SetDirty(true);
        }

        public void Remove(ViewPart viewPart)
        {
            var i = GetViewPartControl(viewPart).index;
            if (i < 0)
                return;
            viewPart.OnClose();

            partsGrid.Children.RemoveAt(i); //remove viewPartControl
                                            // []  [{VP,} GS,VP] [VP,GS,{VP}] [{VP,} GS, VP, GS, VP]  [VP, GS, {VP,} GS, VP] [VP, GS, VP, GS, {VP}] ...
            if (i == 0 && partsGrid.Children.Count > 0)
            {
                partsGrid.Children.RemoveAt(0); //remove splitter after first VP
            }
            else if (i > 1)
            {
                partsGrid.Children.RemoveAt(i - 1); //remove splitter before deleted VP
            }
            FixGridLayout();
            projectManager.SetDirty(true);
        }

        public void RemoveAll()
        {
            foreach (var vp in ViewParts.Reverse())
            {
                Remove(vp);
            }
        }
        public void SetOptions(ViewPart viewPart, ViewPartOptions options)
        {
            var (vpc, i, w) = GetViewPartControl(viewPart);
            if (i < 0)
                return;
            if(!string.IsNullOrEmpty(options.Title))
                vpc.Title = options.Title;
            if (options.Width.HasValue)
            {
                if (options.Width < 50)
                {
                    options.Width = DEFAULT_WIDTH;
                }
                if (i == partsGrid.ColumnDefinitions.Count - 1)
                {
                    //this width will be replaced with Star
                    var max = ViewParts
                        .Select(vp => GetViewPartControl(vp))
                        .OrderByDescending(a => a.width)
                        .FirstOrDefault();

                    if(max.control!=null && max.width>0)
                    {
                        partsGrid.ColumnDefinitions[max.index].Width = new GridLength(max.width - options.Width.Value);
                    }
                }
                else
                {
                    partsGrid.ColumnDefinitions[i].Width = new GridLength(options.Width.GetValueOrDefault());
                }
            }
            FixGridLayout();
        }
        #endregion

        #region IViewPartServiceEx
        IEnumerable<ViewPart> IViewPartServiceEx.ViewParts => this.ViewParts;

        void IViewPartServiceEx.FixGridLayout() => this.FixGridLayout();

        (ViewPartControl vpc, int index, int width) IViewPartServiceEx.GetViewPartControl(ViewPart viewPart) 
            => this.GetViewPartControl(viewPart);

        #endregion

        private (ViewPartControl control, int index, int width) GetViewPartControl(ViewPart viewPart)
        {
            for (int i = 0; i < partsGrid.Children.Count; i++)
            {
                var vpc = partsGrid.Children[i] as ViewPartControl;
                if (vpc?.Content == viewPart)
                {
                    var width = (int)vpc.ActualWidth;
                    //star width is not stored
                    var col = Grid.GetColumn(vpc);
                    if (partsGrid.ColumnDefinitions[col].Width.IsStar)
                        width = 0;
                    return (control: vpc, index: i, width);
                }
            }
            return (control: null, index: -1, width: 0);
        }

        private void FixGridLayout()
        {
            //fix column definitions to be same as children
            var columns = ViewParts.Count();
            if (columns > 0)
                columns = columns * 2 - 1; // []  [VP]  [VP,GS,VP]  [VP,GS,VP,GS,VP]...

            while (partsGrid.ColumnDefinitions.Count > columns)
                partsGrid.ColumnDefinitions.Remove(partsGrid.ColumnDefinitions.Last());
            while (partsGrid.ColumnDefinitions.Count < columns)
                partsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(DEFAULT_WIDTH) });
            //fix Width of coldef[0]
            //fix Grid.Column of control to index
            for (int i = 0; i < partsGrid.ColumnDefinitions.Count; i++)
            {
                var col = partsGrid.ColumnDefinitions[i];
                var con = partsGrid.Children[i];
                if (i % 2 == 1)
                {
                    if (!(con is GridSplitter))
                    {
                        con = new GridSplitter();
                        partsGrid.Children.Insert(i, con);
                    }
                    col.Width = GridLength.Auto;
                    Grid.SetColumn(con, i);
                    continue;
                }
                if (i == partsGrid.Children.Count - 1)
                {
                    //last viewpart - star
                    col.Width = new GridLength(1, GridUnitType.Star);
                }
                else
                {
                    //convert star viewpart to implicit width
                    if (col.Width.IsStar)
                        if (con is ViewPartControl vpc)
                            //if (!double.IsNaN(vpc.ActualWidth))
                            col.Width = new GridLength(DEFAULT_WIDTH);
                }
                Grid.SetColumn(con, i);
            }
            if (partsGrid.Children.Count != columns)
                throw new Exception("Columns error");
        }

    }
}
