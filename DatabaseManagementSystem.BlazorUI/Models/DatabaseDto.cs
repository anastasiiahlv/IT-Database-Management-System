namespace DatabaseManagementSystem.BlazorUI.Models
{
    public class DatabaseDto
    {
        public string Name { get; set; } = string.Empty;
        public List<TableDto> Tables { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }

    public class CreateDatabaseRequest
    {
        public string Name { get; set; } = string.Empty;
    }
}
