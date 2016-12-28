using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Threading;
using System.Diagnostics;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Windows.Media;


namespace ClientProject
{
    public class ServerViewModel : ObservableObject, IPageViewModel
    {
        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        //VARIABILI PRIVATE
        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        private byte[] bytes = new byte[8192];  
        private int _nWindows;
        private string _ip;
        private string _stringCommand;
        private Key _currentCommand;
        private Socket _socket;
        private IpAddressClass _selectedIP;
        private IPAddress _ipConnectedAddress;
        private ApplicationClass _selectedApp;
        private ApplicationClass _appFocus;
        private ApplicationViewModel _aVM;
        private ApplicationClass _newApp;
        private ServerClass _currentServer;
        private bool _redPanelVisibility;
        private bool _bluePanelVisibility;
        private object thisLock = new object();

        /*........................................................................................................................................................................*/
        //LISTE e COLLEZIONI
        private List<string> _keyList = new List<string>();
        private ObservableCollection<IpAddressClass> _ipList = new ObservableCollection<IpAddressClass>();
        //private ObservableCollection<IPAddress> _ipAddressList = new ObservableCollection<IPAddress>();
        private ObservableCollection<ApplicationClass> _applicationList = new ObservableCollection<ApplicationClass>();
        private ObservableCollection<ApplicationClass> _currentApplicationList = new ObservableCollection<ApplicationClass>();
        private ObservableCollection<ServerClass> _serverList = new ObservableCollection<ServerClass>();
        private Dictionary<Socket, Thread> _threadList = new Dictionary<Socket, Thread>();
        private Dictionary<IPAddress, Socket> _socketList = new Dictionary<IPAddress, Socket>();
        private Dictionary<Socket, ObservableCollection<ApplicationClass>> _applicationForSocket = new Dictionary<Socket, ObservableCollection<ApplicationClass>>();
        private Dictionary<ServerClass, ObservableCollection<ApplicationClass>> _serverAppList = new Dictionary<ServerClass, ObservableCollection<ApplicationClass>>();
        //private Dictionary<string, ObservableCollection<ApplicationClass>> _appInFocusList = new Dictionary<string, ObservableCollection<ApplicationClass>>();
        private Dictionary<string, ObservableCollection<ApplicationClass>> _applicationForExe = new Dictionary<string, ObservableCollection<ApplicationClass>>();
        private Dictionary<Socket, bool> _keepListeningList = new Dictionary<Socket, bool>();
        //private Dictionary<ApplicationClass, ObservableCollection<ServerClass>> _appAndServer = new Dictionary<ApplicationClass, ObservableCollection<ServerClass>>();

        /*........................................................................................................................................................................*/
        //COMANDI
        private ICommand _connectCommand;
        private ICommand _disconnectCommand;
        private ICommand _changeServerCommand;
        private ICommand _sendCommand;



        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        //VARIABILI PUBBLICHE
        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/




        public ApplicationViewModel avm
        {
            get { return _aVM; }
            set
            {
                if (_aVM != value)
                {
                    _aVM = value;
                }
            }
        }


        public string Name
        {
            get
            {
                return "Server";
            }
        }

        public string ip
        {
            get
            {
                return _ip;
            }
            set
            {
                if (_ip != value)
                {
                    _ip = value;
                    OnPropertyChanged("IP");
                }
            }
        }

        public string stringCommand
        {
            get { return _stringCommand; }
            set
            {
                if (_stringCommand != value)
                {
                    _stringCommand = value;
                    OnPropertyChanged("stringCommand");
                }
            }
        }

        public Key currentCommand
        {
            get { return _currentCommand; }
            set
            {
                if (_currentCommand != value)
                {
                    _currentCommand = value;
                    OnPropertyChanged("currentCommand");
                }
            }
        }


        public IpAddressClass selectedIp
        {
            get { return _selectedIP; }
            set
            {
                if (_selectedIP != value)
                {
                    _selectedIP = value;
                    ChangeServer();
                    OnPropertyChanged("selectedIP");
                }
            }
        }

        public IPAddress ipConnectedAddress
        {
            get { return _ipConnectedAddress; }
            set
            {
                if (_ipConnectedAddress != value)
                {
                    _ipConnectedAddress = value;
                    OnPropertyChanged("IPAddress");
                }
            }
        }

        public ApplicationClass appFocus
        {
            get
            {
                return _appFocus;
            }
            set
            {
                if (_appFocus != value)
                {
                    _appFocus = value;
                    OnPropertyChanged("AppFocus");
                }
            }
        }

        public ApplicationClass selectedApp
        {
            get { return _selectedApp; }
            set
            {
                if (_selectedApp != value)
                {
                    _selectedApp = value;
                    OnPropertyChanged("selectedApp");
                }
            }
        }

        public ApplicationClass newApp
        {
            get { return _newApp; }
            set
            {
                if (_newApp != value)
                {
                    _newApp = value;
                    OnPropertyChanged("newApp");
                }
            }
        }

        public ServerClass currentServer
        {
            get { return _currentServer; }
            set
            {
                if (_currentServer != value)
                {
                    _currentServer = value;
                    OnPropertyChanged("currentServer");
                }
            }
        }

        public bool redPanelVisibility
        {
            get { return _redPanelVisibility; }
            set
            {
                if (_redPanelVisibility != value)
                {
                    _redPanelVisibility = value;
                    OnPropertyChanged("redPanelVisibility");
                }
            }
        }

        public bool bluePanelVisibility
        {
            get { return _bluePanelVisibility; }
            set
            {
                if (_bluePanelVisibility != value)
                {
                    _bluePanelVisibility = value;
                    OnPropertyChanged("bluePanelVisibility");
                }
            }
        }

        /*........................................................................................................................................................................*/
        //LISTE e COLLEZIONI

        public ObservableCollection<IpAddressClass> ipList
        {
            get { return _ipList; }
            set
            {
                _ipList = value;
                OnPropertyChanged("IPList");
            }
        }

        public ObservableCollection<ApplicationClass> applicationList
        {
            get { return _applicationList; }
            set
            {
                _applicationList = value;
                OnPropertyChanged("ApplicationList");
            }
        }

        public ObservableCollection<ApplicationClass> currentApplicationList
        {
            get { return _currentApplicationList; }
            set
            {
                if (_currentApplicationList != value)
                {
                    _currentApplicationList = value;
                    OnPropertyChanged("CurrentApplicationList");
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
                    OnPropertyChanged("ServerList");
                }
            }
        }

        public Dictionary<Socket, ObservableCollection<ApplicationClass>> applicationForSocket
        {
            get { return _applicationForSocket; }
            set
            {
                if (_applicationForSocket != value)
                {
                    _applicationForSocket = value;
                    OnPropertyChanged("ApplicationForSocket");
                }
            }
        }

        public Dictionary<ServerClass, ObservableCollection<ApplicationClass>> serverAppList
        {
            get { return _serverAppList; }
            set
            {
                if (_serverAppList != value)
                {
                    _serverAppList = value;
                    OnPropertyChanged("ServerAppList");
                }
            }
        }

        public Dictionary<string, ObservableCollection<ApplicationClass>> applicationForExe
        {
            get { return _applicationForExe; }
            set
            {
                if (_applicationForExe != value)
                {
                    _applicationForExe = value;
                    RaisePropertyChanged("applicationForExe");
                }
            }
        }

        public Dictionary<Socket, bool> keepListeningList
        {
            get { return _keepListeningList; }
            set
            {
                if (_keepListeningList != value)
                {
                    _keepListeningList = value;
                    OnPropertyChanged("keepListeningList");
                }
            }
        }

        public Dictionary<IPAddress, Socket> socketList
        {
            get { return _socketList; }
            set
            {
                if (_socketList != value)
                {
                    _socketList = value;
                    OnPropertyChanged("socketList");
                }
            }
        }

        public Dictionary<Socket, Thread> threadList
        {
            get { return _threadList; }
            set
            {
                if (_threadList != value)
                {
                    _threadList = value;
                    OnPropertyChanged("threadList");
                }
            }
        }


        /*........................................................................................................................................................................*/
        //COMANDI

        public ICommand ConnectCommand
        {
            get
            {
                if (_connectCommand == null)
                {
                    _connectCommand = new RelayCommand(
                        param => Connect());
                }
                return _connectCommand;
            }
        }

        public ICommand DisconnectCommand
        {
            get
            {
                if (_disconnectCommand == null)
                {
                    _disconnectCommand = new RelayCommand(
                        param => Disconnect());
                }
                return _disconnectCommand;
            }
        }

