namespace DatabaseManagementSystem.BlazorUI.Models
{
    public static class DataTypes
    {
        public const string Integer = "integer";
        public const string Real = "real";
        public const string Char = "char";
        public const string String = "string";
        public const string Money = "$";
        public const string MoneyInterval = "$Invl";

        public static readonly List<string> AllTypes = new()
        {
            Integer, Real, Char, String, Money, MoneyInterval
        };

        public static string GetDisplayName(string dataType)
        {
            return dataType switch
            {
                Integer => "Integer",
                Real => "Real",
                Char => "Char",
                String => "String",
                Money => "Money ($)",
                MoneyInterval => "Money Interval ($Invl)",
                _ => dataType
            };
        }

        public static bool IsNumeric(string dataType)
        {
            return dataType == Integer || dataType == Real || dataType == Money || dataType == MoneyInterval;
        }

        public static object? ParseValue(string dataType, string? value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            try
            {
                return dataType switch
                {
                    Integer => int.Parse(value),
                    Real => double.Parse(value),
                    Char => value.Length > 0 ? value[0] : '\0',
                    String => value,
                    Money => ParseMoney(value),
                    MoneyInterval => ParseMoneyInterval(value),
                    _ => value
                };
            }
            catch
            {
                return null;
            }
        }

        private static decimal ParseMoney(string value)
        {
            // Remove $ symbol if present
            value = value.Replace("$", "").Replace(",", "");
            if (decimal.TryParse(value, out var result))
            {
                // Validate max value: $10,000,000,000,000.00
                if (result > 10_000_000_000_000.00m)
                    throw new ArgumentException($"Money value cannot exceed $10,000,000,000,000.00");
                return result;
            }
            throw new ArgumentException($"Invalid money format: {value}");
        }

        private static object ParseMoneyInterval(string value)
        {
            // Parse interval format, e.g., "[$100.00, $500.00]"
            value = value.Trim('[', ']');
            var parts = value.Split(',');
            if (parts.Length == 2)
            {
                var start = ParseMoney(parts[0].Trim());
                var end = ParseMoney(parts[1].Trim());
                return new { Start = start, End = end };
            }
            throw new ArgumentException($"Invalid money interval format: {value}");
        }
    }
}
