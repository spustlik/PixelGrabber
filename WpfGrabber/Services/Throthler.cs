using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;

namespace WpfGrabber.Services
{
    public class Throthler
    {
        protected Throthler()
        {
        }
        private bool _called = false;

        public Action Action { get; private set; }
        public DispatcherTimer Timer { get; private set; }

        public static Throthler Create(
            TimeSpan duration,
            Action callback,
            DispatcherPriority priority = DispatcherPriority.Normal,
            bool start = true)
        {
            var r = new Throthler();
            r.Action = callback;
            r.Timer = new DispatcherTimer(duration, priority, (s, e) => r.Timer_Callback(), Application.Current.Dispatcher);
            if (start)
                r.Start();
            return r;
        }

        private void Timer_Callback()
        {
            if (_called)
                return;
            _called = true;
            Action();
        }

        public void Start()
        {
            _called = false;
            Timer.Start();
        }

        public void Stop()
        {
            Timer.Stop();
            Timer_Callback();
        }
    }
}
