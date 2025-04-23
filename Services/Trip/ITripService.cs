using Carvisto.Models;

namespace Carvisto.Services
{
    public interface ITripService
    {
        Task<IEnumerable<Trip>> GetAllTripsAsync();
        Task<Trip> GetTripByIdAsync(int id);
        Task CreateTripAsync(Trip trip);
        Task UpdateTripAsync(Trip trip);
        Task DeleteTripAsync(int id);
        bool TripExists(int id);
        IQueryable<Trip> GetTripsQuery();
    }
}

