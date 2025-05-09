using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public interface ITripsService
{
    Task<List<TripDTO>> GetTrips();
    Task<bool> ClientHasTrips(int id);
    Task< List<TripByClientDTO> > GetTripByClient(int id);
    
    Task<int?> CreateClient(ClientDTO clientDto);

    Task<bool> ClientExists(int IdClient);
    
    Task<(bool succeed, string message)> RegisterClientForTrip(int clientId, int tripId);
    Task<(bool succeed, string message)> UnregisterClientForTrip(int clientId, int tripId);
    
}