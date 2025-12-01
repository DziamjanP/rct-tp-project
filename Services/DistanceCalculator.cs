namespace CourseProject.Services
{   
    public interface IStationDistanceCalculator
    {
        double CalculateKmBetween(long fromStationId, long toStationId);
        int CalculateStationsBetween(long fromStationId, long toStationId);
    }

    public class StationDistanceCalculator : IStationDistanceCalculator
    {
        public double CalculateKmBetween(long fromStationId, long toStationId)
        {
            return 120.0;
        }

        public int CalculateStationsBetween(long fromStationId, long toStationId)
        {
            return 5;
        }
    }
}
