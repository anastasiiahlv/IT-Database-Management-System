using System.Net.Http.Json;
using DatabaseManagementSystem.BlazorUI.Models;

namespace DatabaseManagementSystem.BlazorUI.Services
{
    public class TableService : ITableService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TableService> _logger;

        public TableService(IHttpClientFactory httpClientFactory, ILogger<TableService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("API");
            _logger = logger;
        }

        public async Task<List<TableDto>> GetTablesAsync(string databaseName)
        {
            try
            {
                // Отримуємо список назв таблиць
                var response = await _httpClient.GetAsync("api/database/tables");
                if (response.IsSuccessStatusCode)
                {
                    var tableNames = await response.Content.ReadFromJsonAsync<List<string>>();
                    var tables = new List<TableDto>();

                    // Для кожної таблиці отримуємо деталі
                    if (tableNames != null)
                    {
                        foreach (var tableName in tableNames)
                        {
                            var table = await GetTableAsync(databaseName, tableName);
                            if (table != null)
                                tables.Add(table);
                        }
                    }

                    return tables;
                }
                return new List<TableDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tables");
                return new List<TableDto>();
            }
        }

        public async Task<TableDto?> GetTableAsync(string databaseName, string tableName)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/table/{tableName}");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<dynamic>();
                    // Конвертуємо відповідь в наш DTO
                    return new TableDto
                    {
                        Name = tableName,
                        CreatedAt = DateTime.Now,
                        Columns = new List<ColumnDto>(), // Потрібно parse columns з data
                        Rows = new List<RowDto>() // Потрібно parse rows з data
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting table {TableName}", tableName);
                return null;
            }
        }

        public async Task<ApiResponse> CreateTableAsync(CreateTableRequest request)
        {
            try
            {
                var dto = new
                {
                    TableName = request.TableName,
                    Columns = request.Columns.Select(c => new
                    {
                        Name = c.Name,
                        DataType = ConvertDataType(c.DataType)
                    }).ToList()
                };

                var response = await _httpClient.PostAsJsonAsync("api/table", dto);
                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse { Success = true, Message = "Table created successfully" };
                }

                var error = await response.Content.ReadAsStringAsync();
                return new ApiResponse { Success = false, Message = error };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating table");
                return new ApiResponse { Success = false, Message = ex.Message };
            }
        }

        public async Task<ApiResponse> DeleteTableAsync(string databaseName, string tableName)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/table/{tableName}");
                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse { Success = true, Message = "Table deleted successfully" };
                }

                var error = await response.Content.ReadAsStringAsync();
                return new ApiResponse { Success = false, Message = error };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting table {TableName}", tableName);
                return new ApiResponse { Success = false, Message = ex.Message };
            }
        }

        public async Task<TableDto?> SortTableAsync(string databaseName, string tableName, SortTableRequest request)
        {
            try
            {
                var dto = new
                {
                    ColumnName = request.ColumnName,
                    Ascending = request.Ascending
                };

                var response = await _httpClient.PostAsJsonAsync($"api/table/{tableName}/sort", dto);
                if (response.IsSuccessStatusCode)
                {
                    // Після сортування отримуємо оновлену таблицю
                    return await GetTableAsync(databaseName, tableName);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sorting table {TableName}", tableName);
                return null;
            }
        }

        public Task<ApiResponse> UpdateTableAsync(string databaseName, string tableName, UpdateTableRequest request)
        {
            // Ваш API не має endpoint для оновлення таблиці
            return Task.FromResult(new ApiResponse
            {
                Success = false,
                Message = "Table update is not supported by the API"
            });
        }

        private int ConvertDataType(string blazorType)
        {
            // Конвертація типів даних з Blazor в enum вашого API
            // Потрібно знати які значення enum DataType у вашому DatabaseCore.Models
            return blazorType switch
            {
                "integer" => 0,  // DataType.Integer
                "real" => 1,     // DataType.Real
                "char" => 2,     // DataType.Char
                "string" => 3,   // DataType.String
                "$" => 4,        // DataType.Money
                "$Invl" => 5,    // DataType.MoneyInterval
                _ => 3           // Default to String
            };
        }
    }
}