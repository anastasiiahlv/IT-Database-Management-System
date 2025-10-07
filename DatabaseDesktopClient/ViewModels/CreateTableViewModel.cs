using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DatabaseDesktopClient.Services;
using DatabaseCore.Models;
using DatabaseCore.Services;

namespace DatabaseDesktopClient.ViewModels
{
    /// <summary>
    /// ViewModel для діалогу створення нової таблиці
    /// </summary>
    public partial class CreateTableViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;

        public CreateTableViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            Columns = new ObservableCollection<ColumnDefinition>();

            // Заповнюємо список доступних типів даних
            AvailableDataTypes = Enum.GetValues(typeof(DataType)).Cast<DataType>().ToList();
            SelectedDataType = DataType.String;
        }

        #region Властивості

        [ObservableProperty]
        private string _tableName = string.Empty;

        [ObservableProperty]
        private ObservableCollection<ColumnDefinition> _columns;

        [ObservableProperty]
        private string _newColumnName = string.Empty;

        [ObservableProperty]
        private DataType _selectedDataType;

        [ObservableProperty]
        private ColumnDefinition? _selectedColumn;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _hasError;

        public System.Collections.Generic.List<DataType> AvailableDataTypes { get; }

        #endregion

        #region Команди

        /// <summary>
        /// Команда додавання колонки
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanAddColumn))]
        private void AddColumn()
        {
            try
            {
                // Валідація назви колонки
                var validation = ValidationService.ValidateColumnName(NewColumnName);
                if (!validation.IsValid)
                {
                    ErrorMessage = validation.ErrorMessage;
                    return;
                }

                // Перевірка на дублікат
                if (Columns.Any(c => c.Name.Equals(NewColumnName, StringComparison.OrdinalIgnoreCase)))
                {
                    ErrorMessage = "Колонка з такою назвою вже існує";
                    return;
                }

                // Додаємо колонку
                Columns.Add(new ColumnDefinition
                {
                    Name = NewColumnName.Trim(),
                    DataType = SelectedDataType
                });

                // Очищаємо поле
                NewColumnName = string.Empty;
                ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Помилка: {ex.Message}";
            }
        }

        private bool CanAddColumn() => !string.IsNullOrWhiteSpace(NewColumnName);

        /// <summary>
        /// Команда видалення колонки
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanRemoveColumn))]
        private void RemoveColumn()
        {
            if (SelectedColumn != null)
            {
                Columns.Remove(SelectedColumn);
                SelectedColumn = null;
            }
        }

        private bool CanRemoveColumn() => SelectedColumn != null;

        /// <summary>
        /// Команда створення таблиці
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanCreateTable))]
        private void CreateTable(Window window)
        {
            try
            {
                // Валідація назви таблиці
                var tableNameValidation = ValidationService.ValidateTableName(TableName);
                if (!tableNameValidation.IsValid)
                {
                    ErrorMessage = tableNameValidation.ErrorMessage;
                    return;
                }

                // Перевірка кількості колонок
                if (Columns.Count == 0)
                {
                    ErrorMessage = "Таблиця має містити хоча б одну колонку";
                    return;
                }

                // Створюємо список колонок для DatabaseCore
                var columns = Columns.Select(c => new Column(c.Name, c.DataType)).ToList();

                // Створюємо таблицю
                _databaseService.CreateTable(TableName, columns);

                // Закриваємо діалог
                window.DialogResult = true;
                window.Close();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Помилка створення таблиці: {ex.Message}";
            }
        }

        private bool CanCreateTable() => !string.IsNullOrWhiteSpace(TableName) && Columns.Count > 0;

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

        #region PropertyChanged handlers

        partial void OnNewColumnNameChanged(string value)
        {
            AddColumnCommand.NotifyCanExecuteChanged();
            ErrorMessage = string.Empty;
        }

        partial void OnSelectedColumnChanged(ColumnDefinition? value)
        {
            RemoveColumnCommand.NotifyCanExecuteChanged();
        }

        partial void OnTableNameChanged(string value)
        {
            CreateTableCommand.NotifyCanExecuteChanged();
            ErrorMessage = string.Empty;
        }

        partial void OnColumnsChanged(ObservableCollection<ColumnDefinition> value)
        {
            CreateTableCommand.NotifyCanExecuteChanged();
        }

        #endregion
    }

    /// <summary>
    /// Клас для відображення колонки в UI
    /// </summary>
    public class ColumnDefinition
    {
        public string Name { get; set; } = string.Empty;
        public DataType DataType { get; set; }

        public string DataTypeDisplay => GetDataTypeDisplayName(DataType);

        private string GetDataTypeDisplayName(DataType dataType)
        {
            return dataType switch
            {
                DataType.Integer => "Ціле число",
                DataType.Real => "Дійсне число",
                DataType.Char => "Символ",
                DataType.String => "Рядок",
                DataType.Money => "Гроші",
                DataType.MoneyInterval => "Інтервал грошей",
                _ => dataType.ToString()
            };
        }
    }
}