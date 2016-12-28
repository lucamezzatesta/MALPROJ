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
using System.Collections;

namespace ClientProject
{
    /// <summary>
    /// Logica di interazione per ApplicationView.xaml
    /// </summary>
    public partial class ApplicationView : UserControl
    {
        public ApplicationView()
        {
            
            InitializeComponent();
        }

        private void textBoxComandi_KeyDown(object sender, KeyEventArgs e)
        {
            ApplicationViewModel avm = this.DataContext as ApplicationViewModel;
            avm.getKey(e);
        }

        private void listViewApplicazioni_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplicationViewModel avm = this.DataContext as ApplicationViewModel;
            if (avm != null)
            {
                avm.getSelected();
            }
            
        }
    }
}
