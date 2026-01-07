using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace KURSOVAYA_RABOTA
{
    /// <summary>
    /// Логика взаимодействия для Login.xaml
    /// </summary>
    public partial class Login : Page
    {
        private string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=BlvdesStoreDB;Trusted_Connection=True;";

        public Login()
        {
            InitializeComponent();
        }


        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowError("Введите логин и пароль");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"
                SELECT UserId, Username, NickName, Email, RoleId, Balance 
                FROM Users 
                WHERE Username = @user AND Password = @pass";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@user", username);
                    cmd.Parameters.AddWithValue("@pass", password);

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        App.CurrentUser = new models.User
                        {
                            UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                            Username = reader.GetString(reader.GetOrdinal("Username")),
                            NickName = reader.GetString(reader.GetOrdinal("NickName")),
                            Email = reader.GetString(reader.GetOrdinal("Email")),
                            RoleId = reader.GetInt32(reader.GetOrdinal("RoleId")),
                            Balance = reader.GetDecimal(reader.GetOrdinal("Balance"))
                        };

                        reader.Close();

                        // ✅ Показываем уведомление с обращением по никнейму
                        string welcomeMessage = $"Добро пожаловать, {App.CurrentUser.NickName}!";
                        ShowSuccess(welcomeMessage); // Это зелёное уведомление

                        DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.5) };
                        timer.Tick += (s, ev) =>
                        {

                            timer.Stop();
                            // ✅ Проверяем, что MainWindow установлен как главное окно
                            if (Application.Current.MainWindow is MainWindow mainWindow)
                            {
                                mainWindow.MainFrame.Navigate(new Buy());
                            }
                            else
                            {
                                // Если MainWindow ещё не активен, установите его как главное окно
                                var window = new MainWindow();
                                Application.Current.MainWindow = window;
                                window.Show();
                                window.MainFrame.Navigate(new Buy());

                            }
                        };
                        timer.Start();
                    }
                    else
                    {
                        ShowError("Неверный логин или пароль");
                    }
                }
                catch (Exception ex)
                {
                    ShowError("Ошибка подключения: " + ex.Message);
                }
            }
        }
        private void AnimateShow()
        {
            DoubleAnimation fadeIn = new DoubleAnimation(1.0, TimeSpan.FromSeconds(0.3));
            NotificationBorder.Opacity = 0;
            NotificationBorder.RenderTransform = new TranslateTransform { X = 50 };
            NotificationBorder.BeginAnimation(OpacityProperty, fadeIn);

            var translate = new DoubleAnimation(50, 0, TimeSpan.FromSeconds(0.5));
            ((TranslateTransform)NotificationBorder.RenderTransform).BeginAnimation(TranslateTransform.XProperty, translate);
        }

        #region === Методы для уведомлений ===
        public void ShowError(string message)
        {
            NotificationIcon.Text = "⚠";
            NotificationText.Text = message;
            NotificationText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xD3, 0x2F, 0x2F));
            NotificationBorder.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFE, 0xE6, 0xE6));
            NotificationBorder.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFF, 0x73, 0x73));
            NotificationBorder.Visibility = Visibility.Visible;
            StartAutoHide();
        }

        public void ShowSuccess(string message)
        {
            NotificationIcon.Text = "✔";
            NotificationText.Text = message;
            NotificationText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x2E, 0x7D, 0x32));
            NotificationBorder.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xE8, 0xF5, 0xE9));
            NotificationBorder.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x81, 0xC7, 0x84));
            NotificationBorder.Visibility = Visibility.Visible;
            StartAutoHide();
        }

        private DispatcherTimer notificationTimer = new DispatcherTimer();

        private void StartAutoHide()
        {
            notificationTimer.Interval = TimeSpan.FromSeconds(5);
            notificationTimer.Tick += (s, ev) =>
            {
                HideNotification();
                notificationTimer.Stop();
            };
            notificationTimer.Start();
        }

        private void HideNotification()
        {
            DoubleAnimation fadeOut = new DoubleAnimation(0, TimeSpan.FromSeconds(0.5));
            fadeOut.Completed += (s, e) => NotificationBorder.Visibility = Visibility.Collapsed;
            NotificationBorder.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void CloseNotificationBtn_Click(object sender, RoutedEventArgs e)
        {
            HideNotification();
            notificationTimer.Stop();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (ShowPasswordCheck.IsChecked == true)
            {
                PasswordTextBox.Text = PasswordBox.Password;
            }
        }

        private void PasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ShowPasswordCheck.IsChecked == true)
            {
                PasswordBox.Password = PasswordTextBox.Text;
            }
        }

        private void ShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            bool isPasswordVisible = ShowPasswordCheck.IsChecked == true;

            if (isPasswordVisible)
            {
                PasswordTextBox.Text = PasswordBox.Password;
                PasswordBox.Visibility = Visibility.Collapsed;
                PasswordTextBox.Visibility = Visibility.Visible;
            }
            else
            {
                PasswordBox.Password = PasswordTextBox.Text;
                PasswordBox.Visibility = Visibility.Visible;
                PasswordTextBox.Visibility = Visibility.Collapsed;
            }
        }

        private void PasswordTextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void GoToRegister(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).MainFrame.Navigate(new Register());
        }
        #endregion
    }
}
