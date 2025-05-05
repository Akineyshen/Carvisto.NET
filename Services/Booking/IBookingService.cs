using Carvisto.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Carvisto.Services
{
    public interface IBookingService
    {
        Task<IEnumerable<Booking>> GetUserBookingASync(string userId);
        Task<Booking> GetBookingByIdAsync(int bookingId);
        Task<bool> CreateBookingAsync(int tripId, string userId);
        Task<bool> CancelBookingAsync(int bookingId, string userId);
        Task<Booking> GetUserActiveBookingAsync(int tripId, string userId);
    }
}