using CourseProject.Models;

namespace CourseProject.Services
{

    public interface ITicketPricingService
    {
        decimal CalculateTicketPrice(
            PricePolicy policy,
            long fromStationId,
            long toStationId,
            PerkGroup? selectedPerk = null);
    }

    public class TicketPricingService : ITicketPricingService
    {
        private readonly IStationDistanceCalculator _distanceCalculator;

        public TicketPricingService(IStationDistanceCalculator distanceCalculator)
        {
            _distanceCalculator = distanceCalculator;
        }

        public decimal CalculateTicketPrice(
            PricePolicy policy,
            long fromStationId,
            long toStationId,
            PerkGroup? selectedPerk = null)
        {
            decimal fixedPrice = policy.FixedPrice ?? 0m;

            double km = _distanceCalculator.CalculateKmBetween(fromStationId, toStationId);
            int stations = _distanceCalculator.CalculateStationsBetween(fromStationId, toStationId);

            double pricePerKm = policy.PricePerKm ?? 0.0;
            double pricePerStation = policy.PricePerStation ?? 0.0;

            decimal variablePrice = Math.Max(
                (decimal)(pricePerKm * km),
                (decimal)(pricePerStation * stations)
            );

            decimal totalPrice = fixedPrice + variablePrice;

            if (selectedPerk != null)
            {
                if (selectedPerk.Discount == 100m)
                {
                    totalPrice = 0m;
                }
                else if (selectedPerk.Discount.HasValue)
                {
                    decimal discountedPrice = totalPrice * (1 - selectedPerk.Discount.Value / 100m);

                    if (selectedPerk.FixedPrice.HasValue)
                        totalPrice = Math.Min(discountedPrice, selectedPerk.FixedPrice.Value);
                    else
                        totalPrice = discountedPrice;
                }
            }

            return totalPrice;
        }
    }
}