        public ICommand ChangeServerCommand
        {
            get
            {
                if (_changeServerCommand == null)
                {
                    _changeServerCommand = new RelayCommand(
                        param => ChangeServer());
                }
                return _changeServerCommand;
            }
        }

        public ICommand SendCommand
        {
            get
            {
                if (_sendCommand == null)
                {
                    _sendCommand = new RelayCommand(
                        param => Send());
                }
                return _sendCommand;
            }
        }




        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        //METDODI
        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/




        //cambia la view quando si sceglie un nuovo ip
        private void ChangeServer()
        {
            ServerClass serverFound;
            foreach (ServerClass se in serverAppList.Keys)
            {
                if (_selectedIP != null)
                {
                    if (se.ipAddressString == _selectedIP.ipAddressString)
                    {
                        serverFound = se; 
                        _currentServer = se;
                        _currentApplicationList = _serverAppList[se];
                        RaisePropertyChanged("CurrentApplicationList");

                        bool found = false;
                        foreach (ApplicationClass ap in serverAppList[se])
                        {
                            if (ap.stato != null)
                            {
                                if (ap.stato.Equals("In Focus"))
                                {
                                    _selectedApp = ap;
                                    RaisePropertyChanged("selectedApp");
                                    _appFocus = ap;
                                    RaisePropertyChanged("appFocus");
                                    found = true;
                                }
                            }
                        }
                        if (found == false)
                        {
                            _selectedApp = new ApplicationClass();
                            RaisePropertyChanged("selectedApp");
                            _appFocus = new ApplicationClass();
                            RaisePropertyChanged("appFocus");
                        }
                    }
                }
            }
            return;
        }




        /*........................................................................................................................................................................*/


        //get pressed keys
        public void getKey(KeyEventArgs e)
        {
            Key k = e.Key;
            string sk = k.ToString();
            if(_keyList.Count == 0)
            {
                _stringCommand = sk;
                RaisePropertyChanged("stringCommand");
            }
            else
            {
                _stringCommand += " + " + sk;
                RaisePropertyChanged("stringCommand");
            }
            var vk = KeyInterop.VirtualKeyFromKey(k);
            string hexString = vk.ToString("X");
            _keyList.Add(hexString);
            return;
        }



        /*........................................................................................................................................................................*/



        //send pressed keys to server      
        private void Send()
        {
            if (_keyList.Count != 0)
            {
                if (_currentServer != null)
                {
                    int nKeys = _keyList.Count;
                    Socket so = _currentServer.socket;
                    byte[] message = new byte[8192];
                    for (int j = 0; j < 8192; j++)
                    {
                        message[j] = Convert.ToByte('\0');
                    }

                    string toSend = "command" + " " + _appFocus.windowHandle + " " + _appFocus.pId + " " + nKeys + " ";

                    for (int c = 0; c < _keyList.Count; c++)
                    {
                        toSend += _keyList[c] + " ";
                    }

                    byte[] messageTemp = System.Text.Encoding.Default.GetBytes(toSend);
                    string prova = System.Text.Encoding.Default.GetString(messageTemp);  //elimina
                    for (int j = 0; j < (messageTemp.Length); j++)
                    {
                        message[j] = messageTemp[j];
                    }
                    prova = System.Text.Encoding.Default.GetString(message);  //elimina
                    so.Send(message);

                    for (int j = 0; j < 8192; j++)
                    {
                        message[j] = Convert.ToByte('\0');
                    }

                    byte[] serverAnswer = new byte[8192];
                    //so.Receive(serverAnswer);
                    prova = System.Text.Encoding.Default.GetString(serverAnswer);  //elimina

                    string okToSend = "ok\n";
                    byte[] okMessage = System.Text.Encoding.Default.GetBytes(okToSend);
                    prova = System.Text.Encoding.Default.GetString(okMessage);  //elimina
                    so.Send(okMessage);
                }
                else
                {
                    WindowNoServer ww = new WindowNoServer();
                    var vmm = new SecondWindowViewModel();
                    vmm.ClosingRequest += ww.Close;
                    ww.DataContext = vmm;
                    ww.Show();
                }
            }
            else
            {
                WindowNoKeyInserted ww = new WindowNoKeyInserted();
                var vmm = new SecondWindowViewModel();
                vmm.ClosingRequest += ww.Close;
                ww.DataContext = vmm;
                ww.Show();
            }
            _stringCommand = "";
            RaisePropertyChanged("stringCommand");
            _keyList.Clear();
            return;
        }



        /*........................................................................................................................................................................*/



