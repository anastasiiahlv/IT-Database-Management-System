using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using DatabaseCore.Models;
using DatabaseCore.Services;
using DatabaseDesktopClient.Services;

namespace DatabaseDesktopClient.Views
{
    public partial class CreateTableDialog : Window
    {
        private readonly DatabaseService _databaseService;
        private readonly ObservableCollection<ColumnInfo> _columns;

        public string TableName { get; private set; }

        public CreateTableDialog(DatabaseService databaseService)
        {
            InitializeComponent();
            _databaseService = databaseService;
            _columns = new ObservableCollection<ColumnInfo>();

            ColumnsDataGrid.ItemsSource = _columns;

            // Заповнюємо ComboBox типами даних
            DataTypeComboBox.ItemsSource = Enum.GetValues(typeof(DataType));
            DataTypeComboBox.SelectedIndex = 0;

            TableNameTextBox.Focus();
        }

        private void AddColumn_Click(object sender, RoutedEventArgs e)
        {
            var columnName = NewColumnNameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(columnName))
            {
                ShowError("Введіть назву колонки");
                return;
            }

            // Валідація назви
            var validation = ValidationService.ValidateColumnName(columnName);
            if (!validation.IsValid)
            {
                ShowError(validation.ErrorMessage);
                return;
            }

            // Перевірка на дублікат
            if (_columns.Any(c => c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase)))
            {
                ShowError("Колонка з такою назвою вже існує");
                return;
            }

            var dataType = (DataType)DataTypeComboBox.SelectedItem;
            _columns.Add(new ColumnInfo { Name = columnName, DataType = dataType });

            NewColumnNameTextBox.Clear();
            NewColumnNameTextBox.Focus();
            HideError();
        }

        private void RemoveColumn_Click(object sender, RoutedEventArgs e)
        {
            if (ColumnsDataGrid.SelectedItem is ColumnInfo column)
            {
                _columns.Remove(column);
            }
        }

        private void CreateTable_Click(object sender, RoutedEventArgs e)
        {
            var tableName = TableNameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(tableName))
            {
                ShowError("Введіть назву таблиці");
                return;
            }

            var validation = ValidationService.ValidateTableName(tableName);
            if (!validation.IsValid)
            {
                ShowError(validation.ErrorMessage);
                return;
            }

            if (_columns.Count == 0)
            {
                ShowError("Додайте хоча б одну колонку");
                return;
            }

            try
            {
                var columns = _columns.Select(c => new Column(c.Name, c.DataType)).ToList();
                _databaseService.CreateTable(tableName, columns);

                TableName = tableName;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                ShowError($"Помилка: {ex.Message}");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ShowError(string message)
        {
            ErrorTextBlock.Text = message;
            ErrorTextBlock.Visibility = Visibility.Visible;
        }

        private void HideError()
        {
            ErrorTextBlock.Visibility = Visibility.Collapsed;
        }

        public class ColumnInfo
        {
            public string Name { get; set; }
            public DataType DataType { get; set; }

            public string DisplayType => DataType switch
            {
                DataType.Integer => "Ціле число",
                DataType.Real => "Дійсне число",
                DataType.Char => "Символ",
                DataType.String => "Рядок",
                DataType.Money => "Гроші",
                DataType.MoneyInterval => "Інтервал грошей",
                _ => DataType.ToString()
            };
        }
    }
}