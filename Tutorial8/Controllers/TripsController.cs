using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Tutorial8.Models.DTOs;
using Tutorial8.Services;

namespace Tutorial8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripsController : ControllerBase
    {
        private readonly ITripsService _tripsService;

        public TripsController(ITripsService tripsService)
        {
            _tripsService = tripsService;
        }
       
        
        /// <summary>
        /// Endpoint returns the list of all trips alongside with country information
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTrips()
        {
            var trips = await _tripsService.GetTripsAsync();
            return Ok(trips);
        }
        
        /// <summary>
        /// Endpoint returns the list of all trips of a particular client.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTripByClient(int id)
        {
            
            if (id <= 0)
                return BadRequest("Client ID must be a positive integer.");
            
            if (!await _tripsService.ClientExistsAsync(id)) return NotFound("Client not found.");
            
            if( !await _tripsService.ClientHasTripsAsync(id)){
             return NotFound("Client does not have trips.");
            }
            var trips = await _tripsService.GetTripByClientAsync(id);
            return Ok(trips);
            
        }
        /// <summary>
        /// Endpoint creates a new client.
        /// </summary>
       
        [HttpPost("/api/clients")]
        public async Task<IActionResult> CreateClient([FromBody] ClientDTO clientDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdId = await _tripsService.CreateClientAsync(clientDto);
            if (createdId == null)
                return StatusCode(500, "Could not create client.");

            return Created($"/api/clients/{(int)createdId}", new { IdClient = createdId });
        }
        
        /// <summary>
        /// Endpoint creates a new record for registering a particular trip for a specific client.
        /// </summary>
        [HttpPut("/api/clients/{id}/trips/{tripId}")]
        public async Task<IActionResult> RegisterClientForTrip(int id, int tripId)
        {
            
            if (id <= 0 || tripId <= 0)
                return BadRequest("Client ID and Trip ID must be positive integers.");
            var result = await _tripsService.RegisterClientForTripAsync(id, tripId);
            if (!result.succeed)
            {
                if (result.message.Contains("does not exist"))
                    return NotFound(result.message);
                if (result.message.Contains("no free places"))
                    return Conflict(result.message);
                return BadRequest(result.message);
            }

            return Ok(result.message);
        }
        /// <summary>
        /// Endpoint deletes the record of a particular trip registered for a specific client.
        /// </summary>
        [HttpDelete("/api/clients/{id}/trips/{tripId}")]
        public async Task<IActionResult> UnregisterClientForTrip(int id, int tripId)
        {
            
            if (id <= 0 || tripId <= 0)
                return BadRequest("Client ID and Trip ID must be positive integers.");
            var result = await _tripsService.UnregisterClientForTripAsync(id, tripId);
            if (!result.succeed)
            {
                if (result.message.Contains("does not exist") || result.message.Contains("no Trip"))
                    return NotFound(result.message);
                return BadRequest(result.message);
            }
            return Ok(result.message);
        }
        
        
    }
}
