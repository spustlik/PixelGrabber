using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfGrabber.Properties
{
    partial class Settings
    {
        public Settings()
        {
            this.RecentFileNames = new System.Collections.Specialized.StringCollection();
        }

        internal void GetRecentFileNames(ICollection<string> recentFileNames)
        {
            recentFileNames.Clear();
            foreach (var item in recentFileNames)
            {
                recentFileNames.Add(item);
            }
        }

        internal void SetRecentFileNames(IEnumerable<string> recentFileNames)
        {
            RecentFileNames.Clear();
            RecentFileNames.AddRange(recentFileNames.ToArray());
        }
    }
}
