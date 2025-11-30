namespace CourseProject.Models;

using System.Text.Json.Serialization;

public class PricePolicy
{
    public long Id { get; set; }

    public double? PricePerKm { get; set; }
    public double? PricePerStation { get; set; }
    public decimal? FixedPrice { get; set; }

    // normalized M:N mapping
    public ICollection<PricePolicyPerkGroup>? PerkGroups { get; set; }

    [JsonIgnore]
    public ICollection<TimeTableEntry>? TimeTableEntries { get; set; }
}
