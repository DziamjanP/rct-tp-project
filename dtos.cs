namespace CourseProject.Dtos
{
    public record RegisterDto(string Name, string? Surname, string Phone, string Password);
    public record LoginDto(string Phone, string Password);
    public record AuthResponseDto(string Token, long UserId, int AccessLevel, string Name, string? Surname, string? Password, string? Phone);
    public record TimetableSearchDto(
    long EntryId,
    string TrainType,
    string Source,
    string Destination,
    DateTime Departure,
    DateTime Arrival
);

}
