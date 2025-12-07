namespace PoliHack18.Services;

using PoliHack18.Models;

public interface IFlightService
{
    Task<TripOption?> GetRandomFlight(TripSearchCriteria criteria);

    IEnumerable<string> GetEuropeanAirports();

    Task<FlightBookingResult> CreateFlightOrder(TripOption trip, User user);
}