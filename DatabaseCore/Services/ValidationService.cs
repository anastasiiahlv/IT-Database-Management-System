using DatabaseCore.Models;
using DatabaseCore.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DatabaseCore.Services
{
    /// <summary>
    /// Сервіс для валідації даних
    /// </summary>
    public class ValidationService
    {
        // Константи для валідації
        private const int MaxStringLength = 1000;
        private const int MaxTableNameLength = 50;
        private const int MaxColumnNameLength = 50;

        /// <summary>
        /// Валідує назву таблиці
        /// </summary>
        public static ValidationResult ValidateTableName(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                return ValidationResult.Failure("Назва таблиці не може бути порожньою");

            tableName = tableName.Trim();

            if (tableName.Length > MaxTableNameLength)
                return ValidationResult.Failure($"Назва таблиці не може перевищувати {MaxTableNameLength} символів");

            // Перевіряємо, що назва складається тільки з букв, цифр та підкреслення
            if (!Regex.IsMatch(tableName, @"^[a-zA-Z0-9_а-яА-ЯіІїЇєЄ]+$"))
                return ValidationResult.Failure("Назва таблиці може містити тільки букви, цифри та підкреслення");

            // Назва не може починатися з цифри
            if (char.IsDigit(tableName[0]))
                return ValidationResult.Failure("Назва таблиці не може починатися з цифри");

            return ValidationResult.Success();
        }

        /// <summary>
        /// Валідує назву колонки
        /// </summary>
        public static ValidationResult ValidateColumnName(string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
                return ValidationResult.Failure("Назва колонки не може бути порожньою");

            columnName = columnName.Trim();

            if (columnName.Length > MaxColumnNameLength)
                return ValidationResult.Failure($"Назва колонки не може перевищувати {MaxColumnNameLength} символів");

            // Перевіряємо, що назва складається тільки з букв, цифр та підкреслення
            if (!Regex.IsMatch(columnName, @"^[a-zA-Z0-9_а-яА-ЯіІїЇєЄ]+$"))
                return ValidationResult.Failure("Назва колонки може містити тільки букви, цифри та підкреслення");

            // Назва не може починатися з цифри
            if (char.IsDigit(columnName[0]))
                return ValidationResult.Failure("Назва колонки не може починатися з цифри");

            return ValidationResult.Success();
        }

        /// <summary>
        /// Валідує значення відповідно до типу даних
        /// </summary>
        public static ValidationResult ValidateValue(object? value, DataType dataType)
        {
            // null дозволено для всіх типів
            if (value == null)
                return ValidationResult.Success();

            return dataType switch
            {
                DataType.Integer => ValidateInteger(value),
                DataType.Real => ValidateReal(value),
                DataType.Char => ValidateChar(value),
                DataType.String => ValidateString(value),
                DataType.Money => ValidateMoney(value),
                DataType.MoneyInterval => ValidateMoneyInterval(value),
                _ => ValidationResult.Failure($"Невідомий тип даних: {dataType}")
            };
        }

        /// <summary>
        /// Валідує ціле число (Integer)
        /// </summary>
        public static ValidationResult ValidateInteger(object value)
        {
            if (value == null)
                return ValidationResult.Success();

            // Перевіряємо, чи це вже int
            if (value is int)
                return ValidationResult.Success();

            // Пробуємо конвертувати
            if (value is string strValue)
            {
                strValue = strValue.Trim();
                if (int.TryParse(strValue, out _))
                    return ValidationResult.Success();

                return ValidationResult.Failure($"Значення '{strValue}' не є цілим числом");
            }

            // Пробуємо конвертувати з інших числових типів
            try
            {
                Convert.ToInt32(value);
                return ValidationResult.Success();
            }
            catch
            {
                return ValidationResult.Failure($"Значення '{value}' не може бути конвертоване в ціле число");
            }
        }

        /// <summary>
        /// Валідує дійсне число (Real)
        /// </summary>
        public static ValidationResult ValidateReal(object value)
        {
            if (value == null)
                return ValidationResult.Success();

            // Перевіряємо, чи це вже double/float/decimal
            if (value is double or float or decimal)
                return ValidationResult.Success();

            // Пробуємо конвертувати
            if (value is string strValue)
            {
                strValue = strValue.Trim().Replace(",", ".");
                if (double.TryParse(strValue, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out _))
                    return ValidationResult.Success();

                return ValidationResult.Failure($"Значення '{strValue}' не є дійсним числом");
            }

            // Пробуємо конвертувати з інших числових типів
            try
            {
                Convert.ToDouble(value);
                return ValidationResult.Success();
            }
            catch
            {
                return ValidationResult.Failure($"Значення '{value}' не може бути конвертоване в дійсне число");
            }
        }

        /// <summary>
        /// Валідує символ (Char)
        /// </summary>
        public static ValidationResult ValidateChar(object value)
        {
            if (value == null)
                return ValidationResult.Success();

            // Перевіряємо, чи це вже char
            if (value is char)
                return ValidationResult.Success();

            // Якщо це рядок з одним символом
            if (value is string strValue)
            {
                strValue = strValue.Trim();
                if (strValue.Length == 1)
                    return ValidationResult.Success();

                if (strValue.Length == 0)
                    return ValidationResult.Failure("Значення не може бути порожнім для типу Char");

                return ValidationResult.Failure($"Значення '{strValue}' містить більше одного символу. Очікується рівно один символ");
            }

            return ValidationResult.Failure($"Значення '{value}' не може бути конвертоване в символ");
        }

        /// <summary>
        /// Валідує рядок (String)
        /// </summary>
        public static ValidationResult ValidateString(object value)
        {
            if (value == null)
                return ValidationResult.Success();

            // Будь-яке значення може бути конвертоване в рядок
            var strValue = value.ToString();

            if (strValue != null && strValue.Length > MaxStringLength)
                return ValidationResult.Failure($"Рядок не може перевищувати {MaxStringLength} символів");

            return ValidationResult.Success();
        }

        /// <summary>
        /// Валідує грошове значення (Money): 0.00 до 10,000,000,000,000.00
        /// </summary>
        public static ValidationResult ValidateMoney(object value)
        {
            if (value == null)
                return ValidationResult.Success();

            // Якщо це вже MoneyValue
            if (value is MoneyValue)
                return ValidationResult.Success();

            // Пробуємо розпарсити з рядка
            if (value is string strValue)
            {
                if (MoneyValue.TryParse(strValue, out var moneyValue))
                    return ValidationResult.Success();

                return ValidationResult.Failure($"Значення '{strValue}' не є валідним грошовим значенням. " +
                    $"Очікується формат: 0.00 до {MoneyValue.MaxValue:N2}");
            }

            // Пробуємо створити MoneyValue з числового значення
            try
            {
                decimal amount = Convert.ToDecimal(value);

                if (amount < MoneyValue.MinValue || amount > MoneyValue.MaxValue)
                    return ValidationResult.Failure($"Грошове значення має бути між " +
                        $"{MoneyValue.MinValue:N2} та {MoneyValue.MaxValue:N2}");

                return ValidationResult.Success();
            }
            catch (Exception ex)
            {
                return ValidationResult.Failure($"Значення '{value}' не може бути конвертоване в грошовий тип: {ex.Message}");
            }
        }

        /// <summary>
        /// Валідує інтервал грошових значень (MoneyInterval)
        /// </summary>
        public static ValidationResult ValidateMoneyInterval(object value)
        {
            if (value == null)
                return ValidationResult.Success();

            // Якщо це вже MoneyIntervalValue
            if (value is MoneyIntervalValue)
                return ValidationResult.Success();

            // Пробуємо розпарсити з рядка формату "100.00-500.00"
            if (value is string strValue)
            {
                if (MoneyIntervalValue.TryParse(strValue, out var intervalValue))
                    return ValidationResult.Success();

                return ValidationResult.Failure($"Значення '{strValue}' не є валідним інтервалом. " +
                    $"Очікується формат: '100.00-500.00' або '$100.00-$500.00'");
            }

            return ValidationResult.Failure($"Значення '{value}' не може бути конвертоване в інтервал грошових значень");
        }

        /// <summary>
        /// Валідує, що всі назви колонок унікальні
        /// </summary>
        public static ValidationResult ValidateUniqueColumnNames(System.Collections.Generic.List<Column> columns)
        {
            if (columns == null || columns.Count == 0)
                return ValidationResult.Failure("Список колонок не може бути порожнім");

            var duplicates = columns
                .GroupBy(c => c.Name.ToLower())
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicates.Any())
                return ValidationResult.Failure($"Знайдено дубльовані назви колонок: {string.Join(", ", duplicates)}");

            return ValidationResult.Success();
        }

        /// <summary>
        /// Валідує колонку (назва + тип)
        /// </summary>
        public static ValidationResult ValidateColumn(Column column)
        {
            if (column == null)
                return ValidationResult.Failure("Колонка не може бути null");

            var nameValidation = ValidateColumnName(column.Name);
            if (!nameValidation.IsValid)
                return nameValidation;

            return ValidationResult.Success();
        }
    }

    /// <summary>
    /// Результат валідації
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; }
        public string ErrorMessage { get; }

        private ValidationResult(bool isValid, string errorMessage = "")
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
        }

        public static ValidationResult Success() => new ValidationResult(true);

        public static ValidationResult Failure(string errorMessage) => new ValidationResult(false, errorMessage);

        /// <summary>
        /// Викидає ValidationException, якщо валідація не пройшла
        /// </summary>
        public void ThrowIfInvalid()
        {
            if (!IsValid)
                throw new ValidationException(ErrorMessage);
        }
    }
}
