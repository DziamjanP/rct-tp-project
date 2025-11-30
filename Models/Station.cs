using System.Text.Json.Serialization;

namespace CourseProject.Models;

public class Station
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string CityName { get; set; } = null!;
    public string? Description { get; set; }
    [JsonIgnore]
    public ICollection<Train>? SourceTrains { get; set; }
    [JsonIgnore]
    public ICollection<Train>? DestinationTrains { get; set; }
}
