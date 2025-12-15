using System.Reflection;
using CourseProject.Models;

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
        DateTime ArrivalTime,
        long? PricePolicyId
    );
    public record BuyTicketDto(
        long UserId,
        long EntryId,
        long? PerkGroupId
    );
    public record RefreshTokenDto(
        string RefreshToken
    );
    public record PayLocksRequest(List<long> LockIds);
    public record PriceEstimateResult(long Price);
    public class PaymentReportDto
    {
        public int TotalPayments { get; set; }
        public int SuccessfulPayments { get; set; }
        public decimal AveragePayment { get; set; }
        public decimal TotalRevenue { get; set; }

        public List<TopTrainDto> TopTrains { get; set; } = new();

        public decimal SuccessPercentage =>
            TotalPayments == 0 ? 0 : ((decimal) SuccessfulPayments / TotalPayments) * 100;

        public decimal FailurePercentage => 100 - SuccessPercentage;
    }

    public class TopTrainDto
    {
        public long TrainId { get; set; }
        public int PaymentsCount { get; set; }
    }
}
