using Microsoft.AspNetCore.Mvc;
using DatabaseServer.DTOs;
using DatabaseServer.Services;

namespace DatabaseServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseController : ControllerBase
    {
        private readonly DatabaseStorageService _storageService;

        public DatabaseController(DatabaseStorageService storageService)
        {
            _storageService = storageService;
        }

        /// <summary>
        /// Створити нову базу даних
        /// </summary>
        [HttpPost("create")]
        public IActionResult CreateDatabase([FromBody] CreateDatabaseDto dto)
        {
            try
            {
                var manager = _storageService.GetDatabaseManager();
                var database = manager.CreateDatabase(dto.Name);
                return Ok(new { message = $"База даних '{dto.Name}' створена", name = database.Name });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Отримати інформацію про поточну базу даних
        /// </summary>
        [HttpGet("info")]
        public IActionResult GetDatabaseInfo()
        {
            try
            {
                var manager = _storageService.GetDatabaseManager();
                if (!manager.HasOpenDatabase)
                    return NotFound(new { error = "Немає відкритої бази даних" });

                var stats = manager.GetStatistics();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Отримати список всіх таблиць
        /// </summary>
        [HttpGet("tables")]
        public IActionResult GetTables()
        {
            try
            {
                var manager = _storageService.GetDatabaseManager();
                if (!manager.HasOpenDatabase)
                    return NotFound(new { error = "Немає відкритої бази даних" });

                var tables = manager.GetTableNames();
                return Ok(tables);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Зберегти базу даних у файл
        /// </summary>
        [HttpPost("save")]
        public IActionResult SaveDatabase([FromBody] SaveDatabaseDto dto)
        {
            try
            {
                var manager = _storageService.GetDatabaseManager();
                manager.SaveDatabase(dto.FilePath);
                return Ok(new { message = "База даних збережена", filePath = dto.FilePath });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Завантажити базу даних з файлу
        /// </summary>
        [HttpPost("load")]
        public IActionResult LoadDatabase([FromBody] LoadDatabaseDto dto)
        {
            try
            {
                var manager = _storageService.GetDatabaseManager();
                var database = manager.LoadDatabase(dto.FilePath);
                return Ok(new { message = "База даних завантажена", name = database.Name });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Закрити поточну базу даних
        /// </summary>
        [HttpPost("close")]
        public IActionResult CloseDatabase()
        {
            try
            {
                var manager = _storageService.GetDatabaseManager();
                manager.CloseDatabase();
                return Ok(new { message = "База даних закрита" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}