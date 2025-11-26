using System.Net.Http.Json;
using DatabaseManagementSystem.BlazorUI.Models;

namespace DatabaseManagementSystem.BlazorUI.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DatabaseService> _logger;

        public DatabaseService(IHttpClientFactory httpClientFactory, ILogger<DatabaseService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("API");
            _logger = logger;
        }

        public async Task<List<DatabaseDto>> GetAllDatabasesAsync()
        {
            try
            {
                // Ваш API не має endpoint для отримання всіх баз
                // Можемо повернути поточну базу якщо вона відкрита
                var response = await _httpClient.GetAsync("api/database/info");
                if (response.IsSuccessStatusCode)
                {
                    var info = await response.Content.ReadFromJsonAsync<dynamic>();
                    // Повертаємо список з однією базою
                    return new List<DatabaseDto>
                    {
                        new DatabaseDto
                        {
                            Name = "Current Database",
                            CreatedAt = DateTime.Now,
                            Tables = new List<TableDto>()
                        }
                    };
                }
                return new List<DatabaseDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting database info");
                return new List<DatabaseDto>();
            }
        }

        public async Task<DatabaseDto?> GetDatabaseAsync(string name)
        {
            try
            {
                var response = await _httpClient.GetAsync("api/database/info");
                if (response.IsSuccessStatusCode)
                {
                    var tables = await GetTablesAsync();
                    return new DatabaseDto
                    {
                        Name = name,
                        Tables = tables,
                        CreatedAt = DateTime.Now
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting database {Name}", name);
                return null;
            }
        }

        private async Task<List<TableDto>> GetTablesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/database/tables");
                if (response.IsSuccessStatusCode)
                {
                    var tableNames = await response.Content.ReadFromJsonAsync<List<string>>();
                    if (tableNames != null)
                    {
                        return tableNames.Select(name => new TableDto
                        {
                            Name = name,
                            CreatedAt = DateTime.Now,
                            Columns = new List<ColumnDto>(),
                            Rows = new List<RowDto>()
                        }).ToList();
                    }
                }
                return new List<TableDto>();
            }
            catch
            {
                return new List<TableDto>();
            }
        }

        public async Task<ApiResponse> CreateDatabaseAsync(CreateDatabaseRequest request)
        {
            try
            {
                var dto = new { Name = request.Name };
                var response = await _httpClient.PostAsJsonAsync("api/database/create", dto);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<dynamic>();
                    return new ApiResponse { Success = true, Message = "Database created successfully" };
                }

                var error = await response.Content.ReadAsStringAsync();
                return new ApiResponse { Success = false, Message = error };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating database");
                return new ApiResponse { Success = false, Message = ex.Message };
            }
        }

        public async Task<ApiResponse> DeleteDatabaseAsync(string name)
        {
            try
            {
                // Ваш API має тільки close, не delete
                var response = await _httpClient.PostAsync("api/database/close", null);
                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse { Success = true, Message = "Database closed successfully" };
                }

                var error = await response.Content.ReadAsStringAsync();
                return new ApiResponse { Success = false, Message = error };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing database");
                return new ApiResponse { Success = false, Message = ex.Message };
            }
        }

        public async Task<ApiResponse> SaveDatabaseAsync(string name)
        {
            try
            {
                var dto = new { FilePath = $"{name}.db" };
                var response = await _httpClient.PostAsJsonAsync("api/database/save", dto);

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse { Success = true, Message = "Database saved to disk successfully" };
                }

                var error = await response.Content.ReadAsStringAsync();
                return new ApiResponse { Success = false, Message = error };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving database {Name}", name);
                return new ApiResponse { Success = false, Message = ex.Message };
            }
        }

        public async Task<ApiResponse> LoadDatabaseAsync(string name)
        {
            try
            {
                var dto = new { FilePath = $"{name}.db" };
                var response = await _httpClient.PostAsJsonAsync("api/database/load", dto);

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse { Success = true, Message = "Database loaded from disk successfully" };
                }

                var error = await response.Content.ReadAsStringAsync();
                return new ApiResponse { Success = false, Message = error };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading database {Name}", name);
                return new ApiResponse { Success = false, Message = ex.Message };
            }
        }
    }
}