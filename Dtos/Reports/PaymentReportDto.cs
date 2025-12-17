namespace CourseProject.Dtos;

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
