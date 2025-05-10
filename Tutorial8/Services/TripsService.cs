using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public class TripsService : ITripsService
{
    private readonly string _connectionString;


    public TripsService(String connectionString)
    {
        _connectionString = connectionString;
    }
    public async Task<List<TripDTO>> GetTripsAsync()
    {
        var trips = new Dictionary<int, TripDTO>();
        
        // Selects trip details and associated country names using LEFT JOINs to include trips even without a country.
        string command = @"SELECT t.IdTrip, t.Description, t.DateFrom, t.DateTo, t.MaxPeople, t.Name as tripName, c.Name as countryName FROM Trip t
LEFT JOIN Country_Trip ct ON t.IdTrip = ct.IdTrip
LEFT JOIN Country c ON ct.IdCountry = c.IdCountry
 ";
        
        await using (SqlConnection conn = new SqlConnection(_connectionString))
        await using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            await conn.OpenAsync();

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var idTrip = reader.GetInt32(reader.GetOrdinal("IdTrip"));
                    var tripName = reader.GetString(reader.GetOrdinal("TripName"));
                    var description = reader.GetString(reader.GetOrdinal("Description"));
                    var dateFrom = reader.GetDateTime(reader.GetOrdinal("DateFrom"));
                    var dateTo = reader.GetDateTime(reader.GetOrdinal("DateTo"));
                    var maxPeople = reader.GetInt32(reader.GetOrdinal("MaxPeople"));
                    var countryName = reader.IsDBNull(reader.GetOrdinal("CountryName")) ? null : reader.GetString(reader.GetOrdinal("CountryName"));

                    if (!trips.ContainsKey(idTrip))
                    {
                        trips[idTrip] = new TripDTO
                        {
                            Id = idTrip,
                            Name = tripName,
                            Description = description,
                            DateFrom = dateFrom,
                            DateTo = dateTo,
                            MaxPeople = maxPeople,
                            Countries = new List<CountryDTO>()
                        };
                    }

                    if (!string.IsNullOrEmpty(countryName))
                    {
                        trips[idTrip].Countries.Add(new CountryDTO{ Name = countryName });
                    }
                    
                }
            }
        }
        

        return trips.Values.ToList();
    }


    public async Task<bool> ClientHasTripsAsync(int idClient)
    {
        // Selects the IDs of trips that a specific client is registered for.
        string command = @" SELECT t.IdTrip FROM Trip t
 JOIN Client_Trip c ON t.IdTrip = c.IdTrip
 WHERE c.IdClient = @idClient";
        await using (SqlConnection conn = new SqlConnection(_connectionString))
        await using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("idClient", idClient);
            await conn.OpenAsync();
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                return reader.HasRows;
            }
        }
    }
    
    public async Task<bool> ClientExistsAsync(int IdClient)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
             await conn.OpenAsync();
             // Counts how many clients exist with the specified ID.
                   string command = @"SELECT COUNT(*) FROM Client WHERE IdClient = @IdClient";
                   SqlCommand checkClient = new SqlCommand(command, conn);
                   checkClient.Parameters.AddWithValue("@IdClient", IdClient);
                   if ((int)await checkClient.ExecuteScalarAsync() == 0)
                   {
                       return false;
                   }

                   return true;
        }
      
    }
    
    public async Task<bool> TripExistsAsync(int IdTrip)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            // Counts how many trips exist with the specified ID.
            string command = @"SELECT COUNT(*) FROM Trip WHERE IdTrip = @IdTrip";
            SqlCommand checkClient = new SqlCommand(command, conn);
            checkClient.Parameters.AddWithValue("@IdTrip", IdTrip);
            if ((int)await checkClient.ExecuteScalarAsync() == 0)
            {
                return false;
            }

            return true;
        }
      
    }
    
    
    
    
    
    public async Task<List<TripByClientDTO>> GetTripByClientAsync(int idClient)
    {
        var trips = new List<TripByClientDTO>();
// Selects trip details and registration info for a specific client.
        string command = @" SELECT t.IdTrip, t.Description, t.DateFrom, t.DateTo, t.MaxPeople, t.Name, ct.RegisteredAt, ct.PaymentDate FROM Trip t
 Join Client_Trip ct ON t.IdTrip = ct.IdTrip
 WHERE ct.idClient = @idClient
";
        
        await using (SqlConnection conn = new SqlConnection(_connectionString))
        await using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@idClient", idClient);
            await conn.OpenAsync();
            await using (var reader = await cmd.ExecuteReaderAsync())
            {

                while (await reader.ReadAsync())
                {
                    var idTrip = reader.GetInt32(reader.GetOrdinal("IdTrip"));
                    var tripName = reader.GetString(reader.GetOrdinal("Name"));
                    var description = reader.GetString(reader.GetOrdinal("Description"));
                    var dateFrom = reader.GetDateTime(reader.GetOrdinal("DateFrom"));
                    var dateTo = reader.GetDateTime(reader.GetOrdinal("DateTo"));
                    var maxPeople = reader.GetInt32(reader.GetOrdinal("MaxPeople"));
                    var registeredAt = reader.GetInt32(reader.GetOrdinal("RegisteredAt"));
                    var paymentDate = reader.IsDBNull(reader.GetOrdinal("PaymentDate")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("PaymentDate"));
                
                    trips.Add(new TripByClientDTO
                    {
                        Id = idTrip,
                        Name = tripName,
                        Description = description,
                        DateFrom = dateFrom,
                        DateTo = dateTo,
                        MaxPeople = maxPeople,
                        RegisteredAt = registeredAt,
                        PaymentDate = paymentDate
                    
                    }); 
                }
             
            }
        }
        
        return trips; 
    }

    public async Task<int?> CreateClientAsync(ClientDTO clientDto)
    {
        // Inserts a new client into the Client table and returns the generated IdClient.
        string command = @"INSERT INTO Client ( FirstName, LastName, Email, Telephone, Pesel)
OUTPUT INSERTED.IdClient
VALUES ( @FirstName, @LastName, @Email, @Telephone, @Pesel)";
        
        using (SqlConnection connection = new SqlConnection(_connectionString))
        using (SqlCommand comm = new SqlCommand(command, connection))
        {   
            await connection.OpenAsync();
            comm.Parameters.AddWithValue("@FirstName", clientDto.FirstName);
            comm.Parameters.AddWithValue("@LastName", clientDto.LastName);
            comm.Parameters.AddWithValue("@Email", clientDto.Email);
            comm.Parameters.AddWithValue("@Telephone", clientDto.Telephone);
            comm.Parameters.AddWithValue("@Pesel", clientDto.Pesel);
            

            var result = await comm.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
    }

    public async Task<(bool succeed, string message)> RegisterClientForTripAsync(int IdClient, int IdTrip)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        { 
            await conn.OpenAsync();
            
            if (!await TripExistsAsync(IdTrip))
            {
                return (false, $"Trip {IdTrip} does not exist");
            }
           
            if (!await ClientExistsAsync(IdClient))
            {
                return (false, $"Client {IdClient} does not exist");
            }
            // Selects the maximum number of people allowed for a specific trip.
            string command = @" SELECT MaxPeople From Trip t
 WHERE t.IdTrip = @IdTrip
";          
            SqlCommand checkMax = new SqlCommand(command, conn);
            checkMax.Parameters.AddWithValue("@IdTrip", IdTrip);
            var max = await checkMax.ExecuteScalarAsync();
            // Counts how many clients are currently registered for a specific trip.
            command = @" SELECT COUNT(*) FROM Client_Trip WHERE IdTrip = @IdTrip";
            SqlCommand checkTripAmount = new SqlCommand(command, conn);
            checkTripAmount.Parameters.AddWithValue("@IdTrip", IdTrip);
            var total = await checkTripAmount.ExecuteScalarAsync();

            if (total == max)
            {
                return (false, $"There are no free places left in Trip {IdTrip}");
            }
            // Inserts a registration record for a client to a trip with the current date.
            command = @"INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt)
                    VALUES (@ClientId, @TripId, @RegisteredAt);";
            SqlCommand insert = new SqlCommand(command, conn);
            insert.Parameters.AddWithValue("@ClientId", IdClient);
            insert.Parameters.AddWithValue("@TripId", IdTrip);
            insert.Parameters.AddWithValue("@RegisteredAt", int.Parse(DateTime.Now.ToString("yyMMdd")));
            
            await insert.ExecuteNonQueryAsync();

        }

      return (true, $"Trip {IdTrip} has been registered for Client {IdClient}");

    }

    public async Task<(bool succeed, string message)> UnregisterClientForTripAsync(int clientId, int tripId)
    {
        if (!await TripExistsAsync(tripId)) return (false, $"Trip {tripId} does not exist");
        if(!await ClientExistsAsync(clientId)) return (false, $"Client {clientId} does not exist");
      
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
              await connection.OpenAsync();
              // Checks if a client is registered for a specific trip.
              string checkClientTrip = "SELECT COUNT(*) FROM Client_Trip WHERE IdTrip = @IdTrip and IdClient = @IdClient";
              SqlCommand check = new SqlCommand(checkClientTrip, connection);
              check.Parameters.AddWithValue("@IdTrip", tripId);
              check.Parameters.AddWithValue("@IdClient", clientId);
              var exists = await check.ExecuteScalarAsync();
              if ((int)exists == 0)
              {
                  return (false, $"There is no Trip {tripId} for client {clientId}");
              }
              // Deletes a client-trip registration entry from the Client_Trip table.
              string command = @"DELETE FROM Client_Trip 
       WHERE IdTrip = @IdTrip and IdClient = @IdClient
";
              SqlCommand delete = new SqlCommand(command, connection);
              delete.Parameters.AddWithValue("@IdTrip", tripId);
              delete.Parameters.AddWithValue("@IdClient", clientId);
              delete.ExecuteNonQuery();
            return (true, $"Trip {tripId} has been unregistered for client {clientId}");
              
        }
        
    }
    
    
}