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
using System.Windows.Shapes;

namespace ClientProject
{
    /// <summary>
    /// Logica di interazione per WindowIndirizzoDuplicato.xaml
    /// </summary>
    public partial class WindowIndirizzoDuplicato : Window
    {
        public WindowIndirizzoDuplicato()
        {
            InitializeComponent();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.Close();
            }
        }
    }
}
