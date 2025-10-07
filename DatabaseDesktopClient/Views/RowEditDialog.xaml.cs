using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DatabaseCore.Models;
using DatabaseCore.Services;

namespace DatabaseDesktopClient.Views
{
    public partial class RowEditDialog : Window
    {
        private readonly Table _table;
        private readonly Row _existingRow;
        private readonly Dictionary<string, FieldInfo> _fields = new();

        public RowEditDialog(Table table, Row existingRow)
        {
            InitializeComponent();
            _table = table;
            _existingRow = existingRow;

            Title = existingRow == null ? "Додати рядок" : "Редагувати рядок";

            GenerateFields();
        }

        private void GenerateFields()
        {
            var scrollViewer = this.FindName("FieldsScrollViewer") as ScrollViewer;
            if (scrollViewer == null || scrollViewer.Content is not StackPanel fieldsPanel)
                return;

            fieldsPanel.Children.Clear();
            _fields.Clear();

            foreach (var column in _table.Columns)
            {
                var panel = new StackPanel { Margin = new Thickness(0, 0, 0, 15) };

                // Label
                var label = new Label
                {
                    Content = $"{column.Name}",
                    FontWeight = FontWeights.SemiBold,
                    FontSize = 14
                };
                panel.Children.Add(label);

                // Підказка про тип
                var hint = new TextBlock
                {
                    Text = GetDataTypeHint(column.DataType),
                    Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117)),
                    FontSize = 11,
                    Margin = new Thickness(5, 0, 0, 5)
                };
                panel.Children.Add(hint);

                // TextBox
                var textBox = new TextBox
                {
                    Height = 35,
                    FontSize = 13,
                    Tag = column
                };

                // Встановлюємо існуюче значення
                if (_existingRow != null)
                {
                    var value = _existingRow.GetValue(column.Name);
                    textBox.Text = FormatValue(value);
                }

                // Додаємо валідацію при зміні тексту
                textBox.TextChanged += (s, e) => ValidateField(textBox, column);

                panel.Children.Add(textBox);

                // TextBlock для помилки
                var errorText = new TextBlock
                {
                    Foreground = Brushes.Red,
                    FontSize = 11,
                    Margin = new Thickness(5, 2, 0, 0),
                    TextWrapping = TextWrapping.Wrap,
                    Visibility = Visibility.Collapsed
                };
                panel.Children.Add(errorText);

                fieldsPanel.Children.Add(panel);

                _fields[column.Name] = new FieldInfo
                {
                    TextBox = textBox,
                    ErrorText = errorText,
                    Column = column
                };
            }
        }

        private void ValidateField(TextBox textBox, Column column)
        {
            var errorText = _fields[column.Name].ErrorText;

            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                // Порожнє значення (null) - OK
                textBox.BorderBrush = new SolidColorBrush(Color.FromRgb(189, 189, 189));
                textBox.BorderThickness = new Thickness(1);
                errorText.Visibility = Visibility.Collapsed;
                return;
            }

            try
            {
                // Спробуємо валідувати
                var validation = ValidationService.ValidateValue(textBox.Text, column.DataType);

                if (validation.IsValid)
                {
                    // Успішна валідація
                    textBox.BorderBrush = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Зелений
                    textBox.BorderThickness = new Thickness(2);
                    errorText.Visibility = Visibility.Collapsed;
                }
                else
                {
                    // Помилка валідації
                    textBox.BorderBrush = Brushes.Red;
                    textBox.BorderThickness = new Thickness(2);
                    errorText.Text = validation.ErrorMessage;
                    errorText.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                // Помилка валідації
                textBox.BorderBrush = Brushes.Red;
                textBox.BorderThickness = new Thickness(2);
                errorText.Text = ex.Message;
                errorText.Visibility = Visibility.Visible;
            }
        }

        private string GetDataTypeHint(DataType dataType)
        {
            return dataType switch
            {
                DataType.Integer => "Ціле число (наприклад: 123)",
                DataType.Real => "Дійсне число (наприклад: 123.45)",
                DataType.Char => "Один символ (наприклад: A)",
                DataType.String => "Текст (наприклад: Привіт світ)",
                DataType.Money => "Гроші від 0.00 до 10,000,000,000,000.00 (наприклад: 1000.50)",
                DataType.MoneyInterval => "Інтервал (наприклад: 100.00-500.00)",
                _ => dataType.ToString()
            };
        }

        private string FormatValue(object value)
        {
            if (value == null) return "";
            if (value is MoneyValue money) return money.Amount.ToString("F2");
            if (value is MoneyIntervalValue interval) return $"{interval.From.Amount:F2}-{interval.To.Amount:F2}";
            return value.ToString();
        }

        public Dictionary<string, object> GetRowData()
        {
            var data = new Dictionary<string, object>();

            foreach (var field in _fields.Values)
            {
                var columnName = field.Column.Name;

                try
                {
                    var value = field.Column.ConvertValue(
                        string.IsNullOrWhiteSpace(field.TextBox.Text) ? null : field.TextBox.Text
                    );
                    data[columnName] = value;
                }
                catch
                {
                    data[columnName] = null;
                }
            }

            return data;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Перевіряємо всі поля перед збереженням
            var hasErrors = false;
            var errorMessages = new List<string>();

            foreach (var field in _fields.Values)
            {
                if (string.IsNullOrWhiteSpace(field.TextBox.Text))
                    continue; // null - OK

                var validation = ValidationService.ValidateValue(field.TextBox.Text, field.Column.DataType);
                if (!validation.IsValid)
                {
                    hasErrors = true;
                    errorMessages.Add($"{field.Column.Name}: {validation.ErrorMessage}");

                    // Показуємо помилку
                    field.TextBox.BorderBrush = Brushes.Red;
                    field.TextBox.BorderThickness = new Thickness(2);
                    field.ErrorText.Text = validation.ErrorMessage;
                    field.ErrorText.Visibility = Visibility.Visible;
                }
            }

            if (hasErrors)
            {
                MessageBox.Show(
                    "Виправте помилки у полях:\n\n" + string.Join("\n", errorMessages),
                    "Помилка валідації",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            DialogResult = true;
            Close();
        }

        private class FieldInfo
        {
            public TextBox TextBox { get; set; }
            public TextBlock ErrorText { get; set; }
            public Column Column { get; set; }
        }
    }
}