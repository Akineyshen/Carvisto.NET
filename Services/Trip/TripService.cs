using Carvisto.Data;
using Carvisto.Models;
using Microsoft.EntityFrameworkCore;

namespace Carvisto.Services
{
    public class TripService : ITripService
    {
        // DB context
        private readonly CarvistoDbContext _context;

        // DI constructor
        public TripService(CarvistoDbContext context)
        {
            _context = context;
        }

        // GET all trips with driver info
        public async Task<IEnumerable<Trip>> GetAllTripsAsync()
        {
            return await _context.Trips
                .Include(t => t.Driver)
                .ToListAsync();
        }
    
        // GET trip by ID + driver info
        public async Task<Trip> GetTripByIdAsync(int id)
        {
            return await _context.Trips
                .Include(t => t.Driver)
                .FirstOrDefaultAsync(t => t.Id == id) ?? throw new InvalidOperationException("Trip not found");
        }
        
        // Create new trip
        public async Task CreateTripAsync(Trip trip)
        {
            _context.Trips.Add(trip);
            await _context.SaveChangesAsync();
        }

        // Update existing trip
        public async Task UpdateTripAsync(Trip trip)
        {
            var existingTrip = await _context.Trips.FindAsync(trip.Id);
            if (existingTrip != null)
            {
                _context.Entry(existingTrip).State = EntityState.Detached;
            }
            
            _context.Update(trip);
            await _context.SaveChangesAsync();
        }

        // Delete trip by ID
        public async Task DeleteTripAsync(int id)
        {
            var trip = await _context.Trips.FindAsync(id);
            if (trip != null)
            {
                _context.Trips.Remove(trip);
                await _context.SaveChangesAsync();
            }
        }

        // Check if trip exists
        public bool TripExists(int id)
        {
            return _context.Trips.Any(e => e.Id == id);
        }
    }
}