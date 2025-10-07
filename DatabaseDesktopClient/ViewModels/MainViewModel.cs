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
using Microsoft.Win32;

namespace DatabaseDesktopClient.ViewModels
{
    /// <summary>
    /// Головний ViewModel для MainWindow
    /// Управляє базою даних, списком таблиць та навігацією
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;

        public MainViewModel()
        {
            _databaseService = new DatabaseService();

            // Підписуємося на події DatabaseService
            _databaseService.DatabaseChanged += OnDatabaseChanged;
            _databaseService.DatabaseClosed += OnDatabaseClosed;
            _databaseService.TableAdded += OnTableAdded;
            _databaseService.TableDeleted += OnTableDeleted;

            Tables = new ObservableCollection<string>();
        }

        #region Властивості

        [ObservableProperty]
        private string _databaseName = "Немає відкритої бази даних";

        [ObservableProperty]
        private bool _isDatabaseOpen;

        [ObservableProperty]
        private string? _currentFilePath;

        [ObservableProperty]
        private ObservableCollection<string> _tables;

        [ObservableProperty]
        private string? _selectedTable;

        [ObservableProperty]
        private object? _currentView;

        [ObservableProperty]
        private string _statusMessage = "Готово";

        // Статистика
        [ObservableProperty]
        private int _tableCount;

        [ObservableProperty]
        private int _totalRowCount;

        #endregion

        #region Команди - Операції з базою даних

        /// <summary>
        /// Команда створення нової бази даних
        /// </summary>
        [RelayCommand]
        private void CreateDatabase()
        {
            try
            {
                // Діалог введення назви бази
                var dialog = new InputDialog("Створити нову базу даних", "Введіть назву бази даних:");
                if (dialog.ShowDialog() == true)
                {
                    var dbName = dialog.InputText;
                    if (string.IsNullOrWhiteSpace(dbName))
                    {
                        ShowError("Назва бази даних не може бути порожньою");
                        return;
                    }

                    _databaseService.CreateDatabase(dbName);
                    StatusMessage = $"База даних '{dbName}' створена";
                }
            }
            catch (Exception ex)
            {
                ShowError($"Помилка створення бази даних: {ex.Message}");
            }
        }

        /// <summary>
        /// Команда відкриття існуючої бази даних
        /// </summary>
        [RelayCommand]
        private void OpenDatabase()
        {
            try
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "Database Files (*.json)|*.json|All Files (*.*)|*.*",
                    Title = "Відкрити базу даних"
                };

