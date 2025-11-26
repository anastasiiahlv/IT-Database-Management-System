using DatabaseManagementSystem.BlazorUI.Models;

namespace DatabaseManagementSystem.BlazorUI.Services
{
    public interface ITableService
    {
        Task<List<TableDto>> GetTablesAsync(string databaseName);
        Task<TableDto?> GetTableAsync(string databaseName, string tableName);
        Task<ApiResponse> CreateTableAsync(CreateTableRequest request);
        Task<ApiResponse> UpdateTableAsync(string databaseName, string tableName, UpdateTableRequest request);
        Task<ApiResponse> DeleteTableAsync(string databaseName, string tableName);
        Task<TableDto?> SortTableAsync(string databaseName, string tableName, SortTableRequest request);
    }
}
