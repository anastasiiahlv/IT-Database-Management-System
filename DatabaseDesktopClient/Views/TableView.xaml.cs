using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DatabaseCore.Models;
using DatabaseDesktopClient.Services;
using DatabaseDesktopClient.ViewModels;

namespace DatabaseDesktopClient.Views
{
    public partial class TableView : UserControl
    {
        private TableViewModel _viewModel;
        private Table _table;

        public TableView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is TableViewModel viewModel)
            {
                _viewModel = viewModel;
                LoadTableData();
            }
        }

        private void LoadTableData()
        {
            if (_viewModel == null) return;

            // Отримуємо таблицю
            _table = ((DatabaseService)typeof(TableViewModel)
                .GetField("_databaseService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(_viewModel))
                ?.GetTable((string)typeof(TableViewModel)
                .GetField("_tableName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(_viewModel));

            if (_table == null) return;

            TableNameText.Text = _table.Name;

            // Динамічно створюємо колонки DataGrid
            DataGridView.Columns.Clear();

            foreach (var column in _table.Columns)
            {
                DataGridView.Columns.Add(new DataGridTextColumn
                {
                    Header = $"{column.Name} ({GetDataTypeDisplay(column.DataType)})",
                    Binding = new System.Windows.Data.Binding($"Values[{column.Name}]")
                    {
                        Converter = new ValueConverter()
                    },
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star)
                });
            }

            // Заповнюємо ComboBox для сортування
            SortColumnComboBox.Items.Clear();
            foreach (var column in _table.Columns)
            {
                SortColumnComboBox.Items.Add(column.Name);
            }
            if (SortColumnComboBox.Items.Count > 0)
                SortColumnComboBox.SelectedIndex = 0;

            // Завантажуємо рядки
            RefreshData();
        }

        private void RefreshData()
        {
            if (_table == null) return;

            // Перезавантажуємо таблицю
            var databaseService = (DatabaseService)typeof(TableViewModel)
                .GetField("_databaseService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(_viewModel);

            _table = databaseService?.GetTable(_table.Name);

            DataGridView.ItemsSource = null;
            DataGridView.ItemsSource = _table?.Rows;

            RowCountText.Text = $"Всього рядків: {_table?.RowCount ?? 0}";
        }

        private string GetDataTypeDisplay(DataType dataType)
        {
            return dataType switch
            {
                DataType.Integer => "Число",
                DataType.Real => "Дійсне",
                DataType.Char => "Символ",
                DataType.String => "Рядок",
                DataType.Money => "Гроші",
                DataType.MoneyInterval => "Інтервал",
                _ => dataType.ToString()
            };
        }

        private void Sort_Click(object sender, RoutedEventArgs e)
        {
            if (SortColumnComboBox.SelectedItem == null)
            {
                MessageBox.Show("Виберіть колонку для сортування", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var columnName = SortColumnComboBox.SelectedItem.ToString();
            var ascending = SortAscendingCheckBox.IsChecked == true;

            try
            {
                _table.Sort(columnName, ascending);
                RefreshData();

                var direction = ascending ? "за зростанням" : "за спаданням";
                MessageBox.Show($"Таблиця відсортована за '{columnName}' {direction}", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка сортування: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshData();
        }

        private void AddRow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new RowEditDialog(_table, null)
                {
                    Owner = Window.GetWindow(this)
                };

                if (dialog.ShowDialog() == true)
                {
                    var databaseService = (DatabaseService)typeof(TableViewModel)
                        .GetField("_databaseService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        ?.GetValue(_viewModel);

                    databaseService?.AddRow(_table.Name, dialog.GetRowData());
                    RefreshData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditRow_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridView.SelectedItem is not Row selectedRow)
            {
                MessageBox.Show("Виберіть рядок для редагування", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            EditRow(selectedRow);
        }

        private void DataGrid_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataGridView.SelectedItem is Row selectedRow)
            {
                EditRow(selectedRow);
            }
        }

        private void EditRow(Row row)
        {
            try
            {
                var dialog = new RowEditDialog(_table, row)
                {
                    Owner = Window.GetWindow(this)
                };

                if (dialog.ShowDialog() == true)
                {
                    var databaseService = (DatabaseService)typeof(TableViewModel)
                        .GetField("_databaseService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        ?.GetValue(_viewModel);

                    databaseService?.UpdateRow(_table.Name, row.Id, dialog.GetRowData());
                    RefreshData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteRow_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridView.SelectedItem is not Row selectedRow)
            {
                MessageBox.Show("Виберіть рядок для видалення", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("Видалити вибраний рядок?", "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var databaseService = (DatabaseService)typeof(TableViewModel)
                        .GetField("_databaseService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        ?.GetValue(_viewModel);

                    databaseService?.DeleteRow(_table.Name, selectedRow.Id);
                    RefreshData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Converter для відображення значень
        private class ValueConverter : System.Windows.Data.IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if (value == null) return "";
                if (value is MoneyValue money) return money.ToString();
                if (value is MoneyIntervalValue interval) return interval.ToString();
                return value.ToString();
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }
}