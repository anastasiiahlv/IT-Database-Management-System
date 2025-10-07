using DatabaseCore.Managers;
using DatabaseCore.Models;

namespace DatabaseServer.Services
{
    /// <summary>
    /// Singleton сервіс для збереження стану бази даних
    /// </summary>
    public class DatabaseStorageService
    {
        private readonly DatabaseManager _databaseManager;

        public DatabaseStorageService()
        {
            _databaseManager = new DatabaseManager();
        }

        public DatabaseManager GetDatabaseManager() => _databaseManager;
    }
}