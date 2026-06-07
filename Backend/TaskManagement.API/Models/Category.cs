using System.Text.Json.Serialization;

namespace TaskManagement.API.Models
{
    public class Category
    {
        [JsonRequired]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}