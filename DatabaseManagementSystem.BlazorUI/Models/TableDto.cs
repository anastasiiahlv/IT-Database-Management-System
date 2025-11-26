namespace DatabaseManagementSystem.BlazorUI.Models
{
    public class TableDto
    {
        public string Name { get; set; } = string.Empty;
        public List<ColumnDto> Columns { get; set; } = new();
        public List<RowDto> Rows { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }

    public class ColumnDto
    {
        public string Name { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty; // integer, real, char, string, $, $Invl
        public bool IsNullable { get; set; }
        public object? DefaultValue { get; set; }
    }

    public class CreateTableRequest
    {
        public string DatabaseName { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public List<ColumnDto> Columns { get; set; } = new();
    }

    public class UpdateTableRequest
    {
        public string NewName { get; set; } = string.Empty;
        public List<ColumnDto>? Columns { get; set; }
    }

    public class SortTableRequest
    {
        public string ColumnName { get; set; } = string.Empty;
        public bool Ascending { get; set; } = true;
    }
}
