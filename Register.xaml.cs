using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Логика взаимодействия для Register.xaml
    /// </summary>
    public partial class Register : Page
    {
        private string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=BlvdesStoreDB;Trusted_Connection=True;";
        private byte[] avatarBytes = null;

        public Register()
        {
            InitializeComponent();
        }

        private void SelectAvatar_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg";
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    AvatarPathText.Text = System.IO.Path.GetFileName(dlg.FileName);
                    avatarBytes = File.ReadAllBytes(dlg.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка чтения файла: " + ex.Message);
                }
            }
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameBox.Text.Trim();
            string nickname = NicknameBox.Text.Trim();
            string email = EmailBox.Text.Trim();
            string password = PasswordBox.Password.Trim();
            string confirmPassword = ConfirmPasswordBox.Password.Trim();

            if (string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(confirmPassword))
            {
                ShowError("Все поля обязательны для заполнения");
                return;
            }

            if (username.Length < 4 || !Regex.IsMatch(username, @"^[a-zA-Z][a-zA-Z0-9_-]{3,}$"))
            {
                ShowError("Логин должен содержать минимум 4 символа");
                return;
            }

            if (username.Contains(" "))
            {
                ShowError("Логин не должен содержать пробелов");
                return;
            }

            if (!IsValidEmail(email))
            {
                ShowError("Введите корректный email");
                return;
            }

            if (password.Length < 6)
            {
                ShowError("Пароль должен содержать минимум 6 символов");
                return;
            }

            if (password != confirmPassword)
            {
                ShowError("Пароли не совпадают");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string checkQuery = "SELECT COUNT(*) FROM Users WHERE Username = @username OR Email = @email";
                    SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                    checkCmd.Parameters.AddWithValue("@username", username);
                    checkCmd.Parameters.AddWithValue("@email", email);

                    int existingCount = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (existingCount > 0)
                    {
                        ShowError("Логин или email уже заняты");
                        return;
                    }

                    string query = @"
                        INSERT INTO Users (
                            Username, 
                            Email, 
                            Password, 
                            RoleId, 
                            NickName, 
                            Balance, 
                            AvatarImage
                        ) VALUES (
                            @username, 
                            @email, 
                            @password, 
                            2, 
                            @nickname, 
                            0.00, 
                            @avatar
                        )";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@password", password);
                    cmd.Parameters.AddWithValue("@nickname", nickname ?? DBNull.Value as object);

                    // Получаем аватар: пользовательский или дефолтный
                    byte[] avatarData = GetAvatarBytesForInsert();

                    if (avatarData != null)
                    {
                        cmd.Parameters.AddWithValue("@avatar", (object)avatarData);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@avatar", DBNull.Value);
                    }

                    cmd.ExecuteNonQuery();

                    ShowSuccess("Регистрация успешна!");
                    DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
                    timer.Tick += (s, ev) =>
                    {
                        timer.Stop();
                        ((MainWindow)Application.Current.MainWindow).MainFrame.Navigate(new Start());
                    };
                    timer.Start();
                }
                catch (Exception ex)
                {
                    ShowError("Ошибка регистрации: " + ex.Message);
                }
            }
        }

        private byte[] GetAvatarBytesForInsert()
        {
            if (avatarBytes != null && avatarBytes.Length > 0)
            {
                return avatarBytes;
            }
            // Загружаем дефолтный аватар из ресурсов
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    return (byte[])Properties.Resources.ResourceManager.GetObject("avatar");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось загрузить дефолтный аватар: " + ex.Message);
                return null;
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private DispatcherTimer notificationTimer = new DispatcherTimer();

        public void ShowError(string message)
        {
            NotificationIcon.Text = "⚠";
            NotificationText.Text = message;
            NotificationText.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFD32F2F"));
            NotificationBorder.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFEE6E6"));
            NotificationBorder.BorderBrush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFE57373"));
            NotificationBorder.Visibility = Visibility.Visible;

            AnimateShow();
            StartAutoHide();
        }

        public void ShowSuccess(string message)
        {
            NotificationIcon.Text = "✔";
            NotificationText.Text = message;
            NotificationText.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#2E7D32"));
            NotificationBorder.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#E8F5E9"));
            NotificationBorder.BorderBrush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#81C784"));
            NotificationBorder.Visibility = Visibility.Visible;

            AnimateShow();
            StartAutoHide();
        }

        private void AnimateShow()
        {
            DoubleAnimation fadeIn = new DoubleAnimation(1.0, TimeSpan.FromSeconds(0.3));
            NotificationBorder.Opacity = 0;
            NotificationBorder.RenderTransform = new System.Windows.Media.TranslateTransform { X = 50 };
            NotificationBorder.BeginAnimation(System.Windows.UIElement.OpacityProperty, fadeIn);

            var translate = new DoubleAnimation(50, 0, TimeSpan.FromSeconds(0.3));
            ((System.Windows.Media.TranslateTransform)NotificationBorder.RenderTransform).BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, translate);
        }

        private void StartAutoHide()
        {
            notificationTimer.Interval = TimeSpan.FromSeconds(5);
            notificationTimer.Tick += (s, e) =>
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
            NotificationBorder.BeginAnimation(System.Windows.UIElement.OpacityProperty, fadeOut);
        }

        private void CloseNotificationBtn_Click(object sender, RoutedEventArgs e)
        {
            HideNotification();
            notificationTimer.Stop();
        }

        private void GoToLogin(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).MainFrame.Navigate(new Login());
        }
    }
}
