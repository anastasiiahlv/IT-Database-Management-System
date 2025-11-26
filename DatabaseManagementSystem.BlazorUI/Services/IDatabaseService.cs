using DatabaseManagementSystem.BlazorUI.Models;

namespace DatabaseManagementSystem.BlazorUI.Services
{
    public interface IDatabaseService
    {
        Task<List<DatabaseDto>> GetAllDatabasesAsync();
        Task<DatabaseDto?> GetDatabaseAsync(string name);
        Task<ApiResponse> CreateDatabaseAsync(CreateDatabaseRequest request);
        Task<ApiResponse> DeleteDatabaseAsync(string name);
        Task<ApiResponse> SaveDatabaseAsync(string name);
        Task<ApiResponse> LoadDatabaseAsync(string name);
    }
}
