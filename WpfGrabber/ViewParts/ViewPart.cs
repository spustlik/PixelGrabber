using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WpfGrabber.ViewParts
{
    public class ViewPart : UserControl
    {
        public string Title { get; protected set; }
        public ViewPart()
        {
            Title = GetType().Name.Split('.').Last();
        }
        public virtual void OnDataChanged(DataChangedArgs args)
        {

        }
        public virtual void OnInitialize()
        {

        }
        public virtual void OnClose()
        {

        }
    }

    public class DataChangedArgs
    {
        public byte[] Data { get; set; }
        public int Offset { get; set; }
        public string FileName { get; set; }
    }
}
