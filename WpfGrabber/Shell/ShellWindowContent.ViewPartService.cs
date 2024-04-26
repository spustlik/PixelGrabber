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
		private void UpdateGrid()
		{
			//fix column definitions to be same as children
			while (partsGrid.ColumnDefinitions.Count > partsGrid.Children.Count)
				partsGrid.ColumnDefinitions.Remove(partsGrid.ColumnDefinitions.Last());
			while (partsGrid.ColumnDefinitions.Count < partsGrid.Children.Count)
				partsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(DEFAULT_WIDTH) });
			//fix Width of coldef[0]
			//fix Grid.Column of control to index
			for (int i = 0; i < partsGrid.ColumnDefinitions.Count; i++)
			{
				var col = partsGrid.ColumnDefinitions[i];
				var con = partsGrid.Children[i];
				if (i == partsGrid.Children.Count - 1)
				{
					//last viewpart - star
					col.Width = new GridLength(1, GridUnitType.Star);
				}
				else
				{
					if (i % 2 == 1)
					{
						//splitter
						col.Width = GridLength.Auto;
					}
					else
					{
						//convert star viewpart to implicit width
						if (col.Width.IsStar)
							if (con is ViewPartControl vpc)
								//if (!double.IsNaN(vpc.ActualWidth))
								col.Width = new GridLength(DEFAULT_WIDTH);
					}
				}
				Grid.SetColumn(con, i);
			}
		}

		public IEnumerable<ViewPart> ViewParts => partsGrid.Children.OfType<ViewPartControl>().Select(x => x.Content).OfType<ViewPart>();

		private (ViewPartControl control, int index) GetViewPartControl(ViewPart viewPart)
		{
			for (int i = 0; i < partsGrid.Children.Count; i++)
			{
				var vpc = partsGrid.Children[i] as ViewPartControl;
				if (vpc?.Content == viewPart)
					return (control:vpc,index:i);
			}
			return (control: null, index: -1);
		}

		void IViewPartService.Add(ViewPart viewPart)
		{
			if (GetViewPartControl(viewPart).index >= 0)
				return;

			//add ([splitter] if not first)[viewpart]
			if (partsGrid.Children.Count > 0)
				partsGrid.Children.Add(new GridSplitter());
			partsGrid.Children.Add(new ViewPartControl() { Content = viewPart });
			UpdateGrid();
			viewPart.OnInitialize();
		}

		void IViewPartService.Remove(ViewPart viewPart)
		{
			RemoveViewPart(viewPart);
		}

		private void RemoveViewPart(ViewPart viewPart)
		{
			var i = GetViewPartControl(viewPart).index;
			if (i < 0)
				return;
			viewPart.OnClose();
			partsGrid.Children.RemoveAt(i); //remove viewPartControl
			partsGrid.Children.RemoveAt(i); //remove splitter
			UpdateGrid();
		}

		void IViewPartService.RemoveAll()
		{
			foreach (var vp in ViewParts.Reverse())
			{
				RemoveViewPart(vp);
			}
		}
		void IViewPartService.SetOptions(ViewPart viewPart, string title)
		{
			var (vpc, i) = GetViewPartControl(viewPart);
			if (i < 0)
				return;
			vpc.Title = title;
		}

	}
}
