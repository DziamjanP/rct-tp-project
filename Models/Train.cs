using System.Text.Json.Serialization;

namespace CourseProject.Models;

public class Train
{
    public long Id { get; set; }
    public long SourceId { get; set; }
    public long DestinationId { get; set; }
    public long TypeId { get; set; }

    public Station Source { get; set; } = null!;
    public Station Destination { get; set; } = null!;
    public TrainType Type { get; set; } = null!;
    [JsonIgnore]
    public ICollection<TimeTableEntry>? TimeTableEntries { get; set; }
}
