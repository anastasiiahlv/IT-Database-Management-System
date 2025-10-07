using DatabaseCore.Exceptions;
using DatabaseCore.Models;
using DatabaseCore.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseCore.Managers
{
    /// <summary>
    /// Головний менеджер для управління базою даних
    /// </summary>
    public class DatabaseManager
    {
        private Database? _currentDatabase;
        private string? _currentFilePath;

        /// <summary>
        /// Поточна база даних
        /// </summary>
        public Database? CurrentDatabase => _currentDatabase;

        /// <summary>
        /// Шлях до файлу поточної бази даних
        /// </summary>
        public string? CurrentFilePath => _currentFilePath;

        /// <summary>
        /// Чи є відкрита база даних
        /// </summary>
        public bool HasOpenDatabase => _currentDatabase != null;

        /// <summary>
        /// Створює нову базу даних
        /// </summary>
        public Database CreateDatabase(string databaseName)
        {
            if (string.IsNullOrWhiteSpace(databaseName))
                throw new ArgumentException("Назва бази даних не може бути порожньою", nameof(databaseName));

            var database = new Database(databaseName.Trim());
            _currentDatabase = database;
            _currentFilePath = null; // Новій базі ще не призначено файл

            return database;
        }

        /// <summary>
        /// Зберігає поточну базу даних у файл
        /// </summary>
        public void SaveDatabase(string filePath)
        {
            if (_currentDatabase == null)
                throw new InvalidOperationException("Немає відкритої бази даних для збереження");

            SerializationService.SaveToFile(_currentDatabase, filePath);
            _currentFilePath = filePath;
        }

        /// <summary>
        /// Зберігає поточну базу даних (у поточний файл)
        /// </summary>
        public void SaveDatabase()
        {
            if (_currentDatabase == null)
                throw new InvalidOperationException("Немає відкритої бази даних для збереження");

            if (string.IsNullOrEmpty(_currentFilePath))
                throw new InvalidOperationException("Файл для збереження не вказано. Використайте SaveDatabase(filePath)");

            SerializationService.SaveToFile(_currentDatabase, _currentFilePath);
        }

        /// <summary>
        /// Завантажує базу даних з файлу
        /// </summary>
        public Database LoadDatabase(string filePath)
        {
            var database = SerializationService.LoadFromFile(filePath);
            _currentDatabase = database;
            _currentFilePath = filePath;

            return database;
        }

        /// <summary>
        /// Закриває поточну базу даних
        /// </summary>
        public void CloseDatabase()
        {
            _currentDatabase = null;
            _currentFilePath = null;
        }

        /// <summary>
        /// Створює резервну копію поточної бази даних
        /// </summary>
        public void CreateBackup()
        {
            if (_currentDatabase == null)
                throw new InvalidOperationException("Немає відкритої бази даних для створення резервної копії");

            if (string.IsNullOrEmpty(_currentFilePath))
                throw new InvalidOperationException("Файл бази даних не вказано");

            SerializationService.CreateBackup(_currentDatabase, _currentFilePath);
        }

        #region Операції з таблицями

        /// <summary>
        /// Створює нову таблицю в поточній базі даних
        /// </summary>
        public Table CreateTable(string tableName, List<Column> columns)
        {
            EnsureDatabaseOpen();

            // Валідуємо назву таблиці
            var nameValidation = ValidationService.ValidateTableName(tableName);
            nameValidation.ThrowIfInvalid();

            // Валідуємо колонки
            if (columns == null || columns.Count == 0)
                throw new ValidationException("Таблиця має містити хоча б одну колонку");

            foreach (var column in columns)
            {
                var columnValidation = ValidationService.ValidateColumn(column);
                columnValidation.ThrowIfInvalid();
            }

            // Валідуємо унікальність назв колонок
            var uniqueValidation = ValidationService.ValidateUniqueColumnNames(columns);
            uniqueValidation.ThrowIfInvalid();

            // Створюємо таблицю
            var table = new Table(tableName, columns);
            _currentDatabase!.CreateTable(table);

            return table;
        }

        /// <summary>
        /// Видаляє таблицю з поточної бази даних
        /// </summary>
        public bool DeleteTable(string tableName)
        {
            EnsureDatabaseOpen();
            return _currentDatabase!.DeleteTable(tableName);
        }

        /// <summary>
        /// Отримує таблицю за назвою
        /// </summary>
        public Table GetTable(string tableName)
        {
            EnsureDatabaseOpen();

            var table = _currentDatabase!.GetTable(tableName);
            if (table == null)
                throw new TableNotFoundException(tableName);

            return table;
        }

        /// <summary>
        /// Отримує всі таблиці з поточної бази даних
        /// </summary>
        public List<Table> GetAllTables()
        {
            EnsureDatabaseOpen();
            return _currentDatabase!.GetAllTables();
        }

        /// <summary>
        /// Отримує список назв всіх таблиць
        /// </summary>
        public List<string> GetTableNames()
        {
            EnsureDatabaseOpen();
            return _currentDatabase!.GetTableNames();
        }

        #endregion

        #region Операції з рядками

        /// <summary>
        /// Додає новий рядок до таблиці
        /// </summary>
        public Row AddRow(string tableName, Dictionary<string, object?> values)
        {
            var table = GetTable(tableName);

            // Валідуємо значення
            foreach (var kvp in values)
            {
                var column = table.GetColumn(kvp.Key);
                if (column == null)
                    throw new ColumnNotFoundException(kvp.Key);

                var validation = ValidationService.ValidateValue(kvp.Value, column.DataType);
                validation.ThrowIfInvalid();
            }

            return table.AddRow(values);
        }

        /// <summary>
        /// Оновлює рядок у таблиці
        /// </summary>
        public void UpdateRow(string tableName, Guid rowId, Dictionary<string, object?> values)
        {
            var table = GetTable(tableName);

            // Валідуємо значення
            foreach (var kvp in values)
            {
                var column = table.GetColumn(kvp.Key);
                if (column == null)
                    throw new ColumnNotFoundException(kvp.Key);

                var validation = ValidationService.ValidateValue(kvp.Value, column.DataType);
                validation.ThrowIfInvalid();
            }

            table.UpdateRow(rowId, values);
        }

        /// <summary>
        /// Видаляє рядок з таблиці
        /// </summary>
        public bool DeleteRow(string tableName, Guid rowId)
        {
            var table = GetTable(tableName);
            return table.DeleteRow(rowId);
        }

        /// <summary>
        /// Отримує рядок з таблиці за ID
        /// </summary>
        public Row GetRow(string tableName, Guid rowId)
        {
            var table = GetTable(tableName);
            var row = table.GetRow(rowId);

            if (row == null)
                throw new RowNotFoundException(rowId);

            return row;
        }

        #endregion

        #region Сортування (індивідуальна операція)

        /// <summary>
        /// Сортує таблицю за вказаною колонкою
        /// </summary>
        public void SortTable(string tableName, string columnName, bool ascending = true)
        {
            var table = GetTable(tableName);

            // Перевіряємо, що колонка існує
            var column = table.GetColumn(columnName);
            if (column == null)
                throw new ColumnNotFoundException(columnName);

            // Виконуємо сортування
            table.Sort(columnName, ascending);
        }

        #endregion

        #region Допоміжні методи

        /// <summary>
        /// Перевіряє, чи є відкрита база даних, інакше викидає виняток
        /// </summary>
        private void EnsureDatabaseOpen()
        {
            if (_currentDatabase == null)
                throw new InvalidOperationException("Немає відкритої бази даних. Створіть нову або завантажте існуючу");
        }

        /// <summary>
        /// Отримує статистику по поточній базі даних
        /// </summary>
        public DatabaseStatistics GetStatistics()
        {
            EnsureDatabaseOpen();

            return new DatabaseStatistics
            {
                DatabaseName = _currentDatabase!.Name,
                TableCount = _currentDatabase.TableCount,
                TotalRowCount = _currentDatabase.TotalRowCount,
                CreatedAt = _currentDatabase.CreatedAt,
                ModifiedAt = _currentDatabase.ModifiedAt,
                FilePath = _currentFilePath
            };
        }

        #endregion
    }

    /// <summary>
    /// Статистика бази даних
    /// </summary>
    public class DatabaseStatistics
    {
        public string DatabaseName { get; set; } = string.Empty;
        public int TableCount { get; set; }
        public int TotalRowCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public string? FilePath { get; set; }

        public override string ToString()
        {
            return $"База даних: {DatabaseName}\n" +
                   $"Таблиць: {TableCount}\n" +
                   $"Всього рядків: {TotalRowCount}\n" +
                   $"Створено: {CreatedAt:g}\n" +
                   $"Змінено: {ModifiedAt:g}\n" +
                   $"Файл: {FilePath ?? "не збережено"}";
        }
    }
}
