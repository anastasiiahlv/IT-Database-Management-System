using System;
using System.Windows;
using System.Windows.Input;
using DatabaseDesktopClient.Services;
using DatabaseDesktopClient.ViewModels;
using DatabaseDesktopClient.Views;
using Microsoft.Win32;

namespace DatabaseDesktopClient.Views
{
    /// <summary>
    /// Головне вікно програми
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DatabaseService _databaseService;

        public MainWindow()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();

            // Підписуємось на події
            _databaseService.DatabaseChanged += OnDatabaseChanged;
            _databaseService.TableAdded += OnTablesChanged;
            _databaseService.TableDeleted += OnTablesChanged;
        }

        #region Event Handlers для меню

        private void CreateDatabase_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new InputDialog("Створити нову базу даних", "Введіть назву бази даних:");
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _databaseService.CreateDatabase(dialog.InputText);
                    StatusText.Text = $"База даних '{dialog.InputText}' створена";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OpenDatabase_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Database Files (*.json)|*.json|All Files (*.*)|*.*",
                Title = "Відкрити базу даних"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _databaseService.LoadDatabase(dialog.FileName);
                    StatusText.Text = $"База даних завантажена";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveDatabase_Click(object sender, RoutedEventArgs e)
        {
            if (!_databaseService.HasOpenDatabase)
            {
                MessageBox.Show("Немає відкритої бази даних", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(_databaseService.CurrentFilePath))
            {
                SaveDatabaseAs_Click(sender, e);
                return;
            }

            try
            {
                _databaseService.SaveDatabase();
                StatusText.Text = "База даних збережена";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveDatabaseAs_Click(object sender, RoutedEventArgs e)
        {
            if (!_databaseService.HasOpenDatabase)
            {
                MessageBox.Show("Немає відкритої бази даних", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new SaveFileDialog
            {
                Filter = "Database Files (*.json)|*.json|All Files (*.*)|*.*",
                Title = "Зберегти базу даних",
                FileName = $"{_databaseService.DatabaseName}.json"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _databaseService.SaveDatabase(dialog.FileName);
                    StatusText.Text = $"База даних збережена у '{dialog.FileName}'";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CloseDatabase_Click(object sender, RoutedEventArgs e)
        {
            if (!_databaseService.HasOpenDatabase)
                return;

            var result = MessageBox.Show("Закрити поточну базу даних?", "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _databaseService.CloseDatabase();
                WelcomePanel.Visibility = Visibility.Visible;
                ContentArea.Children.Clear();
                ContentArea.Children.Add(WelcomePanel);
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void CreateTable_Click(object sender, RoutedEventArgs e)
        {
            if (!_databaseService.HasOpenDatabase)
            {
                MessageBox.Show("Спочатку відкрийте або створіть базу даних", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new CreateTableDialog(_databaseService)
            {
                Owner = this
            };

            if (dialog.ShowDialog() == true)
            {
                StatusText.Text = $"Таблиця '{dialog.TableName}' створена";
            }
        }

        private void DeleteTable_Click(object sender, RoutedEventArgs e)
        {
            if (TablesListBox.SelectedItem == null)
            {
                MessageBox.Show("Виберіть таблицю для видалення", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var tableName = TablesListBox.SelectedItem.ToString();
            var result = MessageBox.Show($"Видалити таблицю '{tableName}'?", "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _databaseService.DeleteTable(tableName);
                    StatusText.Text = "Таблиця видалена";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ShowDatabaseInfo_Click(object sender, RoutedEventArgs e)
        {
            if (!_databaseService.HasOpenDatabase)
                return;

            var stats = _databaseService.GetStatistics();
            var info = $"Назва: {stats.DatabaseName}\n" +
                      $"Таблиць: {stats.TableCount}\n" +
                      $"Всього рядків: {stats.TotalRowCount}\n" +
                      $"Створено: {stats.CreatedAt:dd.MM.yyyy HH:mm:ss}\n" +
                      $"Змінено: {stats.ModifiedAt:dd.MM.yyyy HH:mm:ss}";

            MessageBox.Show(info, "Інформація про базу даних", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Система управління табличними базами даних\n\n" +
                "Версія: 1.0\n\n" +
                "Підтримувані типи даних:\n" +
                "• Integer, Real, Char, String\n" +
                "• Money, MoneyInterval",
                "Про програму",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        #endregion

        #region Обробники подій

        private void OnDatabaseChanged(object sender, EventArgs e)
        {
            DatabaseNameText.Text = _databaseService.DatabaseName ?? "Немає відкритої бази даних";
            LoadTables();
            UpdateStatistics();

            // Показуємо контент
            WelcomePanel.Visibility = Visibility.Collapsed;
        }

        private void OnTablesChanged(object sender, EventArgs e)
        {
            LoadTables();
            UpdateStatistics();
        }

        private void LoadTables()
        {
            TablesListBox.Items.Clear();
            var tables = _databaseService.GetTableNames();
            foreach (var table in tables)
            {
                TablesListBox.Items.Add(table);
            }
        }

        private void UpdateStatistics()
        {
            if (!_databaseService.HasOpenDatabase)
            {
                TableCountText.Text = "0";
                RowCountText.Text = "0";
                return;
            }

            var stats = _databaseService.GetStatistics();
            TableCountText.Text = stats.TableCount.ToString();
            RowCountText.Text = stats.TotalRowCount.ToString();
        }

        private void TablesListBox_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (TablesListBox.SelectedItem == null)
                return;

            var tableName = TablesListBox.SelectedItem.ToString();
            OpenTable(tableName);
        }

        private void OpenTable(string tableName)
        {
            try
            {
                var tableViewModel = new TableViewModel(_databaseService, tableName);
                var tableView = new TableView
                {
                    DataContext = tableViewModel
                };

                ContentArea.Children.Clear();
                ContentArea.Children.Add(tableView);

                StatusText.Text = $"Відкрито таблицю '{tableName}'";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка відкриття таблиці: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}