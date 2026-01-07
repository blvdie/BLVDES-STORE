using KURSOVAYA_RABOTA.database;
using KURSOVAYA_RABOTA.models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Логика взаимодействия для Buy.xaml
    /// </summary>
    public partial class Buy : Page
    {
        private List<DotaSkin> _products = new List<DotaSkin>();
        private Dictionary<int, int> _cartItems = new Dictionary<int, int>();
        private string _connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=BlvdesStoreDB;Integrated Security=True;";
        private const int PageSize = 15;
        public bool IsAdmin { get; set; }

        public Buy()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += OnPageLoaded;
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            LoadUserInfo();
            CheckAdminStatus();
            LoadFilters();
            LoadProducts();

            // Добавляем кнопку "Добавить +" только для админа
            if (IsAdmin)
            {
                var addButton = new Button
                {
                    Content = "Добавить +",
                    Style = (Style)FindResource("PrimaryButtonStyle"),
                    Width = 100,
                    Height = 40,
                    Margin = new Thickness(0, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Top
                };
                addButton.Click += (s, ev) => AddProduct_Click(s, ev);
                SearchAndAddPanel.Children.Add(addButton);
            }
        }

        #region Пользователь

        private void LoadUserInfo()
        {
            var user = App.CurrentUser;
            if (user != null)
            {
                UserNameText.Text = user.NickName;
                UserBalanceText.Text = $"Баланс: ₽{user.Balance:F2}";
            }
            else
            {
                UserNameText.Text = "Гость";
                UserBalanceText.Text = "Баланс: ₽0.00";
            }
        }

        private User GetCurrentUser()
        {
            try
            {
                if (App.CurrentUser == null) return null;
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT NickName, Balance FROM Users WHERE UserId = @userId";
                    var cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@userId", App.CurrentUser.UserId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            App.CurrentUser.NickName = reader.GetString(reader.GetOrdinal("NickName"));
                            App.CurrentUser.Balance = reader.GetDecimal(reader.GetOrdinal("Balance"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки пользователя: " + ex.Message);
                return null;
            }
            return App.CurrentUser;
        }

        private void CheckAdminStatus()
        {
            IsAdmin = App.CurrentUser?.RoleId == 1;
            var user = App.CurrentUser;
            if (user == null) return;
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT RoleName FROM Roles WHERE RoleId = @roleId";
                var cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@roleId", user.RoleId);
                var result = cmd.ExecuteScalar() as string;
                IsAdmin = result == "Администратор" || result == "Admin";
            }
        }

        #endregion

        #region Фильтры

        private void LoadFilters()
        {
            LoadHeroes();
            LoadSlots();
            LoadRarities();
            LoadHoldPeriods();
        }

        private void LoadHeroes()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT HeroId, HeroName FROM DotaHeroes";
                    HeroesList.Items.Clear();
                    using (var reader = new SqlCommand(query, connection).ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var hero = new models.Hero
                            {
                                HeroId = reader.GetInt32(0),
                                HeroName = reader.GetString(1)
                            };
                            var cb = new CheckBox { Content = hero.HeroName, Tag = hero.HeroId };
                            cb.Checked += (s, ev) => ApplyFilters();
                            cb.Unchecked += (s, ev) => ApplyFilters();
                            HeroesList.Items.Add(cb);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки героев: " + ex.Message);
            }
        }

        private void LoadSlots()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT SlotId, SlotName FROM DotaSlots";
                    SlotsList.Items.Clear();
                    using (var reader = new SqlCommand(query, connection).ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var slot = new models.Slot
                            {
                                SlotId = reader.GetInt32(0),
                                SlotName = reader.GetString(1)
                            };
                            var cb = new CheckBox { Content = slot.SlotName, Tag = slot.SlotId };
                            cb.Checked += (s, ev) => ApplyFilters();
                            cb.Unchecked += (s, ev) => ApplyFilters();
                            SlotsList.Items.Add(cb);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки слотов: " + ex.Message);
            }
        }

        private void LoadRarities()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT RarityId, RarityName FROM DotaRarities";
                    RaritiesList.Items.Clear();
                    using (var reader = new SqlCommand(query, connection).ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var rarity = new models.Rarity
                            {
                                RarityId = reader.GetInt32(0),
                                RarityName = reader.GetString(1)
                            };
                            var cb = new CheckBox { Content = rarity.RarityName, Tag = rarity.RarityId };
                            cb.Checked += (s, ev) => ApplyFilters();
                            cb.Unchecked += (s, ev) => ApplyFilters();
                            RaritiesList.Items.Add(cb);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки редкостей: " + ex.Message);
            }
        }

        private void LoadHoldPeriods()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT HoldPeriodId, Description FROM HoldPeriods";
                    HoldList.Items.Clear();
                    using (var reader = new SqlCommand(query, connection).ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var hold = new models.HoldPeriod
                            {
                                HoldPeriodId = reader.GetInt32(0),
                                Description = reader.GetString(1)
                            };
                            var cb = new CheckBox { Content = hold.Description, Tag = hold.HoldPeriodId };
                            cb.Checked += (s, ev) => ApplyFilters();
                            cb.Unchecked += (s, ev) => ApplyFilters();
                            HoldList.Items.Add(cb);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки холдов: " + ex.Message);
            }
        }

        #endregion

        #region Загрузка товаров

        private void LoadProducts(int page = 1)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var baseQuery = new StringBuilder("SELECT SkinId, Name, Price, IconUrl FROM DotaSkins WHERE 1=1");
                    var filters = new List<string>();
                    var parameters = new List<SqlParameter>();

                    // Поиск по имени
                    if (!string.IsNullOrEmpty(SearchBox.Text) && SearchBox.Text != "Поиск...")
                    {
                        filters.Add("Name LIKE '%' + @search + '%'");
                        parameters.Add(new SqlParameter("@search", SearchBox.Text));
                    }

                    // Фильтры по цене
                    if (decimal.TryParse(MinPriceBox.Text, out decimal minPrice))
                    {
                        filters.Add("Price >= @minPrice");
                        parameters.Add(new SqlParameter("@minPrice", minPrice));
                    }
                    if (decimal.TryParse(MaxPriceBox.Text, out decimal maxPrice))
                    {
                        filters.Add("Price <= @maxPrice");
                        parameters.Add(new SqlParameter("@maxPrice", maxPrice));
                    }

                    // Фильтры по чекбоксам
                    AddCheckBoxFilters(filters, parameters, HeroesList.Items, "HeroId", "@heroId");
                    AddCheckBoxFilters(filters, parameters, SlotsList.Items, "SlotId", "@slotId");
                    AddCheckBoxFilters(filters, parameters, RaritiesList.Items, "RarityId", "@rarityId");
                    AddCheckBoxFilters(filters, parameters, HoldList.Items, "HoldPeriodId", "@holdId");

                    if (filters.Count > 0)
                    {
                        baseQuery.Append(" AND " + string.Join(" AND ", filters));
                    }

                    // Сортировка
                    if (SortToggleAsc?.IsChecked == true)
                    {
                        baseQuery.Append(" ORDER BY Price ASC");
                    }
                    else if (SortToggleDesc?.IsChecked == true)
                    {
                        baseQuery.Append(" ORDER BY Price DESC");
                    }
                    else
                    {
                        baseQuery.Append(" ORDER BY Name ASC");
                    }

                    // Пагинация
                    int offset = (page - 1) * PageSize;
                    baseQuery.AppendFormat(" OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", offset, PageSize);

                    var products = new List<DotaSkin>();

                    using (var cmd = new SqlCommand(baseQuery.ToString(), connection))
                    {
                        cmd.Parameters.AddRange(parameters.ToArray());
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                products.Add(new DotaSkin
                                {
                                    SkinId = reader.GetInt32(reader.GetOrdinal("SkinId")),
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                    Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                    ImageSource = reader.IsDBNull(reader.GetOrdinal("IconUrl")) ?
                                        "/Resources/default_skin.png" :
                                        reader.GetString(reader.GetOrdinal("IconUrl"))
                                });
                            }
                        }
                    }

                    // ✅ ВСЕГДА обновляем ItemsSource, даже если товаров нет
                    ProductsList.ItemsSource = products;

                    // ✅ Обновляем пагинацию
                    UpdatePaginationButtons(page);
                }
            }
            catch (Exception ex)
            {
                // ❌ Убираем сообщение об ошибке, если товары всё равно загружены
                // MessageBox.Show("Ошибка загрузки товаров: " + ex.Message); ← УДАЛЕНО
            }
        }

        private void AddCheckBoxFilters(List<string> filters, List<SqlParameter> parameters, ItemCollection items, string column, string paramName)
        {
            var selectedValues = items.OfType<CheckBox>()
                                      .Where(cb => cb.IsChecked == true)
                                      .Select(cb => cb.Tag).ToList();

            if (selectedValues.Count > 0)
            {
                var paramNames = new List<string>();
                for (int i = 0; i < selectedValues.Count; i++)
                {
                    var fullParamName = $"{paramName}_{i}";
                    parameters.Add(new SqlParameter(fullParamName, selectedValues[i]));
                    paramNames.Add(fullParamName);
                }

                filters.Add($"{column} IN ({string.Join(", ", paramNames)})");
            }
        }

        private void UpdatePaginationButtons(int page = 1)
        {
            PaginationPanel.Children.Clear();
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string countQuery = "SELECT COUNT(*) FROM DotaSkins WHERE 1=1";
                    var filters = new List<string>();
                    var parameters = new List<SqlParameter>();

                    // Копируем фильтры для подсчёта
                    if (!string.IsNullOrEmpty(SearchBox.Text) && SearchBox.Text != "Поиск...")
                    {
                        filters.Add("Name LIKE '%' + @search + '%'");
                        parameters.Add(new SqlParameter("@search", SearchBox.Text));
                    }

                    AddCheckBoxFilters(filters, parameters, HeroesList.Items, "HeroId", "@heroId");
                    AddCheckBoxFilters(filters, parameters, SlotsList.Items, "SlotId", "@slotId");
                    AddCheckBoxFilters(filters, parameters, RaritiesList.Items, "RarityId", "@rarityId");
                    AddCheckBoxFilters(filters, parameters, HoldList.Items, "HoldPeriodId", "@holdId");

                    if (filters.Count > 0)
                    {
                        countQuery += " AND " + string.Join(" AND ", filters);
                    }

                    using (var countCmd = new SqlCommand(countQuery, connection))
                    {
                        countCmd.Parameters.AddRange(parameters.ToArray());
                        int totalItems = (int)countCmd.ExecuteScalar();
                        int totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

                        // ✅ Добавляем хотя бы одну кнопку (страницу 1), чтобы не исчезала разметка
                        for (int i = 1; i <= Math.Max(1, totalPages); i++)
                        {
                            Button btn = new Button
                            {
                                Content = i.ToString(),
                                Width = 30,
                                Height = 30,
                                BorderBrush = Brushes.Purple,
                                Margin = new Thickness(2),
                                Tag = i
                            };

                            btn.Click += (s, e) =>
                            {
                                int pageNum = (int)btn.Tag;
                                LoadProducts(pageNum);
                            };

                            PaginationPanel.Children.Add(btn);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Не критично — можно проигнорировать
                // MessageBox.Show("Ошибка пагинации: " + ex.Message);
            }
        }

        private void ApplyFilters()
        {
            LoadProducts();
        }

        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in HeroesList.Items.OfType<CheckBox>()) item.IsChecked = false;
            foreach (var item in SlotsList.Items.OfType<CheckBox>()) item.IsChecked = false;
            foreach (var item in RaritiesList.Items.OfType<CheckBox>()) item.IsChecked = false;
            foreach (var item in HoldList.Items.OfType<CheckBox>()) item.IsChecked = false;
            MinPriceBox.Text = "Цена от";
            MaxPriceBox.Text = "Цена до";
            LoadProducts();
        }

        #endregion

        #region Корзина



        private void AddToCart_Click(int skinId)
        {
            if (IsAdmin) return;

            // Ограничиваем до 10 штук
            if (_cartItems.TryGetValue(skinId, out int qty))
            {
                if (qty >= 10) return;
                _cartItems[skinId] = qty + 1;
            }
            else
            {
                _cartItems[skinId] = 1;
            }

            SaveToCart(skinId);
            UpdateCartInterface(skinId);
        }

        private void RemoveFromCart_Click(int skinId)
        {
            if (_cartItems.ContainsKey(skinId))
            {
                if (_cartItems[skinId] > 1)
                {
                    _cartItems[skinId]--;
                }
                else
                {
                    _cartItems.Remove(skinId);
                }

                SaveToCart(skinId);
                UpdateCartInterface(skinId);
            }
        }

        private void UpdateCartInterface(int skinId)
        {
            foreach (var product in ProductsList.Items.OfType<DotaSkin>())
            {
                if (product.SkinId == skinId)
                {
                    var container = FindVisualChild<FrameworkElement>(ProductsList, skinId);
                    if (container == null) continue;

                    var actionPanel = FindVisualChild<ContentControl>(container, skinId);
                    if (actionPanel == null) continue;

                    if (IsAdmin)
                    {
                        var adminPanel = FindVisualChild<StackPanel>(container, skinId);
                        if (adminPanel != null)
                        {
                            actionPanel.Content = adminPanel;
                        }
                    }
                    else
                    {
                        if (_cartItems.TryGetValue(skinId, out int quantity) && quantity > 0)
                        {
                            var panel = new StackPanel
                            {
                                Orientation = Orientation.Horizontal,
                                HorizontalAlignment = HorizontalAlignment.Center
                            };

                            var minusBtn = new Button
                            {
                                Content = "-",
                                Tag = skinId,
                                Style = (Style)FindResource("FlatButtonStyle"),
                                Width = 30
                            };
                            minusBtn.Click += (s, e) => RemoveFromCart_Click(skinId);

                            var txt = new TextBlock
                            {
                                Text = quantity.ToString(),
                                FontSize = 14,
                                Foreground = Brushes.White,
                                VerticalAlignment = VerticalAlignment.Center,
                                Margin = new Thickness(5)
                            };

                            var plusBtn = new Button
                            {
                                Content = "+",
                                Tag = skinId,
                                Style = (Style)FindResource("FlatButtonStyle"),
                                Width = 30
                            };
                            plusBtn.Click += (s, e) => AddToCart_Click(skinId);

                            panel.Children.Add(minusBtn);
                            panel.Children.Add(txt);
                            panel.Children.Add(plusBtn);

                            actionPanel.Content = panel;
                        }
                        else
                        {
                            var addToCartBtn = new Button
                            {
                                Content = "🛒 В КОРЗИНУ",
                                Style = (Style)FindResource("FlatButtonStyle"),
                                Tag = skinId,
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                Margin = new Thickness(5, 0, 9, 10)
                            };
                            addToCartBtn.Click += (s, e) => AddToCart_Click(skinId);
                            actionPanel.Content = addToCartBtn;
                        }
                    }
                }
            }
        }

        private void SaveToCart(int skinId)
        {
            if (App.CurrentUser == null) return;

            int userId = App.CurrentUser.UserId;

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var cmd = new SqlCommand(@"
                IF EXISTS (SELECT * FROM Carts WHERE UserId = @userId AND SkinId = @skinId AND GameId = 2)
                    UPDATE Carts SET Quantity = @quantity WHERE UserId = @userId AND SkinId = @skinId AND GameId = 2
                ELSE
                    INSERT INTO Carts (UserId, SkinId, GameId, Quantity, AddedAt)
                    VALUES (@userId, @skinId, 2, @quantity, GETDATE())", conn);

                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@skinId", skinId);

                    // ✅ Безопасное получение значения из Dictionary
                    int quantity = _cartItems.TryGetValue(skinId, out int value) ? value : 0;
                    cmd.Parameters.AddWithValue("@quantity", quantity);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при работе с корзиной: " + ex.Message);
            }
        }

        #endregion

        #region Админские функции
        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new Add();
            if (addWindow.ShowDialog() == true)
            {
                LoadProducts(); // обновляем список после добавления
            }
        }

        private void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && int.TryParse(btn.Tag.ToString(), out int skinId))
            {
                var editWindow = new Edit(skinId);
                if (editWindow.ShowDialog() == true)
                {
                    LoadProducts(); // обновляем список после редактирования
                }
            }
        }

        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && int.TryParse(btn.Tag.ToString(), out int skinId))
            {
                if (MessageBox.Show("Вы уверены, что хотите удалить этот скин?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var connection = new SqlConnection(_connectionString))
                        {
                            connection.Open();
                            var cmd = new SqlCommand("DELETE FROM DotaSkins WHERE SkinId = @SkinId", connection);
                            cmd.Parameters.AddWithValue("@SkinId", skinId);
                            cmd.ExecuteNonQuery();
                            LoadProducts(); // обновляем интерфейс
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка удаления: " + ex.Message);
                    }
                }
            }
        }

        #endregion

        #region Навигация

        private void GoToStart_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Start());
        private void AddToCart_Nav(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Cart());
        private void Logout_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Start());

        #endregion

        #region Обработчики событий UI

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb.Text == "Поиск...")
            {
                tb.Text = "";
                tb.Foreground = Brushes.White;
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (string.IsNullOrWhiteSpace(tb.Text))
            {
                tb.Text = "Поиск...";
                tb.Foreground = Brushes.Gray;
            }
        }
        private void SearchBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Разрешаем только буквы и цифры
            if (!System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"^[a-zA-Z0-9а-яА-ЯёЁ]+$"))
            {
                e.Handled = true;
            }
            else
            {
                e.Handled = false;
            }
        }

        private void OnFilterBoxGotFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb.Text == "Цена от" || tb.Text == "Цена до")
            {
                tb.Text = "";
                tb.Foreground = Brushes.White;
            }
        }

        private void OnFilterBoxLostFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (string.IsNullOrWhiteSpace(tb.Text))
            {
                tb.Text = tb.Name.Contains("Min") ? "Цена от" : "Цена до";
                tb.Foreground = Brushes.Gray;
            }
        }

        private void MinPriceBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
                e.Handled = true;
        }

        private void MaxPriceBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
                e.Handled = true;
        }

        private void SortToggleAsc_Click(object sender, RoutedEventArgs e)
        {
            SortToggleDesc.IsChecked = false;
            LoadProducts();
        }

        private void SortToggleDesc_Click(object sender, RoutedEventArgs e)
        {
            SortToggleAsc.IsChecked = false;
            LoadProducts();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadProducts();
        }

        #endregion

        #region Вспомогательные методы

        private T FindVisualChild<T>(DependencyObject parent, int skinId) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is FrameworkElement fe && fe.DataContext is DotaSkin skin && skin.SkinId == skinId)
                {
                    var result = LogicalTreeHelper.FindLogicalNode(fe, typeof(T).Name) as T;
                    if (result != null) return result;
                }
                var nextLevel = FindVisualChild<T>(child, skinId);
                if (nextLevel != null) return nextLevel;
            }
            return null;
        }

        #endregion

        #region Обработчики кликов

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && int.TryParse(btn.Tag?.ToString(), out int skinId))
            {
                AddToCart_Click(skinId);
            }
        }
        private void GoToCart_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Cart());
        }
        #endregion
    }
}
    
