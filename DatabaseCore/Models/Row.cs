using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DatabaseCore.Models
{
    public class Row
    {
        [JsonPropertyName("id")]
        public Guid Id { get; private set; }

        [JsonPropertyName("values")]
        public Dictionary<string, object?> Values { get; private set; }

        [JsonConstructor]
        public Row(Guid id, Dictionary<string, object?> values)
        {
            Id = id;
            Values = values ?? new Dictionary<string, object?>();
        }

        public Row()
        {
            Id = Guid.NewGuid();
            Values = new Dictionary<string, object?>();
        }

        public Row(List<Column> columns)
        {
            Id = Guid.NewGuid();
            Values = new Dictionary<string, object?>();

            // Ініціалізуємо всі колонки зі значеннями null
            foreach (var column in columns)
            {
                Values[column.Name] = null;
            }
        }

        /// <summary>
        /// Встановлює значення для колонки з валідацією
        /// </summary>
        public void SetValue(string columnName, object? value, Column column)
        {
            if (!column.IsValidValue(value))
            {
                throw new ArgumentException(
                    $"Значення '{value}' не відповідає типу {column.DataType} для колонки '{columnName}'");
            }

            // Конвертуємо значення у правильний тип
            var convertedValue = column.ConvertValue(value);
            Values[columnName] = convertedValue;
        }

        /// <summary>
        /// Отримує значення колонки
        /// </summary>
        public object? GetValue(string columnName)
        {
            return Values.TryGetValue(columnName, out var value) ? value : null;
        }

        /// <summary>
        /// Отримує значення колонки у вигляді типізованого об'єкта
        /// </summary>
        public T? GetValue<T>(string columnName)
        {
            var value = GetValue(columnName);
            if (value == null)
                return default;

            if (value is T typedValue)
                return typedValue;

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// Перевіряє, чи відповідає рядок схемі таблиці
        /// </summary>
        public bool ValidateAgainstSchema(List<Column> columns)
        {
            // Перевіряємо, що всі колонки присутні
            foreach (var column in columns)
            {
                if (!Values.ContainsKey(column.Name))
                    return false;

                // Перевіряємо тип значення
                var value = Values[column.Name];
                if (!column.IsValidValue(value))
                    return false;
            }

            // Перевіряємо, що немає зайвих колонок
            foreach (var key in Values.Keys)
            {
                if (!columns.Any(c => c.Name == key))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Створює копію рядка
        /// </summary>
        public Row Clone()
        {
            var clonedValues = new Dictionary<string, object?>(Values);
            return new Row(Guid.NewGuid(), clonedValues);
        }

        public override string ToString()
        {
            var values = string.Join(", ", Values.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
            return $"Row [{Id}]: {values}";
        }
    }
}
