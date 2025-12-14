namespace CourseProject.Dtos
{
    public record RegisterDto(string Name, string? Surname, string Phone, string Password);
    public record LoginDto(string Phone, string Password);
    public record AuthResponseDto(string Token, long UserId, int AccessLevel, string Name, string? Surname, string? Password, string? Phone);
    public record TimetableSearchDto(
        long EntryId,
        long TrainId,
        string TrainType,
        string DepartureStationName,
        string ArrivalStationName,
        DateTime DepartureTime,
        DateTime ArrivalTime
    );
    public record BuyTicketDto(
        long UserId,
        long EntryId,
        long? PerkGroupId
    );
    public record RefreshTokenDto(
        string RefreshToken
    );
}
