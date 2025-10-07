using DatabaseCore.Models;

namespace DatabaseServer.DTOs
{
    public class CreateDatabaseDto
    {
        public string Name { get; set; } = string.Empty;
    }

    public class CreateTableDto
    {
        public string TableName { get; set; } = string.Empty;
        public List<ColumnDto> Columns { get; set; } = new();
    }

    public class ColumnDto
    {
        public string Name { get; set; } = string.Empty;
        public DataType DataType { get; set; }
    }

    public class CreateRowDto
    {
        public Dictionary<string, object?> Values { get; set; } = new();
    }

    public class UpdateRowDto
    {
        public Dictionary<string, object?> Values { get; set; } = new();
    }

    public class SortRequestDto
    {
        public string ColumnName { get; set; } = string.Empty;
        public bool Ascending { get; set; } = true;
    }

    public class SaveDatabaseDto
    {
        public string FilePath { get; set; } = string.Empty;
    }

    public class LoadDatabaseDto
    {
        public string FilePath { get; set; } = string.Empty;
    }
}