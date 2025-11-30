using System.Text.Json.Serialization;

namespace CourseProject.Models;

public class TrainType
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    [JsonIgnore]
    public ICollection<Train>? Trains { get; set; }
}
