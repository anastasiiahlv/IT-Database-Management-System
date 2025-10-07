using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DatabaseCore.Models;
using DatabaseCore.Services;

namespace DatabaseDesktopClient.ViewModels
{
    /// <summary>
    /// ViewModel для діалогу додавання/редагування рядка
    /// </summary>
    public partial class RowEditViewModel : ObservableObject
    {
        private readonly Table _table;
        private readonly Row? _existingRow;

        public RowEditViewModel(Table table, Row? existingRow)
        {
            _table = table;
            _existingRow = existingRow;

            Fields = new ObservableCollection<FieldViewModel>();
            IsEditMode = existingRow != null;
            Title = IsEditMode ? "Редагувати рядок" : "Додати рядок";

            LoadFields();
        }

        #region Властивості

        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private bool _isEditMode;

        [ObservableProperty]
        private ObservableCollection<FieldViewModel> _fields;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _hasError;

        #endregion

        #region Команди

        /// <summary>
        /// Команда збереження
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanSave))]
        private void Save(Window window)
        {
            try
            {
                // Валідація всіх полів
                var errors = new List<string>();
                foreach (var field in Fields)
                {
                    var validation = field.Validate();
                    if (!validation.IsValid)
                    {
                        errors.Add($"{field.ColumnName}: {validation.ErrorMessage}");
                    }
                }

                if (errors.Any())
                {
                    ErrorMessage = string.Join("\n", errors);
                    return;
                }

                // Закриваємо діалог
                window.DialogResult = true;
                window.Close();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Помилка: {ex.Message}";
            }
        }

        private bool CanSave() => Fields.Any();

        /// <summary>
        /// Команда скасування
        /// </summary>
        [RelayCommand]
        private void Cancel(Window window)
        {
            window.DialogResult = false;
            window.Close();
        }

        #endregion

        #region Методи

        /// <summary>
        /// Завантажує поля для редагування
        /// </summary>
        private void LoadFields()
        {
            Fields.Clear();

            foreach (var column in _table.Columns)
            {
                var existingValue = _existingRow?.GetValue(column.Name);
                var field = new FieldViewModel(column, existingValue);
                Fields.Add(field);
            }
        }

        /// <summary>
        /// Отримує дані рядка для збереження
        /// </summary>
        public Dictionary<string, object?> GetRowData()
        {
            var data = new Dictionary<string, object?>();

            foreach (var field in Fields)
            {
                data[field.ColumnName] = field.GetValue();
            }

            return data;
        }

        #endregion
    }

    /// <summary>
    /// ViewModel для окремого поля
    /// </summary>
    public partial class FieldViewModel : ObservableObject
    {
        private readonly Column _column;

        public FieldViewModel(Column column, object? initialValue)
        {
            _column = column;
            ColumnName = column.Name;
            DataType = column.DataType;
            DataTypeDisplay = GetDataTypeDisplayName(column.DataType);

            // Встановлюємо початкове значення
            if (initialValue != null)
            {
                InputValue = FormatValueForDisplay(initialValue);
            }
        }

        #region Властивості

        public string ColumnName { get; }
        public DataType DataType { get; }
        public string DataTypeDisplay { get; }

        [ObservableProperty]
        private string _inputValue = string.Empty;

        [ObservableProperty]
        private string _validationError = string.Empty;

        [ObservableProperty]
        private bool _hasError;

        #endregion

        #region Методи

        /// <summary>
        /// Валідує введене значення
        /// </summary>
        public ValidationResult Validate()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(InputValue))
                {
                    // null дозволено
                    HasError = false;
                    ValidationError = string.Empty;
                    return ValidationResult.Success();
                }

                var validation = ValidationService.ValidateValue(InputValue, DataType);

                HasError = !validation.IsValid;
                ValidationError = validation.IsValid ? string.Empty : validation.ErrorMessage;

                return validation;
            }
            catch (Exception ex)
            {
                HasError = true;
                ValidationError = ex.Message;
                return ValidationResult.Failure(ex.Message);
            }
        }

        /// <summary>
        /// Отримує значення у правильному типі
        /// </summary>
        public object? GetValue()
        {
            if (string.IsNullOrWhiteSpace(InputValue))
                return null;

            try
            {
                return _column.ConvertValue(InputValue);
            }
            catch
            {
                return InputValue;
            }
        }

        /// <summary>
        /// Форматує значення для відображення
        /// </summary>
        private string FormatValueForDisplay(object value)
        {
            if (value == null)
                return string.Empty;

            return DataType switch
            {
                DataType.Money when value is MoneyValue mv => mv.Amount.ToString("F2"),
                DataType.MoneyInterval when value is MoneyIntervalValue miv =>
                    $"{miv.From.Amount:F2}-{miv.To.Amount:F2}",
                DataType.Real when value is double d => d.ToString("F2"),
                DataType.Char when value is char c => c.ToString(),
                _ => value.ToString() ?? string.Empty
            };
        }

        /// <summary>
        /// Отримує читабельну назву типу даних
        /// </summary>
        private string GetDataTypeDisplayName(DataType dataType)
        {
            return dataType switch
            {
                DataType.Integer => "Ціле число",
                DataType.Real => "Дійсне число",
                DataType.Char => "Символ",
                DataType.String => "Рядок",
                DataType.Money => "Гроші (0.00 до 10,000,000,000,000.00)",
                DataType.MoneyInterval => "Інтервал грошей (100.00-500.00)",
                _ => dataType.ToString()
            };
        }

        #endregion

        #region PropertyChanged handlers

        partial void OnInputValueChanged(string value)
        {
            // Валідуємо при зміні значення
            Validate();
        }

        #endregion
    }
}