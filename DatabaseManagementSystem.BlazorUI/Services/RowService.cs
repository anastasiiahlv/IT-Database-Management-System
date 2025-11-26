using DatabaseManagementSystem.BlazorUI.Models;

namespace DatabaseManagementSystem.BlazorUI.Services
{
    public class RowService : IRowService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RowService> _logger;

        public RowService(IHttpClientFactory httpClientFactory, ILogger<RowService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("API");
            _logger = logger;
        }

        public async Task<List<RowDto>> GetRowsAsync(string databaseName, string tableName)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<RowDto>>($"api/row/{databaseName}/{tableName}");
                return response ?? new List<RowDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rows for table {TableName}", tableName);
                return new List<RowDto>();
            }
        }

        public async Task<RowDto?> GetRowAsync(string databaseName, string tableName, int rowId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<RowDto>($"api/row/{databaseName}/{tableName}/{rowId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting row {RowId} from table {TableName}", rowId, tableName);
                return null;
            }
        }

        public async Task<ApiResponse> CreateRowAsync(string databaseName, string tableName, CreateRowRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/row/{databaseName}/{tableName}", request);
                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse { Success = true, Message = "Row created successfully" };
                }

                var error = await response.Content.ReadAsStringAsync();
                return new ApiResponse { Success = false, Message = error };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating row");
                return new ApiResponse { Success = false, Message = ex.Message };
            }
        }

        public async Task<ApiResponse> UpdateRowAsync(string databaseName, string tableName, int rowId, UpdateRowRequest request)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/row/{databaseName}/{tableName}/{rowId}", request);
                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse { Success = true, Message = "Row updated successfully" };
                }

                var error = await response.Content.ReadAsStringAsync();
                return new ApiResponse { Success = false, Message = error };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating row {RowId}", rowId);
                return new ApiResponse { Success = false, Message = ex.Message };
            }
        }

        public async Task<ApiResponse> DeleteRowAsync(string databaseName, string tableName, int rowId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/row/{databaseName}/{tableName}/{rowId}");
                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse { Success = true, Message = "Row deleted successfully" };
                }

                var error = await response.Content.ReadAsStringAsync();
                return new ApiResponse { Success = false, Message = error };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting row {RowId}", rowId);
                return new ApiResponse { Success = false, Message = ex.Message };
            }
        }

        public async Task<ApiResponse<bool>> ValidateRowAsync(string databaseName, string tableName, Dictionary<string, object?> values)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/row/{databaseName}/{tableName}/validate", values);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<bool>();
                    return new ApiResponse<bool> { Success = true, Data = result };
                }

                var error = await response.Content.ReadAsStringAsync();
                return new ApiResponse<bool> { Success = false, Message = error, Data = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating row");
                return new ApiResponse<bool> { Success = false, Message = ex.Message, Data = false };
            }
        }
    }
}
