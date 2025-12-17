namespace CourseProject.Dtos;

public record TimetableSearchDto(
    long EntryId,
    long TrainId,
    string TrainType,
    string DepartureStationName,
    string ArrivalStationName,
    DateTime DepartureTime,
    DateTime ArrivalTime,
    long? PricePolicyId
);
