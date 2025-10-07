using Microsoft.AspNetCore.Mvc;
using DatabaseServer.DTOs;
using DatabaseServer.Services;

namespace DatabaseServer.Controllers
{
    [ApiController]
    [Route("api/table/{tableName}/[controller]")]
    public class RowController : ControllerBase
    {
        private readonly DatabaseStorageService _storageService;

        public RowController(DatabaseStorageService storageService)
        {
            _storageService = storageService;
        }

        /// <summary>
        /// Додати новий рядок до таблиці
        /// </summary>
        [HttpPost]
        public IActionResult AddRow(string tableName, [FromBody] CreateRowDto dto)
        {
            try
            {
                var manager = _storageService.GetDatabaseManager();
                var row = manager.AddRow(tableName, dto.Values);

                return Ok(new
                {
                    message = "Рядок додано",
                    rowId = row.Id,
                    values = row.Values
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Оновити існуючий рядок
        /// </summary>
        [HttpPut("{rowId}")]
        public IActionResult UpdateRow(string tableName, Guid rowId, [FromBody] UpdateRowDto dto)
        {
            try
            {
                var manager = _storageService.GetDatabaseManager();
                manager.UpdateRow(tableName, rowId, dto.Values);

                return Ok(new { message = "Рядок оновлено", rowId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Видалити рядок
        /// </summary>
        [HttpDelete("{rowId}")]
        public IActionResult DeleteRow(string tableName, Guid rowId)
        {
            try
            {
                var manager = _storageService.GetDatabaseManager();
                var result = manager.DeleteRow(tableName, rowId);

                if (result)
                    return Ok(new { message = "Рядок видалено", rowId });
                else
                    return NotFound(new { error = "Рядок не знайдено" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Отримати всі рядки таблиці
        /// </summary>
        [HttpGet]
        public IActionResult GetRows(string tableName)
        {
            try
            {
                var manager = _storageService.GetDatabaseManager();
                var table = manager.GetTable(tableName);

                var rows = table.Rows.Select(r => new
                {
                    id = r.Id,
                    values = r.Values
                });

                return Ok(rows);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Отримати конкретний рядок за ID
        /// </summary>
        [HttpGet("{rowId}")]
        public IActionResult GetRow(string tableName, Guid rowId)
        {
            try
            {
                var manager = _storageService.GetDatabaseManager();
                var row = manager.GetRow(tableName, rowId);

                return Ok(new
                {
                    id = row.Id,
                    values = row.Values
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}