                if (dialog.ShowDialog() == true)
                {
                    _databaseService.LoadDatabase(dialog.FileName);
                    CurrentFilePath = dialog.FileName;
                    StatusMessage = $"База даних завантажена з '{dialog.FileName}'";
                }
            }
            catch (Exception ex)
            {
                ShowError($"Помилка відкриття бази даних: {ex.Message}");
            }
        }

        /// <summary>
        /// Команда збереження бази даних
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanSaveDatabase))]
        private void SaveDatabase()
        {
            try
            {
                if (string.IsNullOrEmpty(CurrentFilePath))
                {
                    SaveDatabaseAs();
                    return;
                }

                _databaseService.SaveDatabase();
                StatusMessage = $"База даних збережена у '{CurrentFilePath}'";
            }
            catch (Exception ex)
            {
                ShowError($"Помилка збереження: {ex.Message}");
            }
        }

        private bool CanSaveDatabase() => IsDatabaseOpen;

        /// <summary>
        /// Команда збереження бази даних як...
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanSaveDatabase))]
        private void SaveDatabaseAs()
        {
            try
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "Database Files (*.json)|*.json|All Files (*.*)|*.*",
                    Title = "Зберегти базу даних",
                    FileName = $"{DatabaseName}.json"
                };

                if (dialog.ShowDialog() == true)
                {
                    _databaseService.SaveDatabase(dialog.FileName);
                    CurrentFilePath = dialog.FileName;
                    StatusMessage = $"База даних збережена у '{dialog.FileName}'";
                }
            }
            catch (Exception ex)
            {
                ShowError($"Помилка збереження: {ex.Message}");
            }
        }

        /// <summary>
        /// Команда закриття бази даних
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanCloseDatabase))]
        private void CloseDatabase()
        {
            try
            {
                var result = MessageBox.Show(
                    "Закрити поточну базу даних? Незбережені зміни будуть втрачені.",
                    "Підтвердження",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _databaseService.CloseDatabase();
                    StatusMessage = "База даних закрита";
                }
            }
            catch (Exception ex)
            {
                ShowError($"Помилка закриття бази даних: {ex.Message}");
            }
        }

        private bool CanCloseDatabase() => IsDatabaseOpen;

        #endregion

        #region Команди - Операції з таблицями

        /// <summary>
        /// Команда створення нової таблиці
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanCreateTable))]
        private void CreateTable()
        {
            try
            {
                // Відкриваємо діалог створення таблиці з DatabaseService
                var dialog = new CreateTableDialog(_databaseService)
                {
                    Owner = Application.Current.MainWindow
                };

                if (dialog.ShowDialog() == true)
                {
                    StatusMessage = $"Таблиця '{dialog.TableName}' створена";
                }
            }
            catch (Exception ex)
            {
                ShowError($"Помилка створення таблиці: {ex.Message}");
            }
        }

        private bool CanCreateTable() => IsDatabaseOpen;

        /// <summary>
        /// Команда видалення таблиці
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanDeleteTable))]
        private void DeleteTable()
        {
            try
            {
                if (string.IsNullOrEmpty(SelectedTable))
                    return;

                var result = MessageBox.Show(
                    $"Видалити таблицю '{SelectedTable}'? Всі дані будуть втрачені.",
                    "Підтвердження видалення",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    _databaseService.DeleteTable(SelectedTable);
                    SelectedTable = null;
                    CurrentView = null;
                    StatusMessage = "Таблиця видалена";
                }
            }
            catch (Exception ex)
            {
                ShowError($"Помилка видалення таблиці: {ex.Message}");
            }
        }

        private bool CanDeleteTable() => IsDatabaseOpen && !string.IsNullOrEmpty(SelectedTable);

        /// <summary>
        /// Команда відкриття таблиці для перегляду/редагування
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanOpenTable))]
        private void OpenTable()
        {
            try
            {
                if (string.IsNullOrEmpty(SelectedTable))
                    return;

                // Створюємо ViewModel для таблиці
                var tableViewModel = new TableViewModel(_databaseService, SelectedTable);
                CurrentView = tableViewModel;
                StatusMessage = $"Відкрито таблицю '{SelectedTable}'";
            }
            catch (Exception ex)
            {
                ShowError($"Помилка відкриття таблиці: {ex.Message}");
            }
        }

        private bool CanOpenTable() => IsDatabaseOpen && !string.IsNullOrEmpty(SelectedTable);

        #endregion

        #region Команди - Інше

        /// <summary>
        /// Команда показу інформації про базу даних
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanShowInfo))]
        private void ShowDatabaseInfo()
        {
            try
            {
                var stats = _databaseService.GetStatistics();
                var info = $"Назва: {stats.DatabaseName}\n" +
                          $"Таблиць: {stats.TableCount}\n" +
                          $"Всього рядків: {stats.TotalRowCount}\n" +
                          $"Створено: {stats.CreatedAt:dd.MM.yyyy HH:mm:ss}\n" +
                          $"Змінено: {stats.ModifiedAt:dd.MM.yyyy HH:mm:ss}\n" +
                          $"Файл: {stats.FilePath ?? "не збережено"}";

                MessageBox.Show(info, "Інформація про базу даних", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowError($"Помилка: {ex.Message}");
            }
        }

        private bool CanShowInfo() => IsDatabaseOpen;

        /// <summary>
        /// Команда виходу з програми
        /// </summary>
        [RelayCommand]
        private void Exit()
        {
            Application.Current.Shutdown();
        }

        #endregion

        #region Обробники подій DatabaseService

        private void OnDatabaseChanged(object? sender, EventArgs e)
        {
            DatabaseName = _databaseService.DatabaseName ?? "Невідома база";
            IsDatabaseOpen = _databaseService.HasOpenDatabase;

            LoadTables();
            UpdateStatistics();

            // Показуємо DatabaseView при відкритті бази
            if (IsDatabaseOpen)
            {
                CurrentView = new DatabaseViewModel(_databaseService);
            }
        }

        private void OnDatabaseClosed(object? sender, EventArgs e)
        {
            DatabaseName = "Немає відкритої бази даних";
            IsDatabaseOpen = false;
            CurrentFilePath = null;
            Tables.Clear();
            SelectedTable = null;
            CurrentView = null;
            TableCount = 0;
            TotalRowCount = 0;
        }

        private void OnTableAdded(object? sender, TableEventArgs e)
        {
            LoadTables();
            UpdateStatistics();
        }

        private void OnTableDeleted(object? sender, TableEventArgs e)
        {
            LoadTables();
            UpdateStatistics();
        }

        #endregion

        #region Допоміжні методи

        /// <summary>
        /// Завантажує список таблиць з бази даних
        /// </summary>
        private void LoadTables()
        {
            Tables.Clear();
            var tableNames = _databaseService.GetTableNames();
            foreach (var name in tableNames)
            {
                Tables.Add(name);
            }
        }

        /// <summary>
        /// Оновлює статистику бази даних
        /// </summary>
        private void UpdateStatistics()
        {
            if (!IsDatabaseOpen)
            {
                TableCount = 0;
                TotalRowCount = 0;
                return;
            }

            try
            {
                var stats = _databaseService.GetStatistics();
                TableCount = stats.TableCount;
                TotalRowCount = stats.TotalRowCount;
            }
            catch
            {
                TableCount = 0;
                TotalRowCount = 0;
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

        partial void OnSelectedTableChanged(string? value)
        {
            // Оновлюємо CanExecute для команд
            DeleteTableCommand.NotifyCanExecuteChanged();
            OpenTableCommand.NotifyCanExecuteChanged();
        }

        partial void OnIsDatabaseOpenChanged(bool value)
        {
            // Оновлюємо CanExecute для всіх команд
            SaveDatabaseCommand.NotifyCanExecuteChanged();
            SaveDatabaseAsCommand.NotifyCanExecuteChanged();
            CloseDatabaseCommand.NotifyCanExecuteChanged();
            CreateTableCommand.NotifyCanExecuteChanged();
            DeleteTableCommand.NotifyCanExecuteChanged();
            OpenTableCommand.NotifyCanExecuteChanged();
            ShowDatabaseInfoCommand.NotifyCanExecuteChanged();
        }

        #endregion
    }
}