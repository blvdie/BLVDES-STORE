using Microsoft.Win32;
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

namespace KURSOVAYA_RABOTA
{
    /// <summary>
    /// Логика взаимодействия для Start.xaml
    /// </summary>
    public partial class Start : Page
    {
        public Start()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            // Перейти на страницу входа
            ((MainWindow)Application.Current.MainWindow).MainFrame.Navigate(new Login());
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            // Перейти на страницу регистрации
            ((MainWindow)Application.Current.MainWindow).MainFrame.Navigate(new Register());
        }
    }
}
