using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WpfGrabber
{
    public abstract class SimpleDataObject : INotifyPropertyChanged
    {
        protected virtual void DoPropertyChanged([CallerMemberName] string propertyName = null)
        {
            IsChanged = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }
            field = value;
            DoPropertyChanged(propertyName);
            return true;
        }

        [XmlIgnore]
        public bool IsChanged { get; protected set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