        //set visible blue panel
        private void RedPanelVisible()
        {
            lock (thisLock)
            {
                if (bluePanelVisibility == false)
                {
                    redPanelVisibility = true;
                    System.Windows.Forms.Timer timerRed = new System.Windows.Forms.Timer();
                    timerRed.Interval = 3000;
                    timerRed.Tick += (object sender, EventArgs e) => { bluePanelVisibility = false; timerRed.Stop(); };
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        timerRed.Start();
                    });
                    RaisePropertyChanged("redPanelVisibility");
                }
            }
            return;
        }



        /*........................................................................................................................................................................*/



        //set visible blue panel
        private void BluePanelVisible()
        {
            lock (thisLock)
            {
                redPanelVisibility = false;
            bluePanelVisibility = true;
            System.Windows.Forms.Timer timerBlue = new System.Windows.Forms.Timer();
            timerBlue.Interval = 3000;
            timerBlue.Tick += (object sender, EventArgs e) => { bluePanelVisibility = false; timerBlue.Stop(); };
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                timerBlue.Start();
            });
            RaisePropertyChanged("bluePanelVisibility");
            }
            
            return;
        }



        /*........................................................................................................................................................................*/



        //Ascolta se arrivano messaggi dal client per cambio focus o nuova applicazione o applicazione chiusa
        private void Listen(ServerClass serv)
        {
                byte[] bRec = new byte[8192];
                byte[] partRec = new byte[8192];
                bool focusRec = false;
                bool closedRec = false;
                bool newRec = false;
                //bool iconFound = false;
                bool newWin = false;
                int nClosed = 0;
                int nNew = 0;
                int nClosedRec = 0;
                int nNewRec = 0;
                int bytesRec = 0;
                int countBRec = 0;
                int countPRec = 0;
                string whDelete = "";
                string pIdDelete;
                ApplicationClass appRec = new ApplicationClass();

            while (_keepListeningList[serv.socket] == true)
            {
                
                    if (!(serv.socket.Poll(1, SelectMode.SelectRead) && serv.socket.Available == 0))
                    {
                        string prova;
                        try
                        {
                            bytesRec = serv.socket.Receive(bRec);
                            prova = System.Text.Encoding.Default.GetString(bRec);
                        }
                        catch (SocketException se)
                        {
                            Console.WriteLine("Socket Exception: {0}", se.ToString());
                            return;
                        }

                        lock (thisLock)
                        {
                        //è arrivato qualcosa
                        if (bytesRec != 0)
                        {
                            byte[] okMessage = new byte[8192];
                            for (int j = 0; j < 8192; j++)
                            {
                                okMessage[j] = Convert.ToByte('\0');
                            }

                            string toSend = "ok\n";

                            byte[] okMessageTemp = System.Text.Encoding.Default.GetBytes(toSend);
                            prova = System.Text.Encoding.Default.GetString(okMessageTemp);  //elimina
                            for (int j = 0; j < (okMessageTemp.Length); j++)
                            {
                                okMessage[j] = okMessageTemp[j];
                            }
                            prova = System.Text.Encoding.Default.GetString(okMessage);  //elimina
                            try
                            {
                                serv.socket.Send(okMessageTemp);
                            }
                            catch (SocketException e)
                            {
                                Console.WriteLine("Exception : {0}", e.ToString());
                                return;
                            }


                            for (int i = 0; i < 8192; i++)
                            {
                                //controllo di non essere alla fine del messaggio
                                if (bRec[i] != Convert.ToByte('\0'))
                                {
                                    //controllo che il messaggio non sia vuoto
                                    if ((countPRec == 2 && bRec[i] != Convert.ToByte('\n') && (newRec == true || closedRec == true)) || newWin == true)
                                    {
                                        if (newRec == true || newWin == true)
                                        {
                                            if (bRec[i] != Convert.ToByte('\n') && bRec[i] != Convert.ToByte('\0'))
                                            {
                                                partRec[countBRec] = bRec[i];
                                                countBRec++;
                                            }
                                            else
                                            {
                                                switch (countPRec)
                                                {
                                                    case 0:
                                                        {
                                                            string sp = System.Text.Encoding.Default.GetString(partRec);
                                                            string[] s = sp.Split('\0');
                                                            appRec.exe = s[0];
                                                            countPRec++;
                                                            countBRec = 0;
                                                            for (int c = 0; c < 8192; c++)
                                                            {
                                                                partRec[c] = Convert.ToByte('\0');
                                                            }
                                                            break;
                                                        }
                                                    case 1:
                                                        {
                                                            string sp = System.Text.Encoding.Default.GetString(partRec);
                                                            string[] s = sp.Split('\0');
                                                            appRec.nomeFinestra = s[0];
                                                            if (appRec.exe.Equals(""))
                                                            {
                                                                appRec.exe = "App Senza Exe";
                                                                //appRec.nomeFinestra = "App Senza Nome";
                                                            }
                                                            countPRec++;
                                                            countBRec = 0;
                                                            for (int c = 0; c < 8192; c++)
                                                            {
                                                                partRec[c] = Convert.ToByte('\0');
                                                            }
                                                            break;
                                                        }
                                                    case 2:
                                                        {
                                                            string sp = System.Text.Encoding.Default.GetString(partRec);
                                                            string[] s = sp.Split('\0');
                                                            appRec.windowHandle = s[0];
                                                            countPRec++;
                                                            countBRec = 0;
                                                            for (int c = 0; c < 8192; c++)
                                                            {
                                                                partRec[c] = Convert.ToByte('\0');
                                                            }
                                                            break;
                                                        }
                                                    case 3:
                                                        {
                                                            string sp = System.Text.Encoding.Default.GetString(partRec);
                                                            string[] s = sp.Split('\0');
                                                            appRec.pId = s[0];
                                                            countPRec++;
                                                            countBRec = 0;
                                                            for (int c = 0; c < 8192; c++)
                                                            {
                                                                partRec[c] = Convert.ToByte('\0');
                                                            }
                                                            break;
                                                        }
                                                    case 4:
                                                        {
                                                            string sp = System.Text.Encoding.Default.GetString(partRec);
                                                            string[] s = sp.Split('\0');
                                                            appRec.sizeIcona = int.Parse(s[0]);
                                                            countBRec = 0;
                                                            for (int j = 0; j < 8192; j++)
                                                            {
                                                                partRec[j] = Convert.ToByte('\n');
                                                            }
                                                            if (appRec.sizeIcona > 0)
                                                            {
                                                                for (int j = i + 1, c = 0; j < appRec.sizeIcona; j++, c++)
                                                                {
                                                                    partRec[c] = bRec[j];
                                                                }
                                                                var image = new BitmapImage();
                                                                using (var mem = new MemoryStream(partRec))
                                                                {
                                                                    mem.Position = 0;
                                                                    image.BeginInit();
                                                                    image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                                                                    image.CacheOption = BitmapCacheOption.OnLoad;
                                                                    image.UriSource = null;
                                                                    image.StreamSource = mem;
                                                                    image.EndInit();
                                                                }
                                                                image.Freeze();
                                                                appRec.icona = image;
                                                            }

                                                            for (int j = 0; j < 8192; j++)
                                                            {
                                                                partRec[j] = Convert.ToByte('\0');
                                                            }

                                                            //l'applicazione che prima era in focus ora non lo è più
                                                            if (_selectedIP != null && _selectedIP.ipAddressString != null && serv != null && serv.ipAddressString != null)
                                                            {
                                                                if (_selectedIP.ipAddressString == serv.ipAddressString)
                                                                {
                                                                    foreach (ApplicationClass ap in _currentApplicationList)
                                                                    {
                                                                        if (ap.stato != null)
                                                                        {
                                                                            if (ap.stato.Equals("In Focus"))
                                                                            {
                                                                                ap.stato = " ";
                                                                                ap.StopTimer();
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    foreach (ServerClass se in _serverAppList.Keys)
                                                                    {
                                                                        if (se.ipAddressString.Equals(serv.ipAddressString))
                                                                        {
                                                                            foreach (ApplicationClass ap in _serverAppList[se])
                                                                            {
                                                                                if (ap.stato != null)
                                                                                {
                                                                                    if (ap.stato.Equals("In Focus"))
                                                                                    {
                                                                                        ap.stato = " ";
                                                                                        ap.StopTimer();
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }

                                                            appRec.stato = "In Focus";
                                                            appRec.server = serv;
                                                            App.Current.Dispatcher.Invoke((Action)delegate
                                                            {
                                                                appRec.StartTimer();
                                                            });
                                                           
                                                            //controllo se esiste già un exe corrispondente a quello della nuova app
                                                            bool foundE = false;
                                                            string exeFound = null;
                                                            foreach (string e in _applicationForExe.Keys)
                                                            {
                                                                if (e.Equals(appRec.exe))
                                                                {
                                                                    foundE = true;
                                                                    exeFound = e;
                                                                }
                                                            }
                                                            //se l'exe è già presente aggiungo solo l'app alla lista corrispondente
                                                            if (foundE == true)
                                                            {
                                                                ApplicationClass aptemp = new ApplicationClass();
                                                                aptemp.exe = appRec.exe;
                                                                aptemp.nomeFinestra = appRec.nomeFinestra;
                                                                aptemp.windowHandle = appRec.windowHandle;
                                                                aptemp.pId = appRec.pId;
                                                                aptemp.sizeIcona = appRec.sizeIcona;
                                                                aptemp.icona = appRec.icona;
                                                                aptemp.stato = appRec.stato;
                                                                aptemp.server = appRec.server;
                                                                aptemp.serverList= appRec.serverList;
                                                                aptemp.ipServerList = appRec.ipServerList;
                                                                App.Current.Dispatcher.Invoke((Action)delegate
                                                                {
                                                                    aptemp.StartTimer();
                                                                });
                                                                

                                                                App.Current.Dispatcher.Invoke((Action)delegate
                                                                {
                                                                    _applicationForExe[exeFound].Add(aptemp);
                                                                });
                                                            }
                                                            //se invece non c'era già allora creo una nuova voce
                                                            else
                                                            {
                                                                ObservableCollection<ApplicationClass> tempListApp = new ObservableCollection<ApplicationClass>();
                                                                ApplicationClass aptemp = new ApplicationClass();
                                                                aptemp.exe = appRec.exe;
                                                                aptemp.nomeFinestra = appRec.nomeFinestra;
                                                                aptemp.windowHandle = appRec.windowHandle;
                                                                aptemp.pId = appRec.pId;
                                                                aptemp.sizeIcona = appRec.sizeIcona;
                                                                aptemp.icona = appRec.icona;
                                                                aptemp.stato = appRec.stato;
                                                                aptemp.server = appRec.server;
                                                                aptemp.serverList = appRec.serverList;
                                                                aptemp.ipServerList = appRec.ipServerList;
                                                                App.Current.Dispatcher.Invoke((Action)delegate
                                                                {
                                                                    aptemp.StartTimer();
                                                                });
                                                                tempListApp.Add(aptemp);
                                                                
                                                                App.Current.Dispatcher.Invoke((Action)delegate
                                                                {
                                                                    _applicationForExe.Add(aptemp.exe, tempListApp);
                                                                });
                                                            }

                                                            _aVM.set(_applicationForExe);

                                                            //aggiungo l'app lista del server corrispondente
                                                            foreach (ServerClass se in _serverAppList.Keys)
                                                            {
 
                                                                if (se == serv)
                                                                {
                                                                    App.Current.Dispatcher.Invoke((Action)delegate
                                                                    {
                                                                        _serverAppList[serv].Add(appRec);
                                                                    });

                                                                }
                                                            }

                                                            if (_selectedIP != null && _selectedIP.ipAddressString != null)
                                                            {
                                                                if (_selectedIP.ipAddressString == serv.ipAddressString)
                                                                {
                                                                    _selectedApp = appRec;
                                                                    RaisePropertyChanged("selectedApp");
                                                                    _newApp = appRec;
                                                                    RaisePropertyChanged("newApp");
                                                                    //faccio apparire il pannello blu di notifica nuova finestra
                                                                    BluePanelVisible();
                                                                    RaisePropertyChanged("bluePanelVisibility");
                                                                }
                                                            }

                                                            appRec = new ApplicationClass();

                                                            nNewRec++;
                                                            if (nNewRec == nNew)
                                                            {
                                                                countPRec = 0;
                                                                nNewRec = 0;
                                                                newWin = false;
                                                            }
                                                            else
                                                            {
                                                                countPRec = 1;
                                                                newWin = true;
                                                            }
                                                            newRec = false;
                                                            //iconFound = true;
                                                            break;
                                                        }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (closedRec == true)
                                            {
                                                //inizio il for per ricerca finestre chiuse
                                                for (int j = i; j < (20148 - i); j++)
                                                {
                                                    if (bRec[j] != Convert.ToByte('\0') && countPRec != 0)
                                                    {
                                                        if (bRec[j] != Convert.ToByte(' ') && bRec[j] != Convert.ToByte('\n'))
                                                        {
                                                            partRec[countBRec] = bRec[j];
                                                            countBRec++;
                                                        }
                                                        //ho trovato un campo
                                                        else
                                                        {
                                                            switch (countPRec)
                                                            {
                                                                case 2:
                                                                    {
                                                                        string sp = System.Text.Encoding.Default.GetString(partRec);
                                                                        string[] s = sp.Split('\0');
                                                                        whDelete = s[0];
                                                                        countPRec++;
                                                                        countBRec = 0;
                                                                        for (int c = 0; c < 8192; c++)
                                                                        {
                                                                            partRec[c] = Convert.ToByte('\0');
                                                                        }
                                                                        break;
                                                                    }
                                                                case 3:
                                                                    {
                                                                        ApplicationClass searchedApp = new ApplicationClass();
                                                                        string sp = System.Text.Encoding.Default.GetString(partRec);
                                                                        string[] s = sp.Split('\0');
                                                                        pIdDelete = s[0];
                                                                        foreach (string e in _applicationForExe.Keys)
                                                                        {
                                                                            foreach (ApplicationClass ap in _applicationForExe[e])
                                                                            {
                                                                                if (ap.windowHandle == whDelete && ap.pId == pIdDelete)
                                                                                {
                                                                                    searchedApp = ap;
                                                                                }
                                                                            }
                                                                        }

                                                                        _applicationList.Remove(searchedApp);
                                                                        RaisePropertyChanged("ApplicationList");
                                                                        foreach (ApplicationClass ap in _currentApplicationList)
                                                                        {
                                                                            if (ap.windowHandle == whDelete && ap.pId == pIdDelete)
                                                                            {
                                                                                searchedApp = ap;
                                                                            }
                                                                        }
                                                                        if (_selectedIP != null)
                                                                        {
                                                                            if (_selectedIP.ipAddressString == serv.ipAddressString)
                                                                            {
                                                                                App.Current.Dispatcher.Invoke((Action)delegate
                                                                                {
                                                                                    _currentApplicationList.Remove(searchedApp);
                                                                                });
                                                                                RaisePropertyChanged("CurrentApplicationList");
                                                                            }
                                                                        }

                                                                        //controllo se l'app era presente nell lista delle app per exe, se c'era allora la elimino
                                                                        bool appToRemove = false;
                                                                        ApplicationClass appRemoved = new ApplicationClass();

                                                                        foreach (string e in _applicationForExe.Keys)
                                                                        {
                                                                            if (e.Equals(searchedApp.exe))
                                                                            {
                                                                                foreach (ApplicationClass appTemp in _applicationForExe[searchedApp.exe])
                                                                                {
                                                                                    if (appTemp.pId.Equals(searchedApp.pId) && appTemp.windowHandle.Equals(searchedApp.windowHandle))
                                                                                    {
                                                                                        appToRemove = true;
                                                                                        appRemoved = appTemp;
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                        if (appToRemove == true)
                                                                        {
                                                                            App.Current.Dispatcher.Invoke((Action)delegate
                                                                            {
                                                                                _applicationForExe[searchedApp.exe].Remove(appRemoved);
                                                                            });
                                                                        }

                                                                        //controllo che non fosse l'ultima app rimasta
                                                                        bool lastOne = false;

                                                                        if (searchedApp != null && searchedApp.exe != null && _applicationForExe != null)
                                                                        {
                                                                            if (_applicationForExe[searchedApp.exe].Count == 0)
                                                                            {
                                                                                lastOne = true;
                                                                            }
                                                                            if (lastOne == true)
                                                                            {
                                                                                App.Current.Dispatcher.Invoke((Action)delegate
                                                                                {
                                                                                    _applicationForExe.Remove(searchedApp.exe);
                                                                                });
                                                                            }
                                                                            _aVM.set(_applicationForExe);
                                                                        }

                                                                        //rimuovo l'app dalla lista corrispondente al server
                                                                        ApplicationClass appFound = new ApplicationClass();
                                                                        foreach (ApplicationClass ap in _serverAppList[serv])
                                                                        {
                                                                            if (ap.windowHandle == whDelete && ap.pId == pIdDelete)
                                                                            {
                                                                                appFound = ap;
                                                                            }
                                                                        }
                                                                        if (appFound != null)
                                                                        {
                                                                            App.Current.Dispatcher.Invoke((Action)delegate
                                                                            {
                                                                                _serverAppList[serv].Remove(appFound);
                                                                            });
                                                                        }

                                                                        //TODO: rimuovi anche da altre liste
                                                                        nClosedRec++;
                                                                        countPRec = 0;
                                                                        appRec = new ApplicationClass();
                                                                        countBRec = 0;
                                                                        for (int c = 0; c < 8192; c++)
                                                                        {
                                                                            partRec[c] = Convert.ToByte('\0');
                                                                        }
                                                                        for (int c = 0; c < 8192; c++)
                                                                        {
                                                                            bRec[c] = Convert.ToByte('\0');
                                                                        }
                                                                        closedRec = false;
                                                                        break;
                                                                    }
                                                            }
                                                        }
                                                    }
                                                    //è finito il messaggio
                                                    else
                                                    {
                                                        break;  //check break: dove esco? e quando esco non ritorno?
                                                    }
                                                }
                                            }
                                            //è finito il messaggio
                                            else
                                            {
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //controllo di non essere alla fine di un campo
                                        if (bRec[i] != Convert.ToByte(' ') && bRec[i] != Convert.ToByte('\n'))
                                        {
                                            partRec[countBRec] = bRec[i];
                                            countBRec++;
                                        }
                                        //ho trovato un campo
                                        else
                                        {
                                            switch (countPRec)
                                            {
                                                case 0:
                                                    {
                                                        string sp = System.Text.Encoding.Default.GetString(partRec);
                                                        string[] s = sp.Split('\0');
                                                        if (s[0].Equals("focus"))
                                                        {
                                                            focusRec = true;
                                                        }
                                                        else
                                                        {
                                                            if (s[0].Equals("closed"))
                                                            {
                                                                closedRec = true;
                                                            }
                                                            else
                                                            {
                                                                if (s[0].Equals("windows"))
                                                                {
                                                                    newRec = true;
                                                                }
                                                                else
                                                                {
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                        countPRec++;
                                                        countBRec = 0;
                                                        for (int j = 0; j < 8192; j++)
                                                        {
                                                            partRec[j] = Convert.ToByte('\0');
                                                        }
                                                        break;
                                                    }

                                                case 1:
                                                    {
                                                        string sp = System.Text.Encoding.Default.GetString(partRec);
                                                        string[] s = sp.Split('\0');
                                                        if (focusRec == true)
                                                        {
                                                            appRec.windowHandle = s[0];
                                                            if (appRec.windowHandle.Equals("nullwindow"))
                                                            {
                                                                if (_selectedIP != null)
                                                                {
                                                                    if (_selectedIP.ipAddressString == serv.ipAddressString)
                                                                    {
                                                                        foreach (ApplicationClass ap in _currentApplicationList)
                                                                        {
                                                                            if (ap.stato != null)
                                                                            {
                                                                                if (ap.stato.Equals("In Focus"))
                                                                                {
                                                                                    ap.stato = " ";
                                                                                    ap.StopTimer();
                                                                                }
                                                                            }
                                                                        }
                                                                        _appFocus = new ApplicationClass();
                                                                        _appFocus.exe = " ";
                                                                        _appFocus.stato = "In Focus";
                                                                        _appFocus.StartTimer();
                                                                        RaisePropertyChanged("appFocus");

                                                                        //setto il server dell'app
                                                                        _appFocus.server = serv;
                                                                    }
                                                                }


                                                            }
                                                            else
                                                            {
                                                                if (appRec.windowHandle.Equals("windowsoperatingsystem"))
                                                                {
                                                                    if (_selectedIP.ipAddressString == serv.ipAddressString)
                                                                    {
                                                                        foreach (ApplicationClass ap in _currentApplicationList)
                                                                        {
                                                                            if (ap.stato != null)
                                                                            {
                                                                                if (ap.stato.Equals("In Focus"))
                                                                                {
                                                                                    ap.stato = " ";
                                                                                    ap.StopTimer();
                                                                                }
                                                                            }
                                                                        }
                                                                        _appFocus = new ApplicationClass();
                                                                        _appFocus.exe = "Sistema Operativo";
                                                                        _appFocus.stato = "In Focus";
                                                                        _appFocus.StartTimer();
                                                                        RaisePropertyChanged("appFocus");
                                                                    }
                                                                    //setto il server dell'app
                                                                    _appFocus.server = serv;
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (closedRec == true)
                                                            {
                                                                nClosed = int.Parse(s[0]);
                                                            }
                                                            else
                                                            {
                                                                if (newRec == true)
                                                                {
                                                                    if (s[0] != "")
                                                                    {
                                                                        nNew = int.Parse(s[0]);
                                                                        newWin = true;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        countPRec++;
                                                        countBRec = 0;
                                                        for (int j = 0; j < 8192; j++)
                                                        {
                                                            partRec[j] = Convert.ToByte('\0');
                                                        }
                                                        break;
                                                    }
                                            }
                                        }
                                    }

                                }
                                //è finito il messaggio
                                else
                                {
                                    if (newRec == true && countPRec == 2 && newWin == true)
                                    {
                                        string sp = System.Text.Encoding.Default.GetString(partRec);
                                        string[] s = sp.Split('\0');
                                        appRec.pId = s[0];
                                        countPRec = 0;
                                        countBRec = 0;
                                        for (int c = 0; c < 8192; c++)
                                        {
                                            partRec[c] = Convert.ToByte('\0');
                                        }
                                        if (_selectedIP.ipAddressString.Equals(serv.ipAddressString))
                                        {
                                            foreach (ApplicationClass ap in _currentApplicationList)
                                            {
                                                if (ap.stato != null)
                                                {
                                                    if (ap.stato.Equals("In Focus"))
                                                    {
                                                        ap.stato = " ";
                                                        App.Current.Dispatcher.Invoke((Action)delegate
                                                        {
                                                            ap.StopTimer();
                                                        });

                                                        RaisePropertyChanged("AppFocus");
                                                    }
                                                }
                                            }
                                        }
                                        
                                        appRec.stato = "In Focus";
                                        App.Current.Dispatcher.Invoke((Action)delegate
                                        {
                                            appRec.StartTimer();
                                        });
                                        appRec.server = serv;
                                        ApplicationClass apTemp = new ApplicationClass();
                                        apTemp.exe = appRec.exe;
                                        apTemp.nomeFinestra = appRec.nomeFinestra;
                                        apTemp.windowHandle = appRec.windowHandle;
                                        apTemp.pId = appRec.pId;
                                        apTemp.sizeIcona = appRec.sizeIcona;
                                        apTemp.icona = appRec.icona;
                                        apTemp.stato = appRec.stato;
                                        apTemp.server = appRec.server;
                                        App.Current.Dispatcher.Invoke((Action)delegate
                                        {
                                            apTemp.StartTimer();
                                        });
                                       

                                        if (_selectedIP.ipAddressString == serv.ipAddressString)
                                        {
                                            _appFocus = apTemp;
                                            RaisePropertyChanged("appFocus");
                                        }
                                        //setto il server dell'app in focus
                                        _appFocus.server = appRec.server;
                                        if (_selectedIP.ipAddressString == serv.ipAddressString)
                                        {
                                            _selectedApp = apTemp;
                                            RaisePropertyChanged("selectedApp");
                                        }

                                        if (_selectedIP.ipAddressString == serv.ipAddressString)
                                        {
                                            App.Current.Dispatcher.Invoke((Action)delegate
                                            {
                                                _currentApplicationList.Insert(0, apTemp);
                                            });
                                            RaisePropertyChanged("CurrentApplicationList");
                                        }

                                        foreach (ApplicationClass ap in _serverAppList[serv])
                                        {
                                            if (ap.stato != null)
                                            {
                                                if (ap.stato.Equals("In Focus"))
                                                {
                                                    ap.StopTimer();
                                                    ap.stato = " ";
                                                }
                                            }
                                        }
                                        App.Current.Dispatcher.Invoke((Action)delegate
                                        {
                                            _serverAppList[serv].Add(apTemp);
                                        });


                                        appRec = new ApplicationClass();
                                        focusRec = true;

                                        nNewRec++;
                                        if (nNewRec == nNew)
                                        {
                                            countPRec = 0;
                                            nNewRec = 0;
                                            newWin = false;
                                        }
                                        else
                                        {
                                            countPRec = 1;
                                            newWin = true;
                                            newRec = false;
                                        }
                                        countPRec = 0;
                                        newWin = false;
                                        newRec = false;
                                        //iconFound = true;
                                    }
                                    else
                                    {
                                        string sp = System.Text.Encoding.Default.GetString(partRec);
                                        string[] s = sp.Split('\0');
                                        if (focusRec == true)
                                        {
                                            appRec.pId = s[0];
                                        }
                                        else
                                        {
                                            if (newRec == true)
                                            {
                                                if (!s[0].Equals(""))
                                                {
                                                    string[] nums = Regex.Split(s[0], @"\D+");
                                                    if (nums[0] != "")
                                                    {
                                                        nNew = int.Parse(nums[0]);
                                                    }
                                                    else
                                                    {
                                                        if (nums[1] != "")
                                                        {
                                                            nNew = int.Parse(nums[1]);
                                                        }

                                                    }
                                                    //nNew = int.Parse(s[0]);
                                                    newWin = true;
                                                }
                                            }
                                        }
                                        countPRec = 0;
                                        countBRec = 0;
                                        for (int j = 0; j < 8192; j++)
                                        {
                                            partRec[j] = Convert.ToByte('\0');
                                        }
                                    }
                                    break;
                                }

                            }
                            if (focusRec == true)
                            {
                                if (selectedIp != null)
                                {
                                    if (_selectedIP.ipAddressString == serv.ipAddressString)
                                    {

                                        foreach (ApplicationClass ap in _currentApplicationList)
                                        {
                                            if (ap.stato != null)
                                            {
                                                if (ap.stato.Equals("In Focus"))
                                                {
                                                    ap.stato = " ";
                                                    ap.StopTimer();
                                                    RaisePropertyChanged("AppFocus");
                                                }
                                            }
                                        }
                                    }
                                }
                                foreach (ApplicationClass ap in _serverAppList[serv])
                                {
                                    if (ap.stato != null)
                                    {
                                        if (ap.stato.Equals("In Focus"))
                                        {
                                            ap.StopTimer();
                                            ap.stato = " ";
                                        }
                                    }
                                }
                                foreach (ApplicationClass ap in _applicationList)
                                {
                                    if (ap.windowHandle == appRec.windowHandle && ap.pId == appRec.pId)
                                    {
                                        ap.stato = "In Focus";
                                        //ap.StartTimer();
                                        RaisePropertyChanged("Stato");
                                    }
                                }
                                if (_selectedIP != null && serv != null)
                                {
                                    if (_selectedIP.ipAddressString == serv.ipAddressString)
                                    {
                                        foreach (ApplicationClass ap in _currentApplicationList)
                                        {
                                            if (ap.windowHandle == appRec.windowHandle && ap.pId == appRec.pId)
                                            {
                                                //setto il server dell'app da aggiungere
                                                ap.server = serv;
                                                ap.stato = "In Focus";
                                                App.Current.Dispatcher.Invoke((Action)delegate
                                                {
                                                    ap.StartTimer();
                                                });
                                                RaisePropertyChanged("Stato");
                                                if (_selectedIP.ipAddressString == serv.ipAddressString)
                                                {
                                                    _appFocus = ap;
                                                    RaisePropertyChanged("AppFocus");
                                                    _selectedApp = _appFocus;
                                                    RaisePropertyChanged("selectedApp");
                                                }
                                            }
                                        }
                                        RaisePropertyChanged("CurrentApplicationList");
                                        RedPanelVisible();
                                    }
                                }
                                foreach (Socket s in _applicationForSocket.Keys)
                                {
                                    foreach (ApplicationClass ap in _applicationForSocket[s])
                                    {
                                        if (ap.windowHandle == appRec.windowHandle && ap.pId == appRec.pId)
                                        {
                                            ap.stato = "In Focus";
                                            RaisePropertyChanged("Stato");
                                        }
                                    }
                                }
                                foreach (ServerClass s in _serverAppList.Keys)
                                {
                                    foreach (ApplicationClass ap in _serverAppList[s])
                                    {
                                        if (ap.windowHandle == appRec.windowHandle && ap.pId == appRec.pId)
                                        {
                                            ap.stato = "In Focus";
                                            App.Current.Dispatcher.Invoke((Action)delegate
                                            {
                                                ap.StartTimer();
                                            });
                                            RaisePropertyChanged("Stato");

                                        }
                                    }
                                }
                                focusRec = false;

                            }
                            }
                            //non è arrivato nulla, provo a riascoltare
                            else
                            {
                                continue;
                            }
                        }
                    }
                    else
                    {
                        WindowServerInterrupt wInt = new WindowServerInterrupt();
                        var vm = new SecondWindowViewModel();
                        vm.ClosingRequest += wInt.Close;
                        wInt.DataContext = vm;
                        wInt.Show();

                        foreach (IpAddressClass ipac in _ipList)
                        {
                            if (ipac.ipAddressString.Equals(serv.ipAddressString))
                            {
                                _selectedIP = ipac;
                                RaisePropertyChanged("selectedIP");
                                _currentServer = serv;
                                RaisePropertyChanged("currentServer");
                                _keepListeningList[serv.socket] = false;

                                if (_selectedIP != null)
                                {
                                    try
                                    {
                                        //ricerco il socket legato all'ip
                                        IPAddress _ipa = IPAddress.Parse(_selectedIP.ipAddressString);
                                        //Socket so = _socketList[_ipa];

                                        Socket so = _currentServer.socket;

                                        if (_selectedIP.ipAddressString.Equals(_currentServer.ipAddressString))
                                        {
                                            _keepListeningList[so] = false; //così smette di ascoltare (check!)

                                            //invio il messaggio di chiusura al server
                                            byte[] closingMessage = new byte[8192];
                                            for (int j = 0; j < 8192; j++)
                                            {
                                                closingMessage[j] = Convert.ToByte('0');
                                            }
                                            byte[] closeTemp = System.Text.Encoding.Default.GetBytes("stop");
                                            for (int j = 0; j < (closeTemp.Length); j++)
                                            {
                                                closingMessage[j] = closeTemp[j];
                                            }
                                            string prova = System.Text.Encoding.Default.GetString(closingMessage);  //elimina
                                            so.Send(closingMessage);

                                            ServerClass serverFound = new ServerClass();
                                            bool foundS = false;
                                            foreach (ServerClass se in _serverAppList.Keys)
                                            {
                                                if (se.socket == so)
                                                {
                                                    foundS = true;
                                                    serverFound = se;
                                                }
                                            }
                                            if (foundS == true)
                                            {
                                                App.Current.Dispatcher.Invoke((Action)delegate
                                                {
                                                    _serverAppList.Remove(serverFound);
                                                });

                                            }

                                            //elimino dalla lista delle app per exe quelle che appartengono a questo server
                                            ApplicationClass appFound = new ApplicationClass();
                                            List<ApplicationClass> listAppToRemove = new List<ApplicationClass>();
                                            List<string> exeToRemove = new List<string>();
                                            bool found = false;
                                            bool empty = false;
                                            foreach (string e in _applicationForExe.Keys)
                                            {
                                                foreach (ApplicationClass ap in _applicationForExe[e])
                                                {
                                                    if (ap.server == serverFound)
                                                    {
                                                        found = true;
                                                        App.Current.Dispatcher.Invoke((Action)delegate
                                                        {
                                                            listAppToRemove.Add(ap);
                                                        });

                                                    }
                                                }
                                                if (found == true)
                                                {
                                                    foreach (ApplicationClass appToRem in listAppToRemove)
                                                    {
                                                        App.Current.Dispatcher.Invoke((Action)delegate
                                                        {
                                                            _applicationForExe[e].Remove(appToRem);
                                                        });
                                                    }
                                                }
                                                if (_applicationForExe[e].Count == 0)
                                                {
                                                    exeToRemove.Add(e);
                                                    empty = true;
                                                }
                                            }
                                            if (empty == true)
                                            {
                                                foreach (string eToR in exeToRemove)
                                                {
                                                    App.Current.Dispatcher.Invoke((Action)delegate
                                                    {
                                                        _applicationForExe.Remove(eToR);
                                                    });
                                                }
                                            }
                                            _aVM.Disconnect(_currentServer);

                                            //setto la nuova app selezionata e l'app focus come nuove
                                            _selectedApp = new ApplicationClass();
                                            RaisePropertyChanged("SelectedApp");
                                            _appFocus = new ApplicationClass();
                                            RaisePropertyChanged("AppFocus");

                                            //rendo i due pannelli non più visibili
                                            redPanelVisibility = false;
                                            bluePanelVisibility = false;

                                            //ricerco il socket corrispondente all'ip selezionato e lo disconnetto
                                            _socketList[_ipa].Shutdown(SocketShutdown.Receive);    //giusto both???
                                            _socketList[_ipa].Close();

                                            int i;
                                            //elimino l'indirizzo ip da _ipList
                                            for (i = 0; i < _ipList.Count(); i++)
                                            {
                                                if (_ipList[i].ipAddressString.Equals(_selectedIP.ipAddressString))
                                                {
                                                    App.Current.Dispatcher.Invoke((Action)delegate
                                                    {
                                                        _ipList.Remove(_ipList[i]);
                                                    });

                                                    break;
                                                }
                                            }
                                            //elimino il socket da _socketList
                                            _socketList.Remove(_ipa);
                                            _applicationForSocket.Remove(_socket);



                                            //cambio la lista delle applicazioni(a meno che non fosse l'unico connesso)
                                            if (_selectedIP != null)
                                            {
                                                App.Current.Dispatcher.Invoke((Action)delegate
                                                {
                                                    _currentApplicationList = _applicationForSocket[_socketList[IPAddress.Parse(_selectedIP.ipAddressString)]];
                                                });

                                                RaisePropertyChanged("CurrentApplicationList");
                                            }
                                            else
                                            {
                                                App.Current.Dispatcher.Invoke((Action)delegate
                                                {
                                                    _currentApplicationList = new ObservableCollection<ApplicationClass>();
                                                });

                                                RaisePropertyChanged("CurrentApplicationList");
                                            }
                                            //aspetto la terminazione del thread associato e lo elimino dalla lista
                                            var thisThread = _threadList[so];
                                            App.Current.Dispatcher.Invoke((Action)delegate
                                            {
                                                _threadList.Remove(so);
                                            });
                                            App.Current.Dispatcher.Invoke((Action)delegate
                                            {
                                                _keepListeningList.Remove(so);
                                            });

                                            //rimuovo il server dalla lista dei server
                                            _serverList.Remove(_currentServer);

                                            App.Current.Dispatcher.Invoke((Action)delegate
                                            {
                                                //lancia sempre un'eccezione che serve per eventualmente chiamare Thread.ResetAbort() ed impedire l'abort
                                                thisThread.Abort();
                                            });
                                            return;
                                        }

                                    }
                                    catch (Exception e)
                                    {
                                        //raccolgo l'eccezione dell'abort
                                        Console.WriteLine("Exception : {0}", e.ToString());
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
                return;
        }

        /*........................................................................................................................................................................*/

        //Disconnette il client dal server 
        private void Disconnect()
        {
            if (_selectedIP != null)
            {
                try
                {
                    //ricerco il socket legato all'ip
                    IPAddress _ipa = IPAddress.Parse(_selectedIP.ipAddressString);
                    Socket so = _currentServer.socket;

                    if (_selectedIP.ipAddressString.Equals(_currentServer.ipAddressString))
                    {
                        _keepListeningList[so] = false; //così smette di ascoltare (check!)

                        //invio il messaggio di chiusura al server
                        byte[] closingMessage = new byte[8192];
                        for (int j = 0; j < 8192; j++)
                        {
                            closingMessage[j] = Convert.ToByte('0');
                        }
                        byte[] closeTemp = System.Text.Encoding.Default.GetBytes("stop");
                        for (int j = 0; j < (closeTemp.Length); j++)
                        {
                            closingMessage[j] = closeTemp[j];
                        }
                        string prova = System.Text.Encoding.Default.GetString(closingMessage);  //elimina
                        so.Send(closingMessage);

                        ServerClass serverFound = new ServerClass();
                        bool foundS = false;
                        foreach (ServerClass se in _serverAppList.Keys)
                        {
                            if (se.socket == so)
                            {
                                foundS = true;
                                serverFound = se;
                            }
                        }
                        if (foundS == true)
                        {
                            _serverAppList.Remove(serverFound);
                        }

                        //elimino dalla lista delle app per exe quelle che appartengono a questo server
                        ApplicationClass appFound = new ApplicationClass();
                        List<ApplicationClass> listAppToRemove = new List<ApplicationClass>();
                        List<string> exeToRemove = new List<string>();
                        bool found = false;
                        bool empty = false;
                        string emptyExe = null;
                        foreach (string e in _applicationForExe.Keys)
                        {
                            foreach (ApplicationClass ap in _applicationForExe[e])
                            {
                                if (ap.server == serverFound)
                                {
                                    found = true;
                                    listAppToRemove.Add(ap);
                                }
                            }
                            if (found == true)
                            {
                                foreach (ApplicationClass appToRem in listAppToRemove)
                                {
                                    App.Current.Dispatcher.Invoke((Action)delegate
                                    {
                                        _applicationForExe[e].Remove(appToRem);
                                    });
                                }
                            }
                            if (_applicationForExe[e].Count == 0)
                            {
                                exeToRemove.Add(e);
                                empty = true;
                            }
                        }
                        if (empty == true)
                        {
                            foreach (string eToR in exeToRemove)
                            {
                                App.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    _applicationForExe.Remove(eToR);
                                });
                            }
                        }
                        //_aVM.set(_applicationForExe);
                        _aVM.Disconnect(_currentServer);

                        //rimuovo il server dalla lista dei server
                        _serverList.Remove(_currentServer);

                        //setto la nuova app selezionata e l'app focus come nuove
                        _selectedApp = new ApplicationClass();
                        RaisePropertyChanged("SelectedApp");
                        _appFocus = new ApplicationClass();
                        RaisePropertyChanged("AppFocus");

                        //rendo i due pannelli non più visibili
                        redPanelVisibility = false;
                        bluePanelVisibility = false;

                        //aspetto la terminazione del thread associato e lo elimino dalla lista
                        _threadList[so].Join();
                        _threadList.Remove(so);
                        _keepListeningList.Remove(so);

                        //ricerco il socket corrispondente all'ip selezionato e lo disconnetto
                        _socketList[_ipa].Shutdown(SocketShutdown.Receive);    //giusto both???
                        _socketList[_ipa].Close();

                        int i;
                        //elimino l'indirizzo ip da _ipList
                        for (i = 0; i < _ipList.Count(); i++)
                        {
                            if (_ipList[i].ipAddressString.Equals(_selectedIP.ipAddressString))
                            {
                                _ipList.Remove(_ipList[i]);
                                break;
                            }
                        }
                        //elimino il socket da _socketList
                        _socketList.Remove(_ipa);
                        _applicationForSocket.Remove(_socket);

                       
                        
                        

                        //cambio la lista delle applicazioni(a meno che non fosse l'unico connesso)
                        if (_selectedIP != null)
                        {
                            _currentApplicationList = _applicationForSocket[_socketList[IPAddress.Parse(_selectedIP.ipAddressString)]];
                            RaisePropertyChanged("CurrentApplicationList");
                        }
                        else
                        {
                            _currentApplicationList = new ObservableCollection<ApplicationClass>();
                            RaisePropertyChanged("CurrentApplicationList");
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception : {0}", e.ToString());
                }
            }
            return;
        }

        /*........................................................................................................................................................................*/

        //Connette il client al server
        private void Connect()
        {

            if (_ip != "")
            {
                //TODO: controlla che l'indirizzo sia inserito nel formato corretto!

                if (Regex.IsMatch(_ip, @"^[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}$"))
                {

                    //controlla che l'indirizzo non sia già presente
                    int q = 0;
                    for (int i = 0; i < _ipList.Count(); i++)
                    {
                        if (_ipList[i].ipAddressString.Equals(_ip))
                        {
                            q++;
                        }
                    }

                    if (q == 0)
                    {
                        _ipConnectedAddress = IPAddress.Parse(_ip);  //salvo l'indirizzo IP inserito

                        //mi connetto al server
                        //stabilisco la connessione remota
                        try
                        {
                            IPEndPoint remoteEP = new IPEndPoint(_ipConnectedAddress, 27015);    //mi connetto all'ip passatto usando la porta 27015
                                                                                                 //creo un socket TCP
                            _socket = new Socket(AddressFamily.Unspecified, SocketType.Stream, ProtocolType.Tcp);   //famiglia di indirizzi non specificata
                                                                                                                    //mi connetto al server
                            try
                            {
                                //mi connetto
                                IAsyncResult result = _socket.BeginConnect(remoteEP, null, null);
                                bool success = result.AsyncWaitHandle.WaitOne(2000, true);
                                if (!_socket.Connected)
                                {
                                    _socket.Close();
                                    WindowBadConnection wbc = new WindowBadConnection();
                                    var vmm = new SecondWindowViewModel();
                                    vmm.ClosingRequest += wbc.Close;
                                    wbc.DataContext = vmm;
                                    wbc.Show();
                                    throw new ApplicationException("Failed to Connect Server!");
                                }
                                else
                                {
                                    //_ipAddressList.Insert(0, _ipConnectedAddress);     //lo aggiungo alla lista di indirizzi 
                                    _ipList.Insert(0, new IpAddressClass() { ipAddressString = _ip });
                                    _socketList.Add(_ipConnectedAddress, _socket);
                                    _selectedIP = _ipList[0];
                                    RaisePropertyChanged("selectedIp");
                                    Console.WriteLine("Socket connected to: {0}", _socket.RemoteEndPoint.ToString());

                                    //aggiungo il server e il socket alle liste 
                                    ServerClass s = new ServerClass();
                                    s.socket = _socket;
                                    s.ipAddressString = _ip;
                                    _currentServer = s;
                                    _serverList.Add(s);

                                    //ricevo il primo messaggio
                                    int bytesRec = _socket.Receive(bytes);
                                    string prova = System.Text.Encoding.Default.GetString(bytes);

                                    byte[] okMessage = new byte[8192];
                                    for (int j = 0; j < 8192; j++)
                                    {
                                        okMessage[j] = Convert.ToByte('\0');
                                    }

                                    string toSend = "ok\n";

                                    byte[] okMessageTemp = System.Text.Encoding.Default.GetBytes(toSend);
                                    prova = System.Text.Encoding.Default.GetString(okMessageTemp);  //elimina
                                    for (int j = 0; j < (okMessageTemp.Length); j++)
                                    {
                                        okMessage[j] = okMessageTemp[j];
                                    }
                                    prova = System.Text.Encoding.Default.GetString(okMessage);  //elimina
                                    _socket.Send(okMessageTemp);

                                    //decodifico
                                    byte[] firstBytes = new byte[8192];
                                    int countParts = 0;
                                    int countBytes = 0;
                                    byte[] partial = new byte[8192];
                                    bool iconFound = false;
                                    int nAppArrivate = 0;
                                    ApplicationClass app = new ApplicationClass();

                                    //decodifico il primo messaggio
                                    for (int i = 0; i < 8192; i++)
                                    {
                                        if (bytes[i] != Convert.ToByte('\0'))    //TODO: controlla! fine messaggio = '\0'?
                                        {
                                            firstBytes[i] = bytes[i];
                                        }
                                        // è finito il messaggio! ora ho il numero di finestre
                                        else
                                        {
                                            string firstMessage = System.Text.Encoding.Default.GetString(firstBytes);
                                            string[] nums = Regex.Split(firstMessage, @"\D+");
                                            _nWindows = int.Parse(nums[1]);
                                            break;
                                        }
                                    }

                                    //resetto la lista di app correnti
                                    _currentApplicationList = new ObservableCollection<ApplicationClass>();

                                    //ricerco le info sulle finestre fino a quando non sono arrivate tutte
                                    while (nAppArrivate < _nWindows)
                                    {
                                        _socket.Receive(bytes);
                                        prova = System.Text.Encoding.Default.GetString(bytes);

                                        byte[] okMess = new byte[8192];
                                        for (int j = 0; j < 8192; j++)
                                        {
                                            okMessage[j] = Convert.ToByte('\0');
                                        }

                                        string toSendMess = "ok\n";

                                        byte[] okMessTemp = System.Text.Encoding.Default.GetBytes(toSendMess);
                                        prova = System.Text.Encoding.Default.GetString(okMessageTemp);  //elimina
                                        for (int j = 0; j < (okMessageTemp.Length); j++)
                                        {
                                            okMessage[j] = okMessageTemp[j];
                                        }
                                        prova = System.Text.Encoding.Default.GetString(okMessage);  //elimina
                                        _socket.Send(okMessageTemp);

                                        for (int i = 0; i < 8192; i++)
                                        {
                                            if (iconFound == false)
                                            {
                                                if (bytes[i] != '\n' && bytes[i] != '\0')
                                                {
                                                    partial[countBytes] = bytes[i];
                                                    countBytes++;
                                                }
                                                //ho trovato un campo!
                                                else
                                                {
                                                    switch (countParts)
                                                    {
                                                        //ho trovato il nome!
                                                        case 0:
                                                            {
                                                                app = new ApplicationClass();
                                                                app.exe = System.Text.Encoding.Default.GetString(partial);
                                                                countParts++;
                                                                countBytes = 0;
                                                                for (int j = 0; j < 8192; j++)
                                                                {
                                                                    partial[j] = Convert.ToByte('\n');
                                                                }
                                                                break;
                                                            }
                                                        //ho trovato il nome
                                                        case 1:
                                                            {
                                                                app.nomeFinestra = System.Text.Encoding.Default.GetString(partial);
                                                                countParts++;
                                                                countBytes = 0;
                                                                for (int j = 0; j < 8192; j++)
                                                                {
                                                                    partial[j] = Convert.ToByte('\n');
                                                                }
                                                                break;
                                                            }
                                                        //ho trovato la window handle
                                                        case 2:
                                                            {
                                                                app.windowHandle = System.Text.Encoding.Default.GetString(partial);
                                                                countParts++;
                                                                countBytes = 0;
                                                                for (int j = 0; j < 8192; j++)
                                                                {
                                                                    partial[j] = Convert.ToByte('\n');
                                                                }
                                                                break;
                                                            }
                                                        //ho trovato il pid
                                                        case 3:
                                                            {
                                                                app.pId = System.Text.Encoding.Default.GetString(partial);
                                                                countParts++;
                                                                countBytes = 0;
                                                                for (int j = 0; j < 8192; j++)
                                                                {
                                                                    partial[j] = Convert.ToByte('\n');
                                                                }
                                                                break;
                                                            }
                                                        //ho trovato la size dell'icona
                                                        case 4:
                                                            {
                                                                app.sizeIcona = int.Parse(System.Text.Encoding.Default.GetString(partial));
                                                                countParts = 0;
                                                                countBytes = 0;
                                                                for (int j = 0; j < 8192; j++)
                                                                {
                                                                    partial[j] = Convert.ToByte('\n');
                                                                }
                                                                if (app.sizeIcona > 0)
                                                                {
                                                                    for (int j = i + 1, c = 0; j < app.sizeIcona; j++, c++)
                                                                    {
                                                                        partial[c] = bytes[j];
                                                                    }
                                                                    var image = new BitmapImage();
                                                                    using (var mem = new MemoryStream(partial))
                                                                    {
                                                                        mem.Position = 0;
                                                                        image.BeginInit();
                                                                        image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                                                                        image.CacheOption = BitmapCacheOption.OnLoad;
                                                                        image.UriSource = null;
                                                                        image.StreamSource = mem;
                                                                        image.EndInit();
                                                                    }
                                                                    image.Freeze();

                                                                    app.icona = image;
                                                                }
                                                                //TODO: add icona se manca
                                                                _applicationList.Add(app);

                                                                nAppArrivate++;
                                                                iconFound = true;
                                                                for (int j = 0; j < 8192; j++)
                                                                {
                                                                    partial[j] = Convert.ToByte('\n');
                                                                }

                                                                break;
                                                            }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                iconFound = false;
                                                break;
                                            }
                                        }
                                    }

                                    //salvo le applicazioni 
                                    bool cApp = true;
                                    foreach (ApplicationClass ap in _applicationList)
                                    {
                                        string[] part;
                                        ApplicationClass ac = new ApplicationClass();
                                        if (cApp == true)
                                        {
                                            part = ap.exe.Split('\0');
                                            cApp = false;
                                        }
                                        else
                                        {
                                            part = ap.exe.Split('\n');
                                        }
                                        ac.exe = part[0];
                                        part = ap.nomeFinestra.Split('\n');
                                        ac.nomeFinestra = part[0];
                                        if (ac.exe.Equals(""))
                                        {
                                            ac.exe = "App Senza Exe";
                                            //ac.nomeFinestra = "App Senza Nome";
                                        }
                                        part = ap.windowHandle.Split('\n');
                                        ac.windowHandle = part[0];
                                        part = ap.pId.Split('\n');
                                        ac.pId = part[0];
                                        ac.server = s;
                                        ac.serverList.Add(s);
                                        ac.icona = ap.icona;
                                        ac.ipServerList.Add(new IpAddressClass() { ipAddressString = _ip });
                                        _currentApplicationList.Add(ac);
                                        RaisePropertyChanged("CurrentApplicationList");

                                        //controllo se esiste già un exe uguale a quello della nuova app inserita
                                        bool foundExe = false;
                                        foreach (string e in _applicationForExe.Keys)
                                        {
                                            if (e.Equals(ac.exe))
                                            {
                                                foundExe = true;
                                            }
                                        }
                                        //se l'ho trovata allora aggiungo solo l'app alla lista delle app per quell'exe
                                        if (foundExe == true)
                                        {
                                            App.Current.Dispatcher.Invoke((Action)delegate
                                            {
                                                _applicationForExe[ac.exe].Add(ac);
                                            });
                                        }
                                        //se non l'ho trovata la aggiungo
                                        else
                                        {
                                            ObservableCollection<ApplicationClass> tempList = new ObservableCollection<ApplicationClass>();
                                            tempList.Add(ac);
                                            App.Current.Dispatcher.Invoke((Action)delegate
                                            {
                                                _applicationForExe.Add(ac.exe, tempList);
                                            });
                                        }
                                        _aVM.set(_applicationForExe);
                                    }

                                    _applicationForSocket.Add(_socket, _currentApplicationList);
                                    //faccio partire il timer del server
                                    s.StartTimer();
                                    foreach (ApplicationClass ap in _currentApplicationList)
                                    {
                                        ap.server = s;
                                    }
                                    
                                    //aggiungo le app corrispondenti al server
                                    _serverAppList.Add(s, new ObservableCollection<ApplicationClass>());
                                    foreach (ApplicationClass ap in _currentApplicationList)
                                    {
                                        _serverAppList[s].Add(ap);
                                    }
                                    //_serverAppList.Add(s, tempList);


                                    _appFocus = new ApplicationClass();
                                    ChangeServer();

                                    //lancio il thread che si mette in ascolto per eventuali notifiche
                                    _keepListeningList[s.socket] = true;
                                    Thread t = new Thread(() => Listen(s));
                                    t.SetApartmentState(ApartmentState.STA);
                                    t.Start();
                                    _threadList.Add(_socket, t);

                                    _applicationList = new ObservableCollection<ApplicationClass>();
                                }
                            }
                            catch (ArgumentNullException ane)
                            {
                                Console.WriteLine("Argument Null Exception : {0}", ane.ToString());
                            }
                            catch (SocketException se)
                            {
                                Console.WriteLine("Socket Exception : {0}", se.ToString());
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Exception : {0}", e.ToString());
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Exception : {0}", e.ToString());
                        }
                    }
                    else
                    {
                        //se l'indirizzo è gia stato inserito mostro una finestra di errore
                        WindowIndirizzoDuplicato ww = new WindowIndirizzoDuplicato();
                        var vmm = new SecondWindowViewModel();
                        vmm.ClosingRequest += ww.Close;
                        ww.DataContext = vmm;
                        ww.Show();
                    }
                }
                else
                {
                    //se l'indirizzo è stato inserito in un formato sbagliato mostro una finestra di errore
                    WindowIndirizzoSbagliato ww = new WindowIndirizzoSbagliato();
                    var vmm = new SecondWindowViewModel();
                    vmm.ClosingRequest += ww.Close;
                    ww.DataContext = vmm;
                    ww.Show();
                }
            }
            else
            {
                WindowIndirizzoSbagliato w = new WindowIndirizzoSbagliato();
                var vm = new SecondWindowViewModel();
                vm.ClosingRequest += w.Close;
                w.DataContext = vm;
                w.Show();
            }
            ip = string.Empty;
            return;
        }
    }


}
