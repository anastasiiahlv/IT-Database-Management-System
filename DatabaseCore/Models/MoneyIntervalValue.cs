using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DatabaseCore.Models
{
    public class MoneyIntervalValue : IEquatable<MoneyIntervalValue>
    {
        [JsonPropertyName("from")]
        public MoneyValue From { get; private set; }

        [JsonPropertyName("to")]
        public MoneyValue To { get; private set; }

        [JsonConstructor]
        public MoneyIntervalValue(MoneyValue from, MoneyValue to)
        {
            if (from > to)
                throw new ArgumentException("Початкове значення не може бути більшим за кінцеве");

            From = from;
            To = to;
        }

        public MoneyIntervalValue(decimal from, decimal to)
            : this(new MoneyValue(from), new MoneyValue(to))
        {
        }

        /// <summary>
        /// Парсить рядок формату "100.00-500.00" або "$100.00-$500.00"
        /// </summary>
        public static MoneyIntervalValue Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Значення не може бути порожнім");

            var parts = value.Split('-', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                throw new FormatException($"Неправильний формат інтервалу: '{value}'. Очікується формат: '100.00-500.00'");

            var from = MoneyValue.Parse(parts[0].Trim());
            var to = MoneyValue.Parse(parts[1].Trim());

            return new MoneyIntervalValue(from, to);
        }

        public static bool TryParse(string value, out MoneyIntervalValue? result)
        {
            try
            {
                result = Parse(value);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Перевіряє, чи знаходиться значення в інтервалі
        /// </summary>
        public bool Contains(MoneyValue value)
        {
            return value >= From && value <= To;
        }

        public bool Equals(MoneyIntervalValue? other)
        {
            if (other == null) return false;
            return From == other.From && To == other.To;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as MoneyIntervalValue);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(From, To);
        }

        public override string ToString()
        {
            return $"{From} - {To}";
        }

        public static bool operator ==(MoneyIntervalValue? left, MoneyIntervalValue? right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        public static bool operator !=(MoneyIntervalValue? left, MoneyIntervalValue? right)
        {
            return !(left == right);
        }
    }
}
