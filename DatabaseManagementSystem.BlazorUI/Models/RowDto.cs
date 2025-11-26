namespace DatabaseManagementSystem.BlazorUI.Models
{
    public class RowDto
    {
        public int Id { get; set; }
        public Dictionary<string, object?> Values { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }

    public class CreateRowRequest
    {
        public Dictionary<string, object?> Values { get; set; } = new();
    }

    public class UpdateRowRequest
    {
        public Dictionary<string, object?> Values { get; set; } = new();
    }
}
