using System;
using System.Text.Json;

namespace CourseProject.Models;

public class TimeTableEntry
{
    public long Id { get; set; }
    public long TrainId { get; set; }
    public DateTime Departure { get; set; }
    public DateTime Arrival { get; set; }

    public long PricePolicyId { get; set; }

    public Train Train { get; set; } = null!;
    public PricePolicy PricePolicy { get; set; } = null!;
}
