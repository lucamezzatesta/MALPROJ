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
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Button lastButton;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void click_Click(object sender, RoutedEventArgs e)
        {
            Button current = (Button)sender;
            current.Background = (SolidColorBrush) new BrushConverter().ConvertFrom("#FF6A6969");
            current.Foreground = new SolidColorBrush(Colors.White);
            if (lastButton != null && lastButton != current)
            {
                lastButton.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF6A6969"); 
                lastButton.Background = new SolidColorBrush(Colors.White);
            }
            lastButton = current;
        }

        private void click_Initialized(object sender, EventArgs e)
        {
            if (lastButton == null)
            {
                lastButton = (Button)sender;
            }
        }
    }
}
