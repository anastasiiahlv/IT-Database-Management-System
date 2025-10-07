using DatabaseCore.Managers;
using DatabaseCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseDesktopClient.Services
{
    public class DatabaseService
    {
        private readonly DatabaseManager _databaseManager;

        // Events для повідомлення UI про зміни
        public event EventHandler? DatabaseChanged;
        public event EventHandler? DatabaseClosed;
        public event EventHandler<TableEventArgs>? TableAdded;
        public event EventHandler<TableEventArgs>? TableDeleted;
        public event EventHandler<TableEventArgs>? TableChanged;

        public DatabaseService()
        {
            _databaseManager = new DatabaseManager();
        }

        #region Властивості

        /// <summary>
        /// Чи є відкрита база даних
        /// </summary>
        public bool HasOpenDatabase => _databaseManager.HasOpenDatabase;

        /// <summary>
        /// Поточна база даних
        /// </summary>
        public Database? CurrentDatabase => _databaseManager.CurrentDatabase;

        /// <summary>
        /// Шлях до файлу поточної бази даних
        /// </summary>
        public string? CurrentFilePath => _databaseManager.CurrentFilePath;

        /// <summary>
        /// Назва поточної бази даних
        /// </summary>
        public string? DatabaseName => CurrentDatabase?.Name;

        #endregion

        #region Операції з базою даних

        /// <summary>
        /// Створює нову базу даних
        /// </summary>
        public Database CreateDatabase(string databaseName)
        {
            if (string.IsNullOrWhiteSpace(databaseName))
                throw new ArgumentException("Назва бази даних не може бути порожньою");

            var database = _databaseManager.CreateDatabase(databaseName);
            OnDatabaseChanged();
            return database;
        }

        /// <summary>
        /// Зберігає поточну базу даних у файл
        /// </summary>
        public void SaveDatabase(string filePath)
        {
            if (!HasOpenDatabase)
                throw new InvalidOperationException("Немає відкритої бази даних для збереження");

            _databaseManager.SaveDatabase(filePath);
        }

        /// <summary>
        /// Зберігає поточну базу даних (у поточний файл)
        /// </summary>
        public void SaveDatabase()
        {
            if (!HasOpenDatabase)
                throw new InvalidOperationException("Немає відкритої бази даних для збереження");

            if (string.IsNullOrEmpty(CurrentFilePath))
                throw new InvalidOperationException("Файл для збереження не вказано. Використайте 'Save As'");

            _databaseManager.SaveDatabase();
        }

        /// <summary>
        /// Завантажує базу даних з файлу
        /// </summary>
        public Database LoadDatabase(string filePath)
        {
            var database = _databaseManager.LoadDatabase(filePath);
            OnDatabaseChanged();
            return database;
        }

        /// <summary>
        /// Закриває поточну базу даних
        /// </summary>
        public void CloseDatabase()
        {
            _databaseManager.CloseDatabase();
            OnDatabaseClosed();
        }

        /// <summary>
        /// Створює резервну копію поточної бази даних
        /// </summary>
        public void CreateBackup()
        {
            if (!HasOpenDatabase)
                throw new InvalidOperationException("Немає відкритої бази даних");

            _databaseManager.CreateBackup();
        }

        /// <summary>
        /// Отримує статистику по поточній базі даних
        /// </summary>
        public DatabaseStatistics GetStatistics()
        {
            if (!HasOpenDatabase)
                throw new InvalidOperationException("Немає відкритої бази даних");

            return _databaseManager.GetStatistics();
        }

        #endregion

        #region Операції з таблицями

        /// <summary>
        /// Створює нову таблицю в поточній базі даних
        /// </summary>
        public Table CreateTable(string tableName, List<Column> columns)
        {
            if (!HasOpenDatabase)
                throw new InvalidOperationException("Немає відкритої бази даних");

            var table = _databaseManager.CreateTable(tableName, columns);
            OnTableAdded(table);
            return table;
        }

        /// <summary>
        /// Видаляє таблицю з поточної бази даних
        /// </summary>
        public bool DeleteTable(string tableName)
        {
            if (!HasOpenDatabase)
                throw new InvalidOperationException("Немає відкритої бази даних");

            var result = _databaseManager.DeleteTable(tableName);
            if (result)
            {
                OnTableDeleted(tableName);
            }
            return result;
        }

        /// <summary>
        /// Отримує таблицю за назвою
        /// </summary>
        public Table GetTable(string tableName)
        {
            if (!HasOpenDatabase)
                throw new InvalidOperationException("Немає відкритої бази даних");

            return _databaseManager.GetTable(tableName);
        }

        /// <summary>
        /// Отримує всі таблиці з поточної бази даних
        /// </summary>
        public List<Table> GetAllTables()
        {
            if (!HasOpenDatabase)
                return new List<Table>();

            return _databaseManager.GetAllTables();
        }

        /// <summary>
        /// Отримує список назв всіх таблиць
        /// </summary>
        public List<string> GetTableNames()
        {
            if (!HasOpenDatabase)
                return new List<string>();

            return _databaseManager.GetTableNames();
        }

        /// <summary>
        /// Перевіряє, чи існує таблиця
        /// </summary>
        public bool TableExists(string tableName)
        {
            if (!HasOpenDatabase)
                return false;

            return CurrentDatabase!.TableExists(tableName);
        }

        #endregion

        #region Операції з рядками

        /// <summary>
        /// Додає новий рядок до таблиці
        /// </summary>
        public Row AddRow(string tableName, Dictionary<string, object?> values)
        {
            if (!HasOpenDatabase)
                throw new InvalidOperationException("Немає відкритої бази даних");

            var row = _databaseManager.AddRow(tableName, values);
            OnTableChanged(tableName);
            return row;
        }

        /// <summary>
        /// Оновлює рядок у таблиці
        /// </summary>
        public void UpdateRow(string tableName, Guid rowId, Dictionary<string, object?> values)
        {
            if (!HasOpenDatabase)
                throw new InvalidOperationException("Немає відкритої бази даних");

            _databaseManager.UpdateRow(tableName, rowId, values);
            OnTableChanged(tableName);
        }

        /// <summary>
        /// Видаляє рядок з таблиці
        /// </summary>
        public bool DeleteRow(string tableName, Guid rowId)
        {
            if (!HasOpenDatabase)
                throw new InvalidOperationException("Немає відкритої бази даних");

            var result = _databaseManager.DeleteRow(tableName, rowId);
            if (result)
            {
                OnTableChanged(tableName);
            }
            return result;
        }

        /// <summary>
        /// Отримує рядок з таблиці за ID
        /// </summary>
        public Row GetRow(string tableName, Guid rowId)
        {
            if (!HasOpenDatabase)
                throw new InvalidOperationException("Немає відкритої бази даних");

            return _databaseManager.GetRow(tableName, rowId);
        }

        #endregion

        #region Сортування (індивідуальна операція)

        /// <summary>
        /// Сортує таблицю за вказаною колонкою
        /// ІНДИВІДУАЛЬНА ОПЕРАЦІЯ ⭐
        /// </summary>
        public void SortTable(string tableName, string columnName, bool ascending = true)
        {
            if (!HasOpenDatabase)
                throw new InvalidOperationException("Немає відкритої бази даних");

            _databaseManager.SortTable(tableName, columnName, ascending);
            OnTableChanged(tableName);
        }

        #endregion

        #region Event Handlers

        protected virtual void OnDatabaseChanged()
        {
            DatabaseChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnDatabaseClosed()
        {
            DatabaseClosed?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnTableAdded(Table table)
        {
            TableAdded?.Invoke(this, new TableEventArgs(table.Name));
        }

        protected virtual void OnTableDeleted(string tableName)
        {
            TableDeleted?.Invoke(this, new TableEventArgs(tableName));
        }

        protected virtual void OnTableChanged(string tableName)
        {
            TableChanged?.Invoke(this, new TableEventArgs(tableName));
        }

        #endregion

        #region Допоміжні методи

        /// <summary>
        /// Валідує дані перед додаванням/оновленням рядка
        /// </summary>
        public Dictionary<string, string> ValidateRowData(string tableName, Dictionary<string, object?> values)
        {
            var errors = new Dictionary<string, string>();

            if (!HasOpenDatabase)
            {
                errors.Add("Database", "Немає відкритої бази даних");
                return errors;
            }

            try
            {
                var table = GetTable(tableName);

                foreach (var kvp in values)
                {
                    var column = table.GetColumn(kvp.Key);
                    if (column == null)
                    {
                        errors.Add(kvp.Key, $"Колонка '{kvp.Key}' не існує");
                        continue;
                    }

                    if (!column.IsValidValue(kvp.Value))
                    {
                        errors.Add(kvp.Key, $"Невалідне значення для типу {column.DataType}");
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add("General", ex.Message);
            }

            return errors;
        }

        /// <summary>
        /// Отримує кількість рядків у таблиці
        /// </summary>
        public int GetRowCount(string tableName)
        {
            if (!HasOpenDatabase)
                return 0;

            try
            {
                var table = GetTable(tableName);
                return table.RowCount;
            }
            catch
            {
                return 0;
            }
        }

        #endregion
    }

    /// <summary>
    /// Аргументи події для операцій з таблицями
    /// </summary>
    public class TableEventArgs : EventArgs
    {
        public string TableName { get; }

        public TableEventArgs(string tableName)
        {
            TableName = tableName;
        }
    }
}
