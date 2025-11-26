using DatabaseManagementSystem.BlazorUI.Models;

namespace DatabaseManagementSystem.BlazorUI.Services
{
    public interface IRowService
    {
        Task<List<RowDto>> GetRowsAsync(string databaseName, string tableName);
        Task<RowDto?> GetRowAsync(string databaseName, string tableName, int rowId);
        Task<ApiResponse> CreateRowAsync(string databaseName, string tableName, CreateRowRequest request);
        Task<ApiResponse> UpdateRowAsync(string databaseName, string tableName, int rowId, UpdateRowRequest request);
        Task<ApiResponse> DeleteRowAsync(string databaseName, string tableName, int rowId);
        Task<ApiResponse<bool>> ValidateRowAsync(string databaseName, string tableName, Dictionary<string, object?> values);
    }
}
