using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Controls;

namespace ClientProject
{
    class SecondWindowViewModel:ObservableObject
    {
        private ICommand _closeCommand;

        public ICommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                {
                    _closeCommand = new RelayCommand(
                        param => Close(),
                        param => CanClose());
                }
                return _closeCommand;
            }
        }

        public event Action ClosingRequest;

        public virtual void Close()
        {
            if (ClosingRequest != null)
            {
                ClosingRequest();
            }
        }

        public virtual bool CanClose()
        {
            return true;
        }
    }
}
