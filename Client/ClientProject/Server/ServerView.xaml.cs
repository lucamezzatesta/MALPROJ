using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClientProject
{
    /// <summary>
    /// Logica di interazione per ServerView.xaml
    /// </summary>
    public partial class ServerView : UserControl
    {
        public ServerView()
        {
            InitializeComponent();
        }

        private void textBoxComandi_KeyDown(object sender, KeyEventArgs e)
        {
            ServerViewModel svm = this.DataContext as ServerViewModel;
            svm.getKey(e);
        }
    }
}
