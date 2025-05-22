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
                .Include(t => t.Bookings)
                .FirstOrDefaultAsync(t => t.Id == id) ?? throw new InvalidOperationException("Trip not found");
        }
        
        // Create new trip
        public async Task CreateTripAsync(Trip trip)
        {
            trip.AvailableSeats = trip.Seats;
            _context.Add(trip);
            await _context.SaveChangesAsync();
        }

        // Update trip
        public async Task UpdateTripAsync(Trip trip)
        {
            var existingTrip = await _context.Trips
                .FirstOrDefaultAsync(t => t.Id == trip.Id);

            if (existingTrip == null)
            {
                throw new InvalidOperationException("Trip not found");
            }
            
            var activeBookings = await _context.Bookings
                .Where(b => b.TripId == trip.Id && !b.IsCancelled)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();
            
            int bookedSeats = activeBookings.Count;
            
            int oldSeats = existingTrip.Seats;
            
            _context.Entry(existingTrip).CurrentValues.SetValues(trip);

            if (trip.Seats < bookedSeats)
            {
                int bookingsToCancel = bookedSeats - trip.Seats;
                
                for (int i = 0; i < bookingsToCancel && i < activeBookings.Count; i++)
                {
                    activeBookings[i].IsCancelled = true;
                }

                existingTrip.AvailableSeats = 0;
            }
            else
            {
                existingTrip.AvailableSeats = trip.Seats - bookedSeats;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Trips.AnyAsync(t => t.Id == trip.Id))
                {
                    throw new InvalidOperationException("Trip not found");
                }

                throw;
            }
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

        public IQueryable<Trip> GetTripsQuery()
        {
            return _context.Trips.Include(t => t.Driver);
        }
        
        public async Task<IEnumerable<Trip>> GetAvailableTripsAsync(string startLocation = null, string endLocation = null, DateTime? departureDate = null)
        {
            var query = _context.Trips
                .Include(t => t.Driver)
                .Where(t => t.AvailableSeats > 0);

            if (!string.IsNullOrEmpty(startLocation))
            {
                query = query.Where(t => t.StartLocation.Contains(startLocation));
            }

            if (!string.IsNullOrEmpty(endLocation))
            {
                query = query.Where(t => t.EndLocation.Contains(endLocation));
            }

            if (departureDate.HasValue)
            {
                var date = departureDate.Value.Date;
                query = query.Where(t => t.DepartureDateTime.Date == date);
            }
            
            return await query.ToListAsync();
        }

        public async Task<List<Trip>> GetUserTripsAsync(string userId)
        {
            return await _context.Trips
                .Where(t => t.DriverId == userId)
                .Include(t => t.Driver)
                .OrderByDescending(t => t.DepartureDateTime)
                .ToListAsync();
        }
        
        public async Task<List<ApplicationUser>> GetPassengersUsersAsync(int tripId)
        {
            var trip = await _context.Trips
                .Include(t => t.Bookings)
                .ThenInclude(b => b.User)
                .FirstOrDefaultAsync(t => t.Id == tripId);

            return trip?.Bookings
                .Where(b => !b.IsCancelled)
                .Select(b => b.User)
                .ToList() ?? new List<ApplicationUser>();
        }
    }
}