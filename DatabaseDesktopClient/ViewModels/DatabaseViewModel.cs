using CommunityToolkit.Mvvm.ComponentModel;
using DatabaseDesktopClient.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseCore.Managers;
using DatabaseCore;

namespace DatabaseDesktopClient.ViewModels
{
    public partial class DatabaseViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;

        public DatabaseViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            LoadStatistics();
        }

        #region Властивості

        [ObservableProperty]
        private string _databaseName = string.Empty;

        [ObservableProperty]
        private int _tableCount;

        [ObservableProperty]
        private int _totalRowCount;

        [ObservableProperty]
        private string _createdAt = string.Empty;

        [ObservableProperty]
        private string _modifiedAt = string.Empty;

        [ObservableProperty]
        private string _filePath = "Не збережено";

        [ObservableProperty]
        private string _welcomeMessage = string.Empty;

        #endregion

        #region Методи

        /// <summary>
        /// Завантажує статистику бази даних
        /// </summary>
        private void LoadStatistics()
        {
            try
            {
                var stats = _databaseService.GetStatistics();

                DatabaseName = stats.DatabaseName;
                TableCount = stats.TableCount;
                TotalRowCount = stats.TotalRowCount;
                CreatedAt = stats.CreatedAt.ToString("dd.MM.yyyy HH:mm:ss");
                ModifiedAt = stats.ModifiedAt.ToString("dd.MM.yyyy HH:mm:ss");
                FilePath = stats.FilePath ?? "Не збережено";

                // Формуємо привітальне повідомлення
                if (TableCount == 0)
                {
                    WelcomeMessage = "База даних порожня. Створіть першу таблицю!";
                }
                else
                {
                    WelcomeMessage = $"Всього таблиць: {TableCount}, рядків: {TotalRowCount}";
                }
            }
            catch (Exception ex)
            {
                WelcomeMessage = $"Помилка завантаження статистики: {ex.Message}";
            }
        }

        #endregion
    }
}
