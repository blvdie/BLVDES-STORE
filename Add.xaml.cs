using KURSOVAYA_RABOTA.models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
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

namespace KURSOVAYA_RABOTA
{
    /// <summary>
    /// Логика взаимодействия для Add.xaml
    /// </summary>
    public partial class Add : Window
    {
        private string _connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=BlvdesStoreDB;Integrated Security=True;";

        public Add()
        {
            InitializeComponent();
            LoadCombos();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && textBox.Foreground == Brushes.Gray)
            {
                textBox.Text = string.Empty;
                textBox.Foreground = Brushes.White;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && string.IsNullOrWhiteSpace(textBox.Text))
            {
                string defaultText;

                if (textBox.Name == "NameBox")
                {
                    defaultText = "Название скина";
                }
                else if (textBox.Name == "PriceBox")
                {
                    defaultText = "Цена";
                }
                else if (textBox.Name == "IconUrlBox")
                {
                    defaultText = "URL изображения";
                }
                else
                {
                    defaultText = textBox.Text; // или задайте значение по умолчанию
                }

                textBox.Text = defaultText;
                textBox.Foreground = Brushes.Gray;
            }
        }

        private void PriceBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c) && c != '.')
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private void ChooseFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*";
            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                string filePath = openFileDialog.FileName;
                string fileName = System.IO.Path.GetFileName(filePath);
                string savePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icons", fileName);

                Directory.CreateDirectory(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icons"));
                File.Copy(filePath, savePath, overwrite: true);

                IconUrlBox.Text = $"icons/{fileName}";
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = @"
                        INSERT INTO DotaSkins (Name, Price, IconUrl, HeroId, SlotId, RarityId, HoldPeriodId)
                        VALUES (@Name, @Price, @IconUrl, @HeroId, @SlotId, @RarityId, @HoldPeriodId)";

                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Name", NameBox.Text);
                        cmd.Parameters.AddWithValue("@Price", decimal.Parse(PriceBox.Text));
                        cmd.Parameters.AddWithValue("@IconUrl", IconUrlBox.Text);
                        cmd.Parameters.AddWithValue("@HeroId", HeroCombo.SelectedValue ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@SlotId", SlotCombo.SelectedValue ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@RarityId", RarityCombo.SelectedValue ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@HoldPeriodId", HoldCombo.SelectedValue ?? DBNull.Value);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Скин успешно добавлен!");
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении скина: {ex.Message}");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void LoadCombos()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // Герои
                    using (var cmd = new SqlCommand("SELECT HeroId, HeroName FROM DotaHeroes", connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        var heroes = new List<Hero>();
                        while (reader.Read())
                        {
                            heroes.Add(new Hero
                            {
                                HeroId = reader.GetInt32(reader.GetOrdinal("HeroId")),
                                HeroName = reader.GetString(reader.GetOrdinal("HeroName"))
                            });
                        }
                        HeroCombo.ItemsSource = heroes;
                        HeroCombo.DisplayMemberPath = "HeroName";
                        HeroCombo.SelectedValuePath = "HeroId";
                    }

                    // Слоты
                    using (var cmd = new SqlCommand("SELECT SlotId, SlotName FROM DotaSlots", connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        var slots = new List<Slot>();
                        while (reader.Read())
                        {
                            slots.Add(new Slot
                            {
                                SlotId = reader.GetInt32(reader.GetOrdinal("SlotId")),
                                SlotName = reader.GetString(reader.GetOrdinal("SlotName"))
                            });
                        }
                        SlotCombo.ItemsSource = slots;
                        SlotCombo.DisplayMemberPath = "SlotName";
                        SlotCombo.SelectedValuePath = "SlotId";
                    }

                    // Редкости
                    using (var cmd = new SqlCommand("SELECT RarityId, RarityName FROM DotaRarities", connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        var rarities = new List<Rarity>();
                        while (reader.Read())
                        {
                            rarities.Add(new Rarity
                            {
                                RarityId = reader.GetInt32(reader.GetOrdinal("RarityId")),
                                RarityName = reader.GetString(reader.GetOrdinal("RarityName"))
                            });
                        }
                        RarityCombo.ItemsSource = rarities;
                        RarityCombo.DisplayMemberPath = "RarityName";
                        RarityCombo.SelectedValuePath = "RarityId";
                    }

                    // Холды
                    using (var cmd = new SqlCommand("SELECT HoldPeriodId, Description FROM HoldPeriods", connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        var holds = new List<HoldPeriod>();
                        while (reader.Read())
                        {
                            holds.Add(new HoldPeriod
                            {
                                HoldPeriodId = reader.GetInt32(reader.GetOrdinal("HoldPeriodId")),
                                Description = reader.GetString(reader.GetOrdinal("Description"))
                            });
                        }
                        HoldCombo.ItemsSource = holds;
                        HoldCombo.DisplayMemberPath = "Description";
                        HoldCombo.SelectedValuePath = "HoldPeriodId";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки фильтров: " + ex.Message);
            }
        }

        private void Minimize_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;
        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }
        private void Close_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}
