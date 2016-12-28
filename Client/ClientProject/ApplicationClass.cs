using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Diagnostics;
using System.Net.Sockets;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.Drawing;

namespace ClientProject
{
    public class ApplicationClass :ObservableObject
    {
        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        //VARIABILI PRIVATE
        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        private string _exe;
        private string _nomeFinestra;
        private string _windowHandle;   //TODO: controlla! è una stringa?
        private string _pId;            //TODO: controlla! è una stringa?
        private int _sizeIcona;
        private BitmapImage _icona = null;
        //private byte[] _icona = new byte[4092];          //TODO: controlla! è una stringa?
        private string _stato;
        private ServerClass _server;
        private DispatcherTimer timer;
        private Stopwatch stopWatch;
        private double _TimeElapsed;
        private ObservableCollection<ServerClass> _serverList = new ObservableCollection<ServerClass>();
        private ObservableCollection<IpAddressClass> _ipServerList = new ObservableCollection<IpAddressClass>();
        private bool alreadyStarted = false;


        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        //VARIABILI PUBBLICHE
        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/


        public string exe
        {
            get { return _exe; }
            set
            {
                if (_exe != value)
                {
                    _exe = value;
                    OnPropertyChanged("exe");
                }
            }
        }

        public string nomeFinestra
        {
            get { return _nomeFinestra; }
            set
            {
                if (_nomeFinestra != value)
                {
                    _nomeFinestra = value;
                    OnPropertyChanged("ApplicationName");
                }
            }
        }

        public string nomeCompleto
        {
            get { return _nomeFinestra + "\n" + _exe; }
        }

        public string windowHandle
        {
            get { return _windowHandle; }
            set
            {
                if (_windowHandle != value)
                {
                    _windowHandle = value;
                    OnPropertyChanged("WindowHandle");
                }
            }
        }

        public string pId
        {
            get { return _pId; }
            set
            {
                if (_pId != value)
                {
                    _pId = value;
                    OnPropertyChanged("PId");
                }
            }
        }

        public int sizeIcona
        {
            get { return _sizeIcona; }
            set
            {
                if (_sizeIcona != value)
                {
                    _sizeIcona = value;
                    OnPropertyChanged("SizeIcona");
                }
            }
        }

        public BitmapImage icona
        {
            get { return _icona; }
            set
            {
                //if (_icona != value)
                //{
                //    _icona = value;
                //    OnPropertyChanged("Icona");
                //}
                if (_icona == null)
                {
                    //_icona = new Icon(SystemIcons.Exclamation, 40, 40);
                    _icona = new BitmapImage();
                    _icona = value;
                    OnPropertyChanged("icona");
                }
                else
                {
                    _icona = value;
                    OnPropertyChanged("icona");
                }
            }
        }

        public string stato
        {
            get { return _stato; }
            set
            {
                if(_stato != value)
                {
                    _stato = value;
                    OnPropertyChanged("Stato");
                }
            }
        }

        public double TimeElapsed
        {
            get { return _TimeElapsed; }
            set
            {
                if(_TimeElapsed != value)
                {
                    _TimeElapsed = value;
                    OnPropertyChanged("TimeElapsed");
                }
            }
        }

        public ServerClass server
        {
            get { return _server; }
            set
            {
                if (_server != value)
                {
                    _server = value;
                    OnPropertyChanged("Server");
                }
            }
        }

        public ObservableCollection<ServerClass> serverList
        {
            get { return _serverList; }
            set
            {
                if (_serverList != value)
                {
                    _serverList = value;
                    OnPropertyChanged("serverList");
                }
            }
        }

        public ObservableCollection<IpAddressClass> ipServerList
        {
            get { return _ipServerList; }
            set
            {
                if (_ipServerList != value)
                {
                    _ipServerList = value;
                    OnPropertyChanged("ipServerList");
                }
            }
        }

        public DispatcherTimer timerP
        {
            get { return timer; }
        }
            


        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        //METODI
        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        public void StopTimer()
        {
            if (stopWatch != null)
            {
                stopWatch.Stop();
            }
        }

        public void StartTimer()
        {
            if(alreadyStarted == false)
            {
                timer = new DispatcherTimer();
                timer.Tick += dispatcherTimertick_;
                timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
                stopWatch = new Stopwatch();
                stopWatch.Start();
                timer.Start();
                alreadyStarted = true;
            }
            else
            {
                stopWatch.Start();
            }
        }

        private void dispatcherTimertick_(object sender, EventArgs e)
        {
            double temp = stopWatch.Elapsed.TotalMilliseconds;
            if(_server!=null)
            {
                _TimeElapsed = Math.Round((temp * 100) / _server.TimeElapsed);
                if (_TimeElapsed >= 100)
                {
                    _TimeElapsed = 100;
                }
                RaisePropertyChanged("TimeElapsed");
            }
           
        }
    }
}
