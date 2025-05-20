using Carvisto.Data;
using Carvisto.Models;
using Carvisto.Models.ViewModels;
using Carvisto.Services;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Carvisto.Models.ViewModels;

namespace Carvisto.Tests.Services.Booking
{
    [TestFixture]
    public class BookingServiceTests
    {
        private CarvistoDbContext _context;
        private IBookingService _bookingService;
        private string _testUserId = "test-user-id";
        private string _testDriverId = "test-driver-id";
        private string _anotherUserId = "another-user-id";

        [SetUp]
        public async Task Setup()
        {
            // Use unique database name for each test
            var options = new DbContextOptionsBuilder<CarvistoDbContext>()
                .UseInMemoryDatabase(databaseName: $"BookingServiceTests-{Guid.NewGuid()}")
                .Options;
            
            _context = new CarvistoDbContext(options);
            _bookingService = new BookingService(_context);
            
            // Test data
            await SeedTestDataAsync();
        }
        
        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private async Task SeedTestDataAsync()
        {
            // Create test users
            var testUser = new ApplicationUser()
            {
                Id = _testUserId,
                Email = "testuser@example.com",
                ContactName = "Test User",
                ContactPhone = "1234567890"
            };

            var testDriver = new ApplicationUser
            {
                Id = _testDriverId,
                Email = "testdriver@example.com",
                ContactName = "Test Driver",
                ContactPhone = "0987654321"
            };

            var anotherUser = new ApplicationUser
            {
                Id = _anotherUserId,
                Email = "another@example.com",
                ContactName = "Another User",
                ContactPhone = "1234567890"
            };
            
            _context.Users.AddRange(testUser, testDriver, anotherUser);
            
            // Create test trips
            var activeTrip = new Models.Trip
            {
                Id = 1,
                DriverId = _testDriverId,
                StartLocation = "Bialystok",
                EndLocation = "Warsaw",
                DepartureDateTime = DateTime.Now.AddDays(1),
                Comments = "Test trip",
                Seats = 3,
                AvailableSeats = 2,
                Price = 100,
                VehicleBrand = "Test Brand",
            };

            var pastTrip = new Models.Trip
            {
                Id = 2,
                DriverId = _testDriverId,
                StartLocation = "Lodz",
                EndLocation = "Warsaw",
                DepartureDateTime = DateTime.Now.AddDays(-2),
                Comments = "Past trip",
                Seats = 4,
                AvailableSeats = 3,
                Price = 50,
                VehicleBrand = "Test Brand"
            };

            var fullTrip = new Models.Trip
            {
                Id = 3,
                DriverId = _testDriverId,
                StartLocation = "Gdansk",
                EndLocation = "Warsaw",
                DepartureDateTime = DateTime.Now.AddDays(3),
                Comments = "Full trip",
                Seats = 2,
                AvailableSeats = 0,
                Price = 150,
                VehicleBrand = "Test Brand"
            };
            
            _context.Trips.AddRange(activeTrip, pastTrip, fullTrip);
            
            // Create test bookings
            var activeBooking = new Models.Booking
            {
                Id = 1,
                TripId = activeTrip.Id,
                UserId = _testUserId,
                BookingDate = DateTime.Now.AddDays(-1),
                IsCancelled = false,
            };
            
            var cancelledBooking = new Models.Booking
            {
                Id = 2,
                TripId = activeTrip.Id,
                UserId = _anotherUserId,
                BookingDate = DateTime.Now.AddDays(-1),
                IsCancelled = true,
            };
            
            var pastBooking = new Models.Booking
            {
                Id = 3,
                TripId = pastTrip.Id,
                UserId = _testUserId,
                BookingDate = DateTime.Now.AddDays(-3),
                IsCancelled = false,
            };
            
            _context.Bookings.AddRange(activeBooking, cancelledBooking, pastBooking);
            
            await _context.SaveChangesAsync();
        }
        
        [Test]
        [Category("Booking.GetUserBookings")]
        public async Task GetUserBookingAsync_WithVaildUserId_ShouldReturnUserBookings()
        {
            // Act
            var result = await _bookingService.GetUserBookingASync(_testUserId);
            
            // Assert
            Assert.That(result, Is.Not.Null);
            var bookings = result.ToList();
            Assert.That(bookings.Count, Is.EqualTo(2));
            Assert.That(bookings.All(b => b.UserId == _testUserId), Is.True);
            Assert.That(bookings.Any(b => b.Trip.StartLocation == "Bialystok" && b.Trip.EndLocation == "Warsaw"), Is.True);
            Assert.That(bookings.Any(b => b.Trip.Driver != null), Is.True);
        }

