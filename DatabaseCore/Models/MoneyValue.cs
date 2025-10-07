using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DatabaseCore.Models
{
    public class MoneyValue : IComparable<MoneyValue>, IEquatable<MoneyValue>
    {
        public const decimal MaxValue = 10_000_000_000_000.00m;
        public const decimal MinValue = 0.00m;

        [JsonPropertyName("amount")]
        public decimal Amount { get; private set; }

        [JsonConstructor]
        public MoneyValue(decimal amount)
        {
            if (amount < MinValue || amount > MaxValue)
                throw new ArgumentOutOfRangeException(nameof(amount),
                    $"Значення має бути між {MinValue:N2} та {MaxValue:N2}");

            // Округлюємо до 2 знаків після коми
            Amount = Math.Round(amount, 2);
        }

        public static MoneyValue Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Значення не може бути порожнім");

            // Прибираємо символи валюти та пробіли
            value = value.Replace("$", "").Replace(" ", "").Replace(",", "");

            if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal amount))
                throw new FormatException($"Неможливо розпарсити значення '{value}' як Money");

            return new MoneyValue(amount);
        }

        public static bool TryParse(string value, out MoneyValue? result)
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

        public int CompareTo(MoneyValue? other)
        {
            if (other == null) return 1;
            return Amount.CompareTo(other.Amount);
        }

        public bool Equals(MoneyValue? other)
        {
            if (other == null) return false;
            return Amount == other.Amount;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as MoneyValue);
        }

        public override int GetHashCode()
        {
            return Amount.GetHashCode();
        }

        public override string ToString()
        {
            return $"${Amount:N2}";
        }

        public static bool operator ==(MoneyValue? left, MoneyValue? right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        public static bool operator !=(MoneyValue? left, MoneyValue? right)
        {
            return !(left == right);
        }

        public static bool operator <(MoneyValue left, MoneyValue right)
        {
            return left.Amount < right.Amount;
        }

        public static bool operator >(MoneyValue left, MoneyValue right)
        {
            return left.Amount > right.Amount;
        }

        public static bool operator <=(MoneyValue left, MoneyValue right)
        {
            return left.Amount <= right.Amount;
        }

        public static bool operator >=(MoneyValue left, MoneyValue right)
        {
            return left.Amount >= right.Amount;
        }
    }
}
