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
                .ThenInclude(t => t.Driver)
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

        public async Task<Booking> GetUserActiveBookingAsync(int tripId, string userId)
        {
            if (string.IsNullOrEmpty(userId) || tripId <= 0)
            {
                return null;
            }

            return await _context.Bookings
                .FirstOrDefaultAsync(b => b.TripId == tripId && b.UserId == userId && !b.IsCancelled);
        }

        public async Task<bool> CreateBookingAsync(int tripId, string userId)
        {
            var existingBooking = await GetUserActiveBookingAsync(tripId, userId);
            
            var cancelledBooking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.TripId == tripId && b.UserId == userId && b.IsCancelled);
            
            if (existingBooking != null)
            {
                return false;
            }
            
            var trip = await _context.Trips.FindAsync(tripId);
            if (trip == null || trip.AvailableSeats <= 0 || trip.DepartureDateTime < DateTime.Now)
            {
                return false;
            }

            if (cancelledBooking != null)
            {
                cancelledBooking.IsCancelled = false;
                cancelledBooking.BookingDate = DateTime.Now;
                
                trip.AvailableSeats -= 1;
                
                await _context.SaveChangesAsync();
                return true;
            }

            var booking = new Booking
            {
                TripId = tripId,
                UserId = userId,
                BookingDate = DateTime.Now,
                IsCancelled = false
            };
            
            trip.AvailableSeats -= 1;
            
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            
            return true;
        }

        public async Task<bool> CancelBookingAsync(int bookingId, string userId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Trip)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);
            
            if (booking == null || booking.IsCancelled || booking.Trip.DepartureDateTime < DateTime.Now)
            {
                return false;
            }
            
            booking.IsCancelled = true;
            
            booking.Trip.AvailableSeats += 1;
            
            await _context.SaveChangesAsync();
            
            return true;
        }
    }
}