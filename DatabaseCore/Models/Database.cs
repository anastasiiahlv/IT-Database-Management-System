using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DatabaseCore.Models
{
    public class Database
    {
        [JsonPropertyName("name")]
        public string Name { get; private set; }

        [JsonPropertyName("tables")]
        public Dictionary<string, Table> Tables { get; private set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; private set; }

        [JsonPropertyName("modifiedAt")]
        public DateTime ModifiedAt { get; set; }

        [JsonConstructor]
        public Database(string name, Dictionary<string, Table> tables, DateTime createdAt, DateTime modifiedAt)
        {
            Name = name;
            Tables = tables ?? new Dictionary<string, Table>();
            CreatedAt = createdAt;
            ModifiedAt = modifiedAt;
        }

        public Database(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Назва бази даних не може бути порожньою", nameof(name));

            Name = name.Trim();
            Tables = new Dictionary<string, Table>();
            CreatedAt = DateTime.UtcNow;
            ModifiedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Створює нову таблицю в базі даних
        /// </summary>
        public void CreateTable(string tableName, List<Column> columns)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Назва таблиці не може бути порожньою", nameof(tableName));

            tableName = tableName.Trim();

            if (Tables.ContainsKey(tableName))
                throw new InvalidOperationException($"Таблиця '{tableName}' вже існує в базі даних");

            var table = new Table(tableName, columns);
            Tables[tableName] = table;
            ModifiedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Створює нову таблицю в базі даних (альтернативний метод)
        /// </summary>
        public void CreateTable(Table table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            if (Tables.ContainsKey(table.Name))
                throw new InvalidOperationException($"Таблиця '{table.Name}' вже існує в базі даних");

            Tables[table.Name] = table;
            ModifiedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Видаляє таблицю з бази даних
        /// </summary>
        public bool DeleteTable(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Назва таблиці не може бути порожньою", nameof(tableName));

            tableName = tableName.Trim();

            if (!Tables.ContainsKey(tableName))
                return false;

            var result = Tables.Remove(tableName);
            if (result)
            {
                ModifiedAt = DateTime.UtcNow;
            }
            return result;
        }

        /// <summary>
        /// Отримує таблицю за назвою
        /// </summary>
        public Table? GetTable(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                return null;

            tableName = tableName.Trim();
            return Tables.TryGetValue(tableName, out var table) ? table : null;
        }

        /// <summary>
        /// Перевіряє, чи існує таблиця
        /// </summary>
        public bool TableExists(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                return false;

            tableName = tableName.Trim();
            return Tables.ContainsKey(tableName);
        }

        /// <summary>
        /// Отримує список всіх таблиць
        /// </summary>
        public List<Table> GetAllTables()
        {
            return Tables.Values.ToList();
        }

        /// <summary>
        /// Отримує список назв всіх таблиць
        /// </summary>
        public List<string> GetTableNames()
        {
            return Tables.Keys.ToList();
        }

        /// <summary>
        /// Очищає базу даних (видаляє всі таблиці)
        /// </summary>
        public void Clear()
        {
            Tables.Clear();
            ModifiedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Повертає кількість таблиць у базі даних
        /// </summary>
        public int TableCount => Tables.Count;

        /// <summary>
        /// Повертає загальну кількість рядків у всіх таблицях
        /// </summary>
        public int TotalRowCount => Tables.Values.Sum(t => t.RowCount);

        public override string ToString()
        {
            return $"Database '{Name}': {TableCount} tables, {TotalRowCount} total rows";
        }
    }
}
