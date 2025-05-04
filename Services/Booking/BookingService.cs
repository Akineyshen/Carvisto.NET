using Carvisto.Data;
using Carvisto.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Carvisto.Services
{
    public class BookingService : IBookingService
    {
        private readonly CarvistoDbContext _context;

        public BookingService(CarvistoDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Booking>> GetUserBookingASync(string userId)
        {
            return await _context.Bookings
                .Include(b => b.Trip)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();
        }

        public async Task<Booking> GetBookingByIdAsync(int id)
        {
            return await _context.Bookings
                .Include(b => b.Trip)
                .FirstOrDefaultAsync(b => b.Id == id) ?? throw new InvalidOperationException();
        }

        public async Task<bool> CreateBookingAsync(int tripId, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Get a ride with a lock for updating
                var trip = await _context.Trips
                    .FirstOrDefaultAsync(t => t.Id == tripId);

                if (trip == null || trip.AvailableSeats <= 0)
                {
                    return false;
                }
                
                // Check if the user has already booked this trip
                bool alreadyBooked = await _context.Bookings
                    .AnyAsync(b => b.TripId == tripId && b.UserId == userId && !b.IsCancelled);

                if (alreadyBooked)
                {
                    return false;
                }
                
                // Create a new booking
                var booking = new Booking
                {
                    TripId = tripId,
                    UserId = userId,
                    BookingDate = DateTime.UtcNow
                };
                
                _context.Bookings.Add(booking);
                
                // Reducing the number of available seats
                trip.AvailableSeats -= 1;
                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> CancelBookingAsync(int bookingId, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var booking = await _context.Bookings
                    .Include(b => b.Trip)
                    .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId && !b.IsCancelled);

                if (booking == null)
                {
                    return false;
                }

                booking.IsCancelled = true;

                // Increasing the number of available seats
                booking.Trip.AvailableSeats += 1;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
    }
}