        [Test]
        [Category("Booking.GetUserBookings")]
        public async Task GetUSerBookingAsync_WithNonExistentUserId_ShouldReturnEmptyList()
        {
            // Arrange
            string nonExistentUserId = "non-existent-user-id";
            
            // Act
            var result = await _bookingService.GetUserBookingASync(nonExistentUserId);
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        [Category("Booking.GetBookingById")]
        public async Task GetBookingByIdAsync_WithValidId_ShouldReturnBooking()
        {
            // Arrange
            int bookingId = 1;
            
            // Act
            var result = await _bookingService.GetBookingByIdAsync(bookingId);
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(bookingId));
            Assert.That(result.UserId, Is.EqualTo(_testUserId));
            Assert.That(result.Trip, Is.Not.Null);
        }
        
        [Test]
        [Category("Booking.GetBookingById")]
        public void GetBookingByIdAsync_WithInvalidId_ShouldThrowException()
        {
            // Arrange
            int invalidBookingId = 999;
            
            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => 
                await _bookingService.GetBookingByIdAsync(invalidBookingId));
        }

        [Test]
        [Category("Booking.GetUserActiveBooking")]
        public async Task GetUserActiveBookingAsync_WithValidData_ShouldReturnActiveBooking()
        {
            // Arrange
            int tripId = 1;
            
            // Act
            var result = await _bookingService.GetUserActiveBookingAsync(tripId, _testUserId);
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.TripId, Is.EqualTo(tripId));
            Assert.That(result.UserId, Is.EqualTo(_testUserId));
            Assert.That(result.IsCancelled, Is.False);
        }

        [Test]
        [Category("Booking.GetUserActiveBooking")]
        public async Task GetUserActiveBookingAsync_WithCancelledBooking_ShouldReturnNull()
        {
            // Arrange
            int tripId = 1;
            
            // Act
            var result = await _bookingService.GetUserActiveBookingAsync(tripId, _anotherUserId);
            
            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        [Category("Booking.GetUserActiveBooking")]
        public async Task GetUserActiveBookingAsync_WithInvalidTripId_ShouldReturnNull()
        {
            // Arrange
            int invalidTripId = 999;
            
            // Act
            var result = await _bookingService.GetUserActiveBookingAsync(invalidTripId, _testUserId);
            
            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        [Category("Booking.GetUserActiveBooking")]
        public async Task GetUserActiveBookingAsync_WithNullUserId_ShouldReturnNull()
        {
            // Act
            var result = await _bookingService.GetUserActiveBookingAsync(1, null);
            
            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        [Category("Booking.CreateBooking")]
        public async Task CreateBookingAsync_WithValidData_ShouldCreateBookingAndReturnTrue()
        {
            // Arrange
            int tripId = 1;
            
            // Initial count of AvailableSeats
            var tripBefore = await _context.Trips.FindAsync(tripId);
            int initialAvailableSeats = tripBefore.AvailableSeats;
            
            // Act
            var result = await _bookingService.CreateBookingAsync(tripId, _anotherUserId);
            
            // Assert
            Assert.That(result, Is.True);
            
            // Check if booking was created
            var createdBooking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.TripId == tripId && b.UserId == _anotherUserId && !b.IsCancelled);
            Assert.That(createdBooking, Is.Not.Null);
            
            // Check if AvailableSeats was decremented
            var tripAfter = await _context.Trips.FindAsync(tripId);
            Assert.That(tripAfter.AvailableSeats, Is.EqualTo(initialAvailableSeats - 1));
        }

        [Test]
        [Category("Booking.CreateBooking")]
        public async Task CreateBookingAsync_WithExistingActiveBooking_ShouldReturnFalse()
        {
            // Arrange
            int tripId = 1;
            
            // Act
            var result = await _bookingService.CreateBookingAsync(tripId, _testUserId);
            
            // Assert
            Assert.That(result, Is.False);
        }
        
        [Test]
        [Category("Booking.CreateBooking")]
        public async Task CreateBookingAsync_WithFullTrip_ShouldReturnFalse()
        {
            // Arrange
            int tripId = 3; // Full trip
            
            // Act
            var result = await _bookingService.CreateBookingAsync(tripId, _anotherUserId);
            
            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        [Category("Booking.CreateBooking")]
        public async Task CreateBookingAsync_WithPastTrip_ShouldReturnFalse()
        {
            // Arrange
            int tripId = 2; // Past trip
            
            // Act
            var result = await _bookingService.CreateBookingAsync(tripId, _anotherUserId);
            
            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        [Category("Booking.CreateBooking")]
        public async Task CreateBookingAsync_WithReactivatingCancelledBooking_ShouldReactivateAndReturnTrue()
        {
            // Arrange
            int tripId = 1;
            
            // Initial count of AvailableSeats
            var tripBefore = await _context.Trips.FindAsync(tripId);
            int initialAvailableSeats = tripBefore.AvailableSeats;
            
            // Act
            var result = await _bookingService.CreateBookingAsync(tripId, _anotherUserId);
            
            // Assert
            Assert.That(result, Is.True);
            
            // Check if booking was reactivated
            var reactivatedBooking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.TripId == tripId && b.UserId == _anotherUserId && !b.IsCancelled);
            Assert.That(reactivatedBooking, Is.Not.Null);
            Assert.That(reactivatedBooking.IsCancelled, Is.False);
            
            // Check if AvailableSeats was decremented
            var tripAfter = await _context.Trips.FindAsync(tripId);
            Assert.That(tripAfter.AvailableSeats, Is.EqualTo(initialAvailableSeats - 1));
        }

        [Test]
        [Category("Booking.CancelBooking")]
        public async Task CancelBookingAsync_WithValidData_ShouldCancelBookingAndReturnTrue()
        {
            // Arrange
            int bookingId = 1; // Active booking
            
            // Initial count of AvailableSeats
            var booking = await _context.Bookings
                .Include(b => b.Trip)
                .FirstOrDefaultAsync(b => b.Id == bookingId);
            int initialAvailableSeats = booking.Trip.AvailableSeats;
            
            // Act
            var result = await _bookingService.CancelBookingAsync(bookingId, _testUserId);
            
            // Assert
            Assert.That(result, Is.True);
            
            // Check if booking was cancelled
            var cancelledBooking = await _context.Bookings.FindAsync(bookingId);
            Assert.That(cancelledBooking, Is.Not.Null);
            
            // Check if AvailableSeats was incremented
            var tripAfter = await _context.Trips.FindAsync(booking.TripId);
            Assert.That(tripAfter.AvailableSeats, Is.EqualTo(initialAvailableSeats + 1));
        }

        [Test]
        [Category("Booking.CancelBooking")]
        public async Task CancelBookingAsync_WithInvalidUserId_ShouldReturnFalse()
        {
            // Arrange
            int bookingId = 1; // Active booking
            string wrongUserId = _anotherUserId;
            
            // Act
            var result = await _bookingService.CancelBookingAsync(bookingId, wrongUserId);
            
            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        [Category("Booking.CancelBooking")]
        public async Task CancelBookingAsync_WithAlreadyCancelledBooking_ShouldReturnFalse()
        {
            // Arrange
            int bookingId = 2; // Cancelled booking
            
            // Act
            var result = await _bookingService.CancelBookingAsync(bookingId, _anotherUserId);
            
            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        [Category("Booking.CancelBooking")]
        public async Task CancelBookingAsync_WithPastTrip_ShouldReturnFalse()
        {
            // Arrange
            int bookingId = 3; // Past booking
            
            // Act
            var result = await _bookingService.CancelBookingAsync(bookingId, _testUserId);
            
            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        [Category("Booking.GetBookingReceipt")]
        public async Task GetBooikngReceiptAsync_WithValidData_ShouldReturnReceipt()
        {
            // Arrange
            int bookingId = 1; // Active booking
            
            // Act
            var result = await _bookingService.GetBookingReceiptAsync(bookingId, _testUserId);
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.BookingId, Is.EqualTo(bookingId));
            Assert.That(result.PassengerName, Is.EqualTo("Test User"));
            Assert.That(result.DriverName, Is.EqualTo("Test Driver"));
            Assert.That(result.StartLocation, Is.EqualTo("Bialystok"));
            Assert.That(result.EndLocation, Is.EqualTo("Warsaw"));
            Assert.That(result.BookingStatus, Is.EqualTo("Active"));
            Assert.That(result.BookingReference, Is.EqualTo($"BK-{bookingId:D6}"));
        }

        [Test]
        [Category("Booking.GetBookingReceipt")]
        public async Task GetBookingReceiptAsync_WithInvalidBookingId_ShouldReturnNull()
        {
            // Arrange
            int invalidBookingId = 999; // Non-existent booking
            
            // Act
            var result = await _bookingService.GetBookingReceiptAsync(invalidBookingId, _testUserId);
            
            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        [Category("Booking.GetBookingReceipt")]
        public async Task GetBookingReceiptAsync_WithWrongUserId_ShouldReturnNull()
        {
            // Arrange
            int bookingId = 1; // Active booking
            string wrongUserId = _anotherUserId;
            
            // Act
            var result = await _bookingService.GetBookingReceiptAsync(bookingId, wrongUserId);
            
            // Assert
            Assert.That(result, Is.Null);
        }
    }
}