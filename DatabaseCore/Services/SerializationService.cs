using DatabaseCore.Exceptions;
using DatabaseCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace DatabaseCore.Services
{
    /// <summary>
    /// Сервіс для серіалізації та десеріалізації бази даних
    /// </summary>
    public class SerializationService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true, // Для зручності читання
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                new JsonStringEnumConverter(), // Для enum (DataType)
                new ObjectJsonConverter() // Для Dictionary<string, object?>
            }
        };

        /// <summary>
        /// Зберігає базу даних у JSON файл
        /// </summary>
        public static void SaveToFile(Database database, string filePath)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Шлях до файлу не може бути порожнім", nameof(filePath));

            try
            {
                // Створюємо директорію, якщо вона не існує
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Серіалізуємо базу даних в JSON
                var json = JsonSerializer.Serialize(database, JsonOptions);

                // Записуємо у файл
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                throw new FileOperationException(filePath, "Помилка збереження бази даних", ex);
            }
        }

        /// <summary>
        /// Завантажує базу даних з JSON файлу
        /// </summary>
        public static Database LoadFromFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Шлях до файлу не може бути порожнім", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileOperationException(filePath, "Файл не існує");

            try
            {
                // Читаємо JSON з файлу
                var json = File.ReadAllText(filePath);

                // Десеріалізуємо базу даних
                var database = JsonSerializer.Deserialize<Database>(json, JsonOptions);

                if (database == null)
                    throw new SerializationException("Не вдалося десеріалізувати базу даних");

                return database;
            }
            catch (JsonException ex)
            {
                throw new SerializationException($"Помилка парсингу JSON файлу '{filePath}'", ex);
            }
            catch (Exception ex) when (ex is not FileOperationException && ex is not SerializationException)
            {
                throw new FileOperationException(filePath, "Помилка завантаження бази даних", ex);
            }
        }

        /// <summary>
        /// Експортує таблицю в JSON
        /// </summary>
        public static string ExportTableToJson(Table table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            try
            {
                return JsonSerializer.Serialize(table, JsonOptions);
            }
            catch (Exception ex)
            {
                throw new SerializationException($"Помилка експорту таблиці '{table.Name}'", ex);
            }
        }

        /// <summary>
        /// Імпортує таблицю з JSON
        /// </summary>
        public static Table ImportTableFromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("JSON не може бути порожнім", nameof(json));

            try
            {
                var table = JsonSerializer.Deserialize<Table>(json, JsonOptions);

                if (table == null)
                    throw new SerializationException("Не вдалося десеріалізувати таблицю");

                return table;
            }
            catch (JsonException ex)
            {
                throw new SerializationException("Помилка парсингу JSON таблиці", ex);
            }
        }

        /// <summary>
        /// Створює резервну копію бази даних
        /// </summary>
        public static void CreateBackup(Database database, string originalFilePath)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            if (string.IsNullOrWhiteSpace(originalFilePath))
                throw new ArgumentException("Шлях до файлу не може бути порожнім", nameof(originalFilePath));

            try
            {
                var directory = Path.GetDirectoryName(originalFilePath);
                var fileName = Path.GetFileNameWithoutExtension(originalFilePath);
                var extension = Path.GetExtension(originalFilePath);
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

                var backupFileName = $"{fileName}_backup_{timestamp}{extension}";
                var backupPath = Path.Combine(directory ?? "", backupFileName);

                SaveToFile(database, backupPath);
            }
            catch (Exception ex)
            {
                throw new FileOperationException(originalFilePath, "Помилка створення резервної копії", ex);
            }
        }
    }

    /// <summary>
    /// Конвертер для Dictionary<string, object?> при серіалізації
    /// </summary>
    public class ObjectJsonConverter : JsonConverter<object>
    {
        public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.True:
                    return true;
                case JsonTokenType.False:
                    return false;
                case JsonTokenType.Number:
                    if (reader.TryGetInt32(out int intValue))
                        return intValue;
                    if (reader.TryGetInt64(out long longValue))
                        return longValue;
                    return reader.GetDouble();
                case JsonTokenType.String:
                    var stringValue = reader.GetString();

                    // Пробуємо розпарсити як MoneyValue
                    if (stringValue != null && stringValue.Contains("$"))
                    {
                        if (stringValue.Contains("-") && MoneyIntervalValue.TryParse(stringValue, out var intervalValue))
                            return intervalValue;

                        if (MoneyValue.TryParse(stringValue, out var moneyValue))
                            return moneyValue;
                    }

                    return stringValue;
                case JsonTokenType.StartObject:
                    using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
                    {
                        var element = doc.RootElement;

                        // Перевіряємо, чи це MoneyValue
                        if (element.TryGetProperty("amount", out _))
                        {
                            return JsonSerializer.Deserialize<MoneyValue>(element.GetRawText(), options);
                        }

                        // Перевіряємо, чи це MoneyIntervalValue
                        if (element.TryGetProperty("from", out _) && element.TryGetProperty("to", out _))
                        {
                            return JsonSerializer.Deserialize<MoneyIntervalValue>(element.GetRawText(), options);
                        }

                        return JsonSerializer.Deserialize<object>(element.GetRawText(), options);
                    }
                case JsonTokenType.Null:
                    return null;
                default:
                    throw new JsonException($"Unsupported token type: {reader.TokenType}");
            }
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            // Спеціальна обробка для MoneyValue та MoneyIntervalValue
            if (value is MoneyValue moneyValue)
            {
                JsonSerializer.Serialize(writer, moneyValue, options);
                return;
            }

            if (value is MoneyIntervalValue intervalValue)
            {
                JsonSerializer.Serialize(writer, intervalValue, options);
                return;
            }

            // Для інших типів використовуємо стандартну серіалізацію
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
    }
}
