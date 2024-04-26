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
	public interface IViewPartService
	{
		void Add(ViewPart viewPart);
		void Remove(ViewPart viewPart);
		void RemoveAll();
		void SetOptions(ViewPart viewPart, string title);
	}

	partial class ShellWindowContent : IViewPartService
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
        }

		public void RemoveAll()
		{
			foreach (var vp in ViewParts.Reverse())
			{
				Remove(vp);
			}
		}
		public void SetOptions(ViewPart viewPart, string title)
		{
			var (vpc, i) = GetViewPartControl(viewPart);
			if (i < 0)
				return;
			vpc.Title = title;
		}
		#endregion

		private (ViewPartControl control, int index) GetViewPartControl(ViewPart viewPart)
		{
			for (int i = 0; i < partsGrid.Children.Count; i++)
			{
				var vpc = partsGrid.Children[i] as ViewPartControl;
				if (vpc?.Content == viewPart)
					return (control: vpc, index: i);
			}
			return (control: null, index: -1);
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
