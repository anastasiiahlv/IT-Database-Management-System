using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DatabaseCore.Models
{
    public class Table
    {
        [JsonPropertyName("name")]
        public string Name { get; private set; }

        [JsonPropertyName("columns")]
        public List<Column> Columns { get; private set; }

        [JsonPropertyName("rows")]
        public List<Row> Rows { get; private set; }

        [JsonConstructor]
        public Table(string name, List<Column> columns, List<Row> rows)
        {
            Name = name;
            Columns = columns ?? new List<Column>();
            Rows = rows ?? new List<Row>();
        }

        public Table(string name, List<Column> columns)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Назва таблиці не може бути порожньою", nameof(name));

            if (columns == null || columns.Count == 0)
                throw new ArgumentException("Таблиця має містити хоча б одну колонку", nameof(columns));

            // Перевіряємо унікальність назв колонок
            var duplicateColumns = columns.GroupBy(c => c.Name)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateColumns.Any())
                throw new ArgumentException($"Дубльовані назви колонок: {string.Join(", ", duplicateColumns)}");

            Name = name.Trim();
            Columns = columns;
            Rows = new List<Row>();
        }

        /// <summary>
        /// Додає новий рядок до таблиці
        /// </summary>
        public void AddRow(Row row)
        {
            if (row == null)
                throw new ArgumentNullException(nameof(row));

            if (!row.ValidateAgainstSchema(Columns))
                throw new ArgumentException("Рядок не відповідає схемі таблиці");

            Rows.Add(row);
        }

        /// <summary>
        /// Додає новий рядок зі значеннями
        /// </summary>
        public Row AddRow(Dictionary<string, object?> values)
        {
            var row = new Row(Columns);

            foreach (var kvp in values)
            {
                var column = Columns.FirstOrDefault(c => c.Name == kvp.Key);
                if (column == null)
                    throw new ArgumentException($"Колонка '{kvp.Key}' не існує в таблиці");

                row.SetValue(kvp.Key, kvp.Value, column);
            }

            Rows.Add(row);
            return row;
        }

        /// <summary>
        /// Оновлює існуючий рядок
        /// </summary>
        public void UpdateRow(Guid rowId, Dictionary<string, object?> values)
        {
            var row = Rows.FirstOrDefault(r => r.Id == rowId);
            if (row == null)
                throw new ArgumentException($"Рядок з ID {rowId} не знайдено");

            foreach (var kvp in values)
            {
                var column = Columns.FirstOrDefault(c => c.Name == kvp.Key);
                if (column == null)
                    throw new ArgumentException($"Колонка '{kvp.Key}' не існує в таблиці");

                row.SetValue(kvp.Key, kvp.Value, column);
            }
        }

        /// <summary>
        /// Видаляє рядок з таблиці
        /// </summary>
        public bool DeleteRow(Guid rowId)
        {
            var row = Rows.FirstOrDefault(r => r.Id == rowId);
            if (row == null)
                return false;

            return Rows.Remove(row);
        }

        /// <summary>
        /// Отримує рядок за ID
        /// </summary>
        public Row? GetRow(Guid rowId)
        {
            return Rows.FirstOrDefault(r => r.Id == rowId);
        }

        /// <summary>
        /// Отримує колонку за назвою
        /// </summary>
        public Column? GetColumn(string columnName)
        {
            return Columns.FirstOrDefault(c => c.Name == columnName);
        }

        /// <summary>
        /// Сортує таблицю за вказаною колонкою (ІНДИВІДУАЛЬНА ОПЕРАЦІЯ)
        /// </summary>
        public void Sort(string columnName, bool ascending = true)
        {
            var column = GetColumn(columnName);
            if (column == null)
                throw new ArgumentException($"Колонка '{columnName}' не існує в таблиці");

            Rows = ascending
                ? Rows.OrderBy(r => r.GetValue(columnName), Comparer<object?>.Create(CompareValues)).ToList()
                : Rows.OrderByDescending(r => r.GetValue(columnName), Comparer<object?>.Create(CompareValues)).ToList();
        }

        /// <summary>
        /// Порівнює значення різних типів для сортування
        /// </summary>
        private int CompareValues(object? x, object? y)
        {
            // Null завжди менше за будь-яке значення
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            // Порівнюємо за типами
            if (x is IComparable comparable && x.GetType() == y.GetType())
            {
                return comparable.CompareTo(y);
            }

            // Порівнюємо MoneyValue
            if (x is MoneyValue mx && y is MoneyValue my)
            {
                return mx.CompareTo(my);
            }

            // Для інших типів порівнюємо як рядки
            return string.Compare(x.ToString(), y.ToString(), StringComparison.Ordinal);
        }

        /// <summary>
        /// Очищує всі рядки з таблиці
        /// </summary>
        public void Clear()
        {
            Rows.Clear();
        }

        /// <summary>
        /// Повертає кількість рядків у таблиці
        /// </summary>
        public int RowCount => Rows.Count;

        /// <summary>
        /// Повертає кількість колонок у таблиці
        /// </summary>
        public int ColumnCount => Columns.Count;

        public override string ToString()
        {
            return $"Table '{Name}': {ColumnCount} columns, {RowCount} rows";
        }
    }
}
