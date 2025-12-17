namespace CourseProject.Api.Endpoints;

public static class ApiEndpointMapper
{
    public static RouteGroupBuilder MapAllApiEndpoints(
        this RouteGroupBuilder api)
    {
        api.MapAuth();
        api.MapUsers();
        api.MapPayments();
        api.MapPricePolicies();
        api.MapPerkGroups();
        api.MapTrainTypes();
        api.MapTrains();
        api.MapTimeTableEntries();
        api.MapTicketBookings();
        api.MapStations();
        api.MapTickets();
        api.MapReports();

        return api;
    }
}