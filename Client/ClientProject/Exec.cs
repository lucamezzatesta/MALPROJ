using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Drawing;

namespace ClientProject
{
    public class Exec :ObservableObject
    {
        private BitmapImage _icona = null;
        private string _exe;

        public BitmapImage icona
        {
            get { return _icona; }
            set
            {
                if (_icona != value)
                {
                    _icona = value;
                    OnPropertyChanged("icona");
                }
            }
        }

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
    }
}
