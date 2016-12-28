using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Net;
using System.Net.Sockets;
using System.Drawing;

namespace ClientProject
{
    public class MainWindowViewModel : ObservableObject
    {
        private ICommand _changePageCommand;
        private IPageViewModel _currentPageViewModel;
        private ICommand _onClosingCommand;
        private ICommand _infoCommand;
        private List<IPageViewModel> _pageViewModels;
        private ServerViewModel svm;
        private ApplicationViewModel avm;
        private Color _backServer = ColorTranslator.FromHtml("#FF6A6969");
        private Color _backApplication;

        public MainWindowViewModel()
        {
            //alternationNumberServer = ColorTranslator.FromHtml("#FF6A6969");
            //alternationNumberApplication = ColorTranslator.FromHtml("#FFFFFFFF");
            alternationNumberServer = ColorTranslator.FromHtml("#FF6A6969");
            alternationNumberApplication = ColorTranslator.FromHtml("#FFFFFFFF");

            //add avaiable pages
            svm = new ServerViewModel();
            avm = new ApplicationViewModel();
            svm.avm = avm;
            PageViewModels.Add(svm);
            PageViewModels.Add(avm);

            //set starting page
            CurrentPageViewModel = PageViewModels[0];
        }

        public Color alternationNumberServer
        {
            get { return _backServer; }
            set
            {
                if (_backServer != value)
                {
                    _backServer = value;
                    RaisePropertyChanged("alternationNumberServer");
                }
            }
        }

        public Color alternationNumberApplication
        {
            get { return _backApplication; }
            set
            {
                if (_backApplication != value)
                {
                    _backApplication = value;
                    RaisePropertyChanged("alternationNumberApplication");
                }
            }
        }

        public ICommand ChangePageCommand
        {
            get
            {
                if (_changePageCommand == null)
                {
                    _changePageCommand = new RelayCommand(
                        p => ChangeViewModel((IPageViewModel)p),
                        p => p is IPageViewModel);
                }
                return _changePageCommand;
            }
        }

        public ICommand onClosingCommand
        {
            get
            {
                if (_onClosingCommand == null)
                {
                    _onClosingCommand = new RelayCommand(
                        param => onClose());
                }
                return _onClosingCommand;
            }
        }

        public ICommand InfoCommand
        {
            get
            {
                if (_infoCommand == null)
                {
                    _infoCommand = new RelayCommand(
                        param => getInfo());
                }
                return _infoCommand;
            }
        }

        public List<IPageViewModel> PageViewModels
        {
            get
            {
                if(_pageViewModels == null)
                {
                    _pageViewModels = new List<IPageViewModel>();
                }
                return _pageViewModels;
            }
        }

        public IPageViewModel CurrentPageViewModel
        {
            get
            {
                return _currentPageViewModel;
            }
            set
            {
                if (_currentPageViewModel != value)
                {
                    _currentPageViewModel = value;
                    OnPropertyChanged("CurrentPageViewModel");
                }
            }
        }

        private void getInfo()
        {
            WindowInfo ww = new WindowInfo();
            var vmm = new SecondWindowViewModel();
            vmm.ClosingRequest += ww.Close;
            ww.DataContext = vmm;
            ww.Show();
        }

        private void ChangeViewModel(IPageViewModel viewModel)
        {
            if (!PageViewModels.Contains(viewModel))
            {
                PageViewModels.Add(viewModel);
            }
            CurrentPageViewModel = PageViewModels.FirstOrDefault(vm => vm == viewModel);
        }

        private void onClose()
        {
            foreach (ServerClass se in svm.serverList)
            {
                Socket so = se.socket;
                svm.keepListeningList[so] = false; //così smette di ascoltare (check!)

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

                //_socket.Receive(bytes);
                //prova = System.Text.Encoding.Default.GetString(bytes);

                //aspetto la terminazione del thread associato e lo elimino dalla lista


                svm.threadList[so].Join();
                svm.threadList.Remove(so);
                svm.keepListeningList.Remove(so);
            }

            foreach(IPAddress ipa in svm.socketList.Keys)
            {
                svm.socketList[ipa].Shutdown(SocketShutdown.Receive);    //giusto both???
                svm.socketList[ipa].Close();
            }
        }
    }
}
