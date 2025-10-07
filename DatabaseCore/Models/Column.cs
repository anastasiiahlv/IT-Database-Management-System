using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DatabaseCore.Models
{
    public class Column
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("dataType")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DataType DataType { get; set; }

        public Column()
        {
            Name = string.Empty;
        }

        public Column(string name, DataType dataType)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Назва колонки не може бути порожньою", nameof(name));

            if (name.Length > 50)
                throw new ArgumentException("Назва колонки не може перевищувати 50 символів", nameof(name));

            Name = name.Trim();
            DataType = dataType;
        }

        /// <summary>
        /// Валідує значення відповідно до типу даних колонки
        /// </summary>
        public bool IsValidValue(object? value)
        {
            if (value == null)
                return true; // null дозволено для всіх типів

            return DataType switch
            {
                DataType.Integer => value is int,
                DataType.Real => value is double or float or decimal,
                DataType.Char => value is char || (value is string s && s.Length == 1),
                DataType.String => value is string,
                DataType.Money => value is MoneyValue,
                DataType.MoneyInterval => value is MoneyIntervalValue,
                _ => false
            };
        }

        /// <summary>
        /// Конвертує значення у відповідний тип
        /// </summary>
        public object? ConvertValue(object? value)
        {
            if (value == null)
                return null;

            try
            {
                return DataType switch
                {
                    DataType.Integer => Convert.ToInt32(value),
                    DataType.Real => Convert.ToDouble(value),
                    DataType.Char => value is string s ? s[0] : (char)value,
                    DataType.String => value.ToString(),
                    DataType.Money => value is MoneyValue mv ? mv : MoneyValue.Parse(value.ToString()!),
                    DataType.MoneyInterval => value is MoneyIntervalValue miv ? miv : MoneyIntervalValue.Parse(value.ToString()!),
                    _ => throw new InvalidOperationException($"Невідомий тип даних: {DataType}")
                };
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Неможливо конвертувати значення '{value}' у тип {DataType}", ex);
            }
        }

        public override string ToString()
        {
            return $"{Name} ({DataType})";
        }
    }
}
