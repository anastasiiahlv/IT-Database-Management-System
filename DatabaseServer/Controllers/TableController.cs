using Microsoft.AspNetCore.Mvc;
using DatabaseCore.Models;
using DatabaseServer.DTOs;
using DatabaseServer.Services;

namespace DatabaseServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TableController : ControllerBase
    {
        private readonly DatabaseStorageService _storageService;

        public TableController(DatabaseStorageService storageService)
        {
            _storageService = storageService;
        }

        /// <summary>
        /// Створити нову таблицю
        /// </summary>
        [HttpPost]
        public IActionResult CreateTable([FromBody] ApiDtos dto)
        {
            try
            {
                var manager = _storageService.GetDatabaseManager();
                var columns = dto.Columns.Select(c => new Column(c.Name, c.DataType)).ToList();
                var table = manager.CreateTable(dto.TableName, columns);

                return Ok(new { message = $"Таблиця '{dto.TableName}' створена", tableName = table.Name });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Отримати таблицю з даними
        /// </summary>
        [HttpGet("{tableName}")]
        public IActionResult GetTable(string tableName)
        {
            try
            {
                var manager = _storageService.GetDatabaseManager();
                var table = manager.GetTable(tableName);

                return Ok(new
                {
                    name = table.Name,
                    columns = table.Columns.Select(c => new { c.Name, c.DataType }),
                    rows = table.Rows.Select(r => new
                    {
                        id = r.Id,
                        values = r.Values
                    }),
                    rowCount = table.RowCount
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Видалити таблицю
        /// </summary>
        [HttpDelete("{tableName}")]
        public IActionResult DeleteTable(string tableName)
        {
            try
            {
                var manager = _storageService.GetDatabaseManager();
                var result = manager.DeleteTable(tableName);

                if (result)
                    return Ok(new { message = $"Таблиця '{tableName}' видалена" });
                else
                    return NotFound(new { error = $"Таблиця '{tableName}' не знайдена" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Сортувати таблицю за колонкою (ІНДИВІДУАЛЬНА ОПЕРАЦІЯ ⭐)
        /// </summary>
        [HttpPost("{tableName}/sort")]
        public IActionResult SortTable(string tableName, [FromBody] SortRequestDto dto)
        {
            try
            {
                var manager = _storageService.GetDatabaseManager();
                manager.SortTable(tableName, dto.ColumnName, dto.Ascending);

                var direction = dto.Ascending ? "за зростанням" : "за спаданням";
                return Ok(new { message = $"Таблиця '{tableName}' відсортована за '{dto.ColumnName}' {direction}" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}