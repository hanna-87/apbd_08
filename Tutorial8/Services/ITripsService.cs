using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public interface ITripsService
{
    Task<List<TripDTO>> GetTripsAsync();
    Task<bool> ClientHasTripsAsync(int id);
    Task< List<TripByClientDTO> > GetTripByClientAsync(int id);
    
    Task<int?> CreateClientAsync(ClientDTO clientDto);

    Task<bool> ClientExistsAsync(int IdClient);
    
    Task<(bool succeed, string message)> RegisterClientForTripAsync(int clientId, int tripId);
    Task<(bool succeed, string message)> UnregisterClientForTripAsync(int clientId, int tripId);
    
}