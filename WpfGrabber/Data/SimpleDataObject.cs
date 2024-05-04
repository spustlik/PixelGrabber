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
            if (propertyName != nameof(IsChanged))
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

        #region IsChanged property
        private bool _isChanged;
        [XmlIgnore]
        public bool IsChanged
        {
            get => _isChanged;
            private set => Set(ref _isChanged, value);
        }

        #endregion


        public event PropertyChangedEventHandler PropertyChanged;
    }
}
