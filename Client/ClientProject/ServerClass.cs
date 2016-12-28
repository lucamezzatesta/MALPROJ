using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Diagnostics;
using System.Windows.Threading;

namespace ClientProject
{
    public class ServerClass :ObservableObject
    {
        private string _ipAddressString;
        private Socket _socket;
        private DispatcherTimer timer;
        private Stopwatch stopWatch;
        private double _TimeElapsed;

        public string ipAddressString
        {
            get { return _ipAddressString; }
            set
            {
                if (_ipAddressString != value)
                {
                    _ipAddressString = value;
                    OnPropertyChanged("ipAddressString");
                }
            }
        }

        public Socket socket
        {
            get { return _socket; }
            set
            {
                if (_socket != value)
                {
                    _socket = value;
                    OnPropertyChanged("Socket");
                }
            }
        }

        public double TimeElapsed
        {
            get { return _TimeElapsed; }
            set
            {
                if (_TimeElapsed != value)
                {
                    _TimeElapsed = value;
                    OnPropertyChanged("TimeElapsed");
                }
            }
        }

        public void StartTimer()
        {
            timer = new DispatcherTimer();
            timer.Tick += dispatcherTimertick_;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            stopWatch = new Stopwatch();
            stopWatch.Start();
            timer.Start();
        }

        private void dispatcherTimertick_(object sender, EventArgs e)
        {
            _TimeElapsed = stopWatch.Elapsed.TotalMilliseconds;
            RaisePropertyChanged("TimeElapsed");
        }
    }
}
