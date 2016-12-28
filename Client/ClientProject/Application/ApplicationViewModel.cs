using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Drawing;

namespace ClientProject
{
    public class ApplicationViewModel : ObservableObject, IPageViewModel
    {
        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        //VARIABILI PRIVATE
        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        private string _stringCommand;
        private BitmapImage _iconToShow;
        private List<string> _keyList = new List<string>();
        private Dictionary<string, ObservableCollection<ServerClass>> _serverList = new Dictionary<string, ObservableCollection<ServerClass>>();
        private ObservableCollection<ApplicationClass> _selectedApp = new ObservableCollection<ApplicationClass>();
        private Dictionary<string, ObservableCollection<ApplicationClass>> _appList = new Dictionary<string, ObservableCollection<ApplicationClass>>();
        private ObservableCollection<ServerClass> _selectedServerList = new ObservableCollection<ServerClass>();
        private ObservableCollection<Exec> _exeList = new ObservableCollection<Exec>();
        private Exec _selectedExe = new Exec();
        private ICommand _sendCommand;
        private object thisLock = new object();


        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        //VARIABILI PUBBLICHE
        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/


        public string Name
        {
            get { return "Applicazioni"; }
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

        public BitmapImage iconToShow
        {
            get { return _iconToShow; }
            set
            {
                if (_iconToShow != value)
                {
                    _iconToShow = value;
                    OnPropertyChanged("iconToShow");
                }
            }
        }

        public ObservableCollection<ApplicationClass> selectedApp
        {
            get { return _selectedApp; }
            set
            {
                    _selectedApp = value;
                    OnPropertyChanged("selectedApp");
            }
        }

        public Exec selectedExe
        {
            get { return _selectedExe; }
            set
            {
                _selectedExe = value;
                OnPropertyChanged("selectedExe");
            }
        }

        public Dictionary<string, ObservableCollection<ApplicationClass>> appList
        {
            get { return _appList; }
            set
            {
                if (_appList != value)
                {
                    _appList = value;
                    OnPropertyChanged("appList");
                }
            }
        }

        public ObservableCollection<Exec> exeList
        {
            get { return _exeList; }
            set
            {
                if (_exeList != value)
                {
                    _exeList = value;
                    OnPropertyChanged("exeList");
                }
            }
        }

        public Dictionary<string, ObservableCollection<ServerClass>> serverList
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

        public ObservableCollection<ServerClass> selectedServerList
        {
            get { return _selectedServerList; }
            set
            {
                if (_selectedServerList != value)
                {
                    _selectedServerList = value;
                    OnPropertyChanged("selectedServerList");
                }
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

        
        public void Disconnect(ServerClass se)
        {
            List<string> exeToRemove = new List<string>();
            foreach(string e in _appList.Keys)
            {
                List<ApplicationClass> appToRemove = new List<ApplicationClass>();
                foreach(ApplicationClass ap in _appList[e])
                {
                    if (ap.server.ipAddressString.Equals(se.ipAddressString))
                    {
                        appToRemove.Add(ap);
                        RaisePropertyChanged("appList");
                    }
                }
                foreach(ApplicationClass ac in appToRemove)
                {
                    _appList[e].Remove(ac);
                }
                if (_appList[e].Count == 0)
                {
                    exeToRemove.Add(e);
                }
            }
            foreach(string e in exeToRemove)
            {
                _appList.Remove(e);
                RaisePropertyChanged("appList");
            }

            _exeList = new ObservableCollection<Exec>();
            foreach (string e in _appList.Keys)
            {
                Exec ex = new Exec();
                ex.icona = _appList[e][0].icona;
                ex.exe = _appList[e][0].exe;
                if (!_exeList.Contains(ex))
                {
                    _exeList.Add(ex);
                    RaisePropertyChanged("exeList");
                }
            }

            if (_exeList.Count != 0)
            {
                _selectedExe = _exeList.Last<Exec>();
            }

            _serverList = new Dictionary<string, ObservableCollection<ServerClass>>();

            foreach (string e in _appList.Keys)
            {
                bool present = false;
                foreach (string stre in _serverList.Keys)
                {
                    if (e.Equals(stre))
                    {
                        present = true;
                    }
                }
                //c'era già l'exe
                if (present == true)
                {
                    foreach (ApplicationClass ap in _appList[e])
                    {
                        if (!_serverList[e].Contains<ServerClass>(ap.server))
                        {
                            _serverList[e].Add(ap.server);
                            RaisePropertyChanged("serverList");
                        }
                    }
                }
                //non c'era ancora questo exe
                else
                {
                    ObservableCollection<ServerClass> tempServs = new ObservableCollection<ServerClass>();
                    foreach (ApplicationClass ap in _appList[e])
                    {
                        if (!tempServs.Contains(ap.server))
                        {
                            tempServs.Add(ap.server);
                        }
                    }
                    _serverList.Add(e, tempServs);
                    RaisePropertyChanged("serverList");
                }
            }

            if (_serverList.Count == 0)
            {
                _selectedServerList = null;
                RaisePropertyChanged("selectedServerList");
            }

            return;
        }


        //set current list
        public void set(Dictionary<string, ObservableCollection<ApplicationClass>> l)
        {
            lock (thisLock)
            {
                _appList = l;
                RaisePropertyChanged("appList");

                _exeList = new ObservableCollection<Exec>();
                foreach (string e in _appList.Keys)
                {
                    Exec ex = new Exec();
                    ex.icona = _appList[e][0].icona;
                    ex.exe = _appList[e][0].exe;
                    if (!_exeList.Contains(ex))
                    {
                        _exeList.Add(ex);
                        RaisePropertyChanged("exeList");
                    }
                }

                _selectedExe = _exeList.Last<Exec>();

                _serverList = new Dictionary<string, ObservableCollection<ServerClass>>();
                Dictionary<string, List<ServerClass>> _listToAdd = new Dictionary<string, List<ServerClass>>();
                foreach (string e in _appList.Keys)
                {
                    bool present = false;
                    foreach (string se in _serverList.Keys)
                    {
                        if (e.Equals(se))
                        {
                            present = true;
                        }
                    }
                    //c'era già l'exe
                    if (present == true)
                    {
                        foreach (ApplicationClass ap in _appList[e])
                        {
                            if (!_serverList[e].Contains<ServerClass>(ap.server))
                            {

                                _serverList[e].Add(ap.server);
                                RaisePropertyChanged("serverList");
                            }
                        }
                    }
                    //non c'era ancora questo exe
                    else
                    {
                        ObservableCollection<ServerClass> tempServs = new ObservableCollection<ServerClass>();
                        foreach (ApplicationClass ap in _appList[e])
                        {
                            if (!tempServs.Contains(ap.server))
                            {
                                tempServs.Add(ap.server);
                            }
                        }
                        _serverList.Add(e, tempServs);
                        RaisePropertyChanged("serverList");
                    }
                }

                if (_serverList.Count == 0)
                {
                    _selectedServerList = null;
                    RaisePropertyChanged("selectedServerList");
                }
            }
            
            return;
        }


        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/


        //get pressed keys
        public void getKey(KeyEventArgs e)
        {

            Key k = e.Key;
            string sk = k.ToString();
            if (_keyList.Count == 0)
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



        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        
        public void getSelected()
        {
            //string exe = _selectedApp.Key;
            if (_selectedExe != null)
            {
                string exe = _selectedExe.exe;
                _selectedServerList = _serverList[exe];
                RaisePropertyChanged("selectedServerList");
            }
            else
            {
                _selectedServerList = new ObservableCollection<ServerClass>();
                RaisePropertyChanged("selectedServerList");
            }
           
            return;
        }


        /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/



        //send pressed keys to server      
        private void Send()
        {
            if(_selectedExe!=null && _selectedExe.exe != null)
            {
                _selectedApp = _appList[_selectedExe.exe];
            }

            if (_keyList.Count != 0)
            {
                if (! _selectedApp.Equals(new ObservableCollection<ApplicationClass>()))
                {
                    int nKeys = _keyList.Count;
                    byte[] message = new byte[8192];
                    for (int j = 0; j < 8192; j++)
                    {
                        message[j] = Convert.ToByte('\0');
                    }
                    
                    List<string> listTemp = new List<string>();
                    foreach (ApplicationClass ap in _selectedApp)
                    {
                        if (!listTemp.Contains(ap.server.ipAddressString))
                        {
                            int nCommand = 0;
                            foreach (ApplicationClass app in _selectedApp)
                            {
                                if (app.server.ipAddressString.Equals(ap.server.ipAddressString))
                                {
                                    nCommand++;
                                }
                            }
                            string toSend = "ncommands " + nCommand + " ";
                            foreach (ApplicationClass app in _selectedApp)
                            {
                                if (app.server.ipAddressString.Equals(ap.server.ipAddressString))
                                {
                                    toSend += "command" + " " + app.windowHandle + " " + app.pId + " " + nKeys + " ";
                                    for (int c = 0; c < _keyList.Count; c++)
                                    {
                                        toSend += _keyList[c] + " ";
                                    }
                                }
                            }
                            byte[] messageTemp = System.Text.Encoding.Default.GetBytes(toSend);
                            string prova = System.Text.Encoding.Default.GetString(messageTemp);  //elimina
                            for (int j = 0; j < (messageTemp.Length); j++)
                            {
                                message[j] = messageTemp[j];
                            }
                            prova = System.Text.Encoding.Default.GetString(message);  //elimina

                            ap.server.socket.Send(message);

                            byte[] serverAnswer = new byte[8192];
                            //ap.server.socket.Receive(serverAnswer);
                            prova = System.Text.Encoding.Default.GetString(serverAnswer);  //elimina
                            string okToSend = "ok\n";
                            byte[] okMessage = System.Text.Encoding.Default.GetBytes(okToSend);
                            prova = System.Text.Encoding.Default.GetString(okMessage);  //elimina
                            ap.server.socket.Send(okMessage);

                            for (int j = 0; j < 8192; j++)
                            {
                                message[j] = Convert.ToByte('\0');
                            }
                            listTemp.Add(ap.server.ipAddressString);
                        }
                    }
                }
                else
                {
                    WindowNoAppSelected ww = new WindowNoAppSelected();
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

    }
}
