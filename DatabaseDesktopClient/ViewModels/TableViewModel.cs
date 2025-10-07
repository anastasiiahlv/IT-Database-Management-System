using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DatabaseDesktopClient.Services;
using DatabaseDesktopClient.Views;
using DatabaseCore.Models;

namespace DatabaseDesktopClient.ViewModels
{
    public partial class TableViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;
        private readonly string _tableName;
        private Table _table;

        public TableViewModel(DatabaseService databaseService, string tableName)
        {
            _databaseService = databaseService;
            _tableName = tableName;
            _table = _databaseService.GetTable(tableName);

            Rows = new ObservableCollection<Row>();
            SortColumns = new ObservableCollection<string>();

            LoadTable();
        }

        #region Властивості

        [ObservableProperty]
        private string _tableNameDisplay = string.Empty;

        [ObservableProperty]
        private ObservableCollection<Row> _rows;

        [ObservableProperty]
        private Row? _selectedRow;

        [ObservableProperty]
        private ObservableCollection<string> _sortColumns;

        [ObservableProperty]
        private string? _selectedSortColumn;

        [ObservableProperty]
        private bool _sortAscending = true;

        [ObservableProperty]
        private int _rowCount;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        public ObservableCollection<Column> Columns => new ObservableCollection<Column>(_table.Columns);

        #endregion

        #region Команди - CRUD операції

        /// <summary>
        /// Команда додавання нового рядка
        /// </summary>
        [RelayCommand]
        private void AddRow()
        {
            try
            {
                var viewModel = new RowEditViewModel(_table, null);
                var dialog = new RowEditDialog(_table, null)  // ⬅️ Додали _table
                {
                    DataContext = viewModel,
                    Owner = Application.Current.MainWindow
                };

                if (dialog.ShowDialog() == true)
                {
                    var rowData = viewModel.GetRowData();
                    _databaseService.AddRow(_tableName, rowData);
                    LoadTable();
                    StatusMessage = "Рядок додано";
                }
            }
            catch (Exception ex)
            {
                ShowError($"Помилка додавання рядка: {ex.Message}");
            }
        }

        /// <summary>
        /// Команда редагування рядка
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanEditRow))]
        private void EditRow()
        {
            try
            {
                if (SelectedRow == null)
                    return;

                var viewModel = new RowEditViewModel(_table, SelectedRow);
                var dialog = new RowEditDialog(_table, SelectedRow)  // ⬅️ Додали _table
                {
                    DataContext = viewModel,
                    Owner = Application.Current.MainWindow
                };

                if (dialog.ShowDialog() == true)
                {
                    var rowData = viewModel.GetRowData();
                    _databaseService.UpdateRow(_tableName, SelectedRow.Id, rowData);
                    LoadTable();
                    StatusMessage = "Рядок оновлено";
                }
            }
            catch (Exception ex)
            {
                ShowError($"Помилка редагування рядка: {ex.Message}");
            }
        }

        private bool CanEditRow() => SelectedRow != null;

        /// <summary>
        /// Команда видалення рядка
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanDeleteRow))]
        private void DeleteRow()
        {
            try
            {
                if (SelectedRow == null)
                    return;

                var result = MessageBox.Show(
                    "Видалити вибраний рядок?",
                    "Підтвердження видалення",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _databaseService.DeleteRow(_tableName, SelectedRow.Id);
                    LoadTable();
                    StatusMessage = "Рядок видалено";
                }
            }
            catch (Exception ex)
            {
                ShowError($"Помилка видалення рядка: {ex.Message}");
            }
        }

        private bool CanDeleteRow() => SelectedRow != null;

        #endregion

        #region Команди - Сортування ⭐ (ІНДИВІДУАЛЬНА ОПЕРАЦІЯ)

        /// <summary>
        /// Команда сортування таблиці за вибраною колонкою
        /// ІНДИВІДУАЛЬНА ОПЕРАЦІЯ ⭐
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanSort))]
        private void Sort()
        {
            try
            {
                if (string.IsNullOrEmpty(SelectedSortColumn))
                    return;

                // Виконуємо сортування через DatabaseService
                _databaseService.SortTable(_tableName, SelectedSortColumn, SortAscending);

                // Оновлюємо відображення
                LoadTable();

                var direction = SortAscending ? "за зростанням" : "за спаданням";
                StatusMessage = $"Таблиця відсортована за колонкою '{SelectedSortColumn}' {direction}";
            }
            catch (Exception ex)
            {
                ShowError($"Помилка сортування: {ex.Message}");
            }
        }

        private bool CanSort() => !string.IsNullOrEmpty(SelectedSortColumn);

        /// <summary>
        /// Команда перемикання напрямку сортування
        /// </summary>
        [RelayCommand]
        private void ToggleSortDirection()
        {
            SortAscending = !SortAscending;

            if (!string.IsNullOrEmpty(SelectedSortColumn))
            {
                Sort();
            }
        }

        #endregion

        #region Команди - Інше

        /// <summary>
        /// Команда оновлення даних таблиці
        /// </summary>
        [RelayCommand]
        private void Refresh()
        {
            LoadTable();
            StatusMessage = "Таблиця оновлена";
        }

        #endregion

        #region Методи

        /// <summary>
        /// Завантажує дані таблиці
        /// </summary>
        private void LoadTable()
        {
            try
            {
                // Оновлюємо посилання на таблицю
                _table = _databaseService.GetTable(_tableName);

                TableNameDisplay = _table.Name;

                // Завантажуємо рядки
                Rows.Clear();
                foreach (var row in _table.Rows)
                {
                    Rows.Add(row);
                }

                RowCount = Rows.Count;

                // Завантажуємо список колонок для сортування
                SortColumns.Clear();
                foreach (var column in _table.Columns)
                {
                    SortColumns.Add(column.Name);
                }

                // Оновлюємо властивості
                OnPropertyChanged(nameof(Columns));
            }
            catch (Exception ex)
            {
                ShowError($"Помилка завантаження таблиці: {ex.Message}");
            }
        }

        /// <summary>
        /// Показує повідомлення про помилку
        /// </summary>
        private void ShowError(string message)
        {
            MessageBox.Show(message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = $"Помилка: {message}";
        }

        #endregion

        #region PropertyChanged handlers

        partial void OnSelectedRowChanged(Row? value)
        {
            EditRowCommand.NotifyCanExecuteChanged();
            DeleteRowCommand.NotifyCanExecuteChanged();
        }

        partial void OnSelectedSortColumnChanged(string? value)
        {
            SortCommand.NotifyCanExecuteChanged();
        }

        #endregion
    }
}