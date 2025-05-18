using Carvisto.Data;
using Carvisto.Models;
using Carvisto.Services;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Carvisto.Tests.Services.Trip
{
    [TestFixture]
    public class TripServiceTests
    {
        private CarvistoDbContext _context;
        private ITripService _tripService;
        private string _testDriverId = "test-driver-1";
        private string _testPassengerId = "test-passenger-1";

        [SetUp]
        public async Task Setup()
        {
            // In-memory database setup
            var options = new DbContextOptionsBuilder<CarvistoDbContext>()
                .UseInMemoryDatabase(databaseName: "TripServiceTests")
                .Options;
            
            _context = new CarvistoDbContext(options);
            _tripService = new TripService(_context);
            
            // Add test data
            await SetupTestUsers();
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private async Task SetupTestUsers()
        {
            // Add testing driver
            _context.Users.Add(new ApplicationUser
            {
                Id = _testDriverId,
                UserName = "driver@test.com",
                ContactName = "Test Driver",
                ContactPhone = "1234567890"
            });
            
            // Add testing passenger
            _context.Users.Add(new ApplicationUser
            {
                Id = _testPassengerId,
                UserName = "passanger@test.com",
                ContactName = "Test Passenger",
                ContactPhone = "0987654321"
            });

            await _context.SaveChangesAsync();
        }

        private async Task<Models.Trip> CreateTestTrip(
            string startLocation = "Bialystok",
            string endLocation = "Warszawa",
            DateTime? departureDate = null,
            string vehicleBrand = "Test Brand",
            int seats = 4,
            decimal price = 1000.0m)
        {
            var departure = departureDate ?? DateTime.Now.AddDays(1);

            var trip = new Models.Trip
            {
                DriverId = _testDriverId,
                StartLocation = startLocation,
                EndLocation = endLocation,
                DepartureDateTime = departure,
                Comments = "Test trip",
                VehicleBrand = vehicleBrand,
                Price = price,
                Seats = seats,
                AvailableSeats = seats,
            };
            
            _context.Trips.Add(trip);
            await _context.SaveChangesAsync();
            return trip;
        }

        private async Task<Booking> CreateTestBooking(int tripId, string passenderId = null)
        {
            var booking = new Booking
            {
                TripId = tripId,
                UserId = passenderId ?? _testPassengerId,
                BookingDate = DateTime.Now,
                IsCancelled = false,
            };
            
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            return booking;
        }
        
        [Test]
        [Category("Trip.Create")]
        public async Task CreateTripAsync_ShouldCreateTrip_AndSetAvailableSeats()
        {
            // Arrange
            var newTrip = new Models.Trip
            {
                DriverId = _testDriverId,
                StartLocation = "Bialystok",
                EndLocation = "Warszawa",
                DepartureDateTime = DateTime.Now.AddDays(1),
                Seats = 3,
                Price = 1500,
                Comments = "Test trip",
                VehicleBrand = "Test Brand",
            };
            
            // Act
            await _tripService.CreateTripAsync(newTrip);
            
            // Assert
            var savedTrip = await _context.Trips.FindAsync(newTrip.Id);
            Assert.That(savedTrip, Is.Not.Null);
            Assert.That(savedTrip.AvailableSeats, Is.EqualTo(savedTrip.Seats));
            Assert.That(savedTrip.StartLocation, Is.EqualTo("Bialystok"));
            Assert.That(savedTrip.EndLocation, Is.EqualTo("Warszawa"));
        }

        [Test]
        [Category("Trip.Get")]
        public async Task GetAllTRipsAsync_ShouldReturnAllTrips_WithDriverInfo()
        {
            // Arrange
            await CreateTestTrip("Bialystok", "Warszawa");
            await CreateTestTrip("Krakow", "Gdansk");
            await CreateTestTrip("Wroclaw", "Poznan");
            
            // Act
            var result = await _tripService.GetAllTripsAsync();
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(3));
            Assert.That(result.All(t => t.Driver != null), Is.True, "All trips should have driver info");
        }

        [Test]
        [Category("Trip.Get")]
        public async Task GetTripByIdAsync_ShouldReturnTrip_WthDriverAndBookings()
        {
            // Arrange
            var trip = await CreateTestTrip();
            await CreateTestBooking(trip.Id);
            
            // Act
            var result = await _tripService.GetTripByIdAsync(trip.Id);
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(trip.Id));
            Assert.That(result.Driver, Is.Not.Null, "Trip should have driver info");
            Assert.That(result.Bookings, Is.Not.Null, "Trip should have bookings info");
            Assert.That(result.Bookings.Count, Is.EqualTo(1), "Trip should have one booking");
        }

        [Test]
        [Category("Trip.Get")]
        public void GetTripByIdAsync_ShouldThrowException_WhenTripNotFound()
        {
            // Arrange
            int nonExistentTripId = 999;
            
            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _tripService.GetTripByIdAsync(nonExistentTripId));
        }

        [Test]
        [Category("Trip.Update")]
        public async Task UpdateTripAsync_ShouldUpdateTrip_AndAdjustAvailableSeats()
        {
            // Arrange
            var trip = await CreateTestTrip(seats: 4);
            await CreateTestBooking(trip.Id);
            await CreateTestBooking(trip.Id, "another-passenger-id");
            
            // 4 seats, 2 booked, 2 available
            var tripEntity = await _context.Trips.FindAsync(trip.Id);
            tripEntity.AvailableSeats = 2;
            await _context.SaveChangesAsync();
            
            var tripToUpdate = await _context.Trips.FindAsync(trip.Id);
            tripToUpdate.Seats = 5;
            tripToUpdate.Comments = "Updated trip";
            tripToUpdate.VehicleBrand = "Updated brand";
            
            // Act
            await _tripService.UpdateTripAsync(tripToUpdate);
            
            // Assert
            var updatedTrip = await _context.Trips.FindAsync(trip.Id);
            Assert.That(updatedTrip, Is.Not.Null);
            Assert.That(updatedTrip.Comments, Is.EqualTo("Updated trip"));
            Assert.That(updatedTrip.VehicleBrand, Is.EqualTo("Updated brand"));
            Assert.That(updatedTrip.Seats, Is.EqualTo(5));
            Assert.That(updatedTrip.AvailableSeats, Is.EqualTo(3), "Available seats should be adjusted");
        }

        [Test]
        [Category("Trip.Update")]
        public async Task UpdateTripAsync_ShouldNotAllowNegativeAvailableSeats()
        {
            // Arrange
            var trip = await CreateTestTrip(seats: 4);
            await CreateTestBooking(trip.Id);
            await CreateTestBooking(trip.Id, "another-passenger-id");
            
            // 4 seats, 2 booked, 2 available
            var tripEntity = await _context.Trips.FindAsync(trip.Id);
            tripEntity.AvailableSeats = 1; // Update seats 4 to 1
            await _context.SaveChangesAsync();
            
            var tripToUpdate = await _context.Trips.FindAsync(trip.Id);
            tripToUpdate.Seats = 1; // Update seats 4 to 1
            
            // Act
            await _tripService.UpdateTripAsync(tripToUpdate);
            
            // Assert
            var updatedTrip = await _context.Trips.FindAsync(trip.Id);
            Assert.That(updatedTrip, Is.Not.Null);
            Assert.That(updatedTrip.Seats, Is.EqualTo(1));
            Assert.That(updatedTrip.AvailableSeats, Is.EqualTo(0), "Available seats should be adjusted");
        }

        [Test]
        [Category("Trip.Delete")]
        public async Task DeleteTripAsync_ShouldRemoveTripFromDatabase()
        {
            // Arrange
            var trip = await CreateTestTrip();
            
            // Act
            await _tripService.DeleteTripAsync(trip.Id);
            
            // Assert
            var deletedTrip = await _context.Trips.FindAsync(trip.Id);
            Assert.That(deletedTrip, Is.Null);
        }

        [Test]
        [Category("Trip.Exists")]
        public async Task TripExists_ShouldReturnCorrectValue()
        {
            // Arrange
            var trip = await CreateTestTrip();
            int nonExistentTripId = 999;
            
            // Act & Assert
            Assert.That(_tripService.TripExists(trip.Id), Is.True);
            Assert.That(_tripService.TripExists(nonExistentTripId), Is.False);
        }

        [Test]
        [Category("Trip.Available")]
        public async Task GetAvailableTripsAsync_ShouldFilterCorrectly()
        {
            // Arrange
            var trip1 = await CreateTestTrip(
                "Bialystok",
                "Warszawa",
                DateTime.Now.AddDays(1)
            );

            var trip2 = await CreateTestTrip(
                "Krakow",
                "Gdansk",
                DateTime.Now.AddDays(2),
                seats: 1
            );

            trip2.AvailableSeats = 0;
            await _context.SaveChangesAsync();
            
            // Act - search for available trips
            var result = await _tripService.GetAvailableTripsAsync();
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(1), "Only one trip should be available");
            Assert.That(result.First().Id, Is.EqualTo(trip1.Id), "The available trip should be trip1");
            
            // Act - search startLocation
            result = await _tripService.GetAvailableTripsAsync("Bialystok");
            
            // Assert
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().StartLocation, Is.EqualTo("Bialystok"));
        }

        [Test]
        [Category("Trip.User")]
        public async Task GetUserTripsAsync_ShouldReturnOnlyUserTrips()
        {
            // Arrange
            await CreateTestTrip();
            
            // Add other driver
            var otherDriverId = "other-driver-1";
            _context.Users.Add(new ApplicationUser
            {
                Id = otherDriverId,
                UserName = "other@driver.com",
                ContactName = "Other Driver",
                ContactPhone = "1234567890"
            });
            await _context.SaveChangesAsync();

            var otherTrip = new Models.Trip
            {
                DriverId = otherDriverId,
                StartLocation = "Krakow",
                EndLocation = "Gdansk",
                DepartureDateTime = DateTime.Now.AddDays(2),
                Seats = 4,
                Price = 1500,
                Comments = "Other trip",
                VehicleBrand = "Other Brand",
            };
            _context.Trips.Add(otherTrip);
            await _context.SaveChangesAsync();
            
            // Act
            var result = await _tripService.GetUserTripsAsync(_testDriverId);
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].DriverId, Is.EqualTo(_testDriverId));
        }

        [Test]
        [Category("Trip.Passengers")]
        public async Task GetPassangersUsersAsync_ShouldReturnPassengersForTrip()
        {
            // Arrange
            var trip = await CreateTestTrip();
            
            // Add additional passengers
            string additionalPassengerId = "additional-passenger-1";
            _context.Users.Add(new ApplicationUser
            {
                Id = additionalPassengerId,
                UserName = "additional@passanges.com",
                ContactName = "Additional Passenger",
                ContactPhone = "1234567890"
            });
            await _context.SaveChangesAsync();
            
            // Create bookings
            await CreateTestBooking(trip.Id);
            await CreateTestBooking(trip.Id, additionalPassengerId);
            
            // Create cancell booking
            var cancelledBooking = new Booking
            {
                TripId = trip.Id,
                UserId = "cancelled-passenger-1",
                BookingDate = DateTime.Now,
                IsCancelled = true,
            };
            _context.Bookings.Add(cancelledBooking);
            await _context.SaveChangesAsync();
            
            // Act
            var passengers = await _tripService.GetPassengersUsersAsync(trip.Id);
            
            // Assert
            Assert.That(passengers, Is.Not.Null);
            Assert.That(passengers.Count, Is.EqualTo(2), "Should return 2 passengers");
            Assert.That(passengers.Any(p => p.Id == _testPassengerId), Is.True, "Should include test passenger");
            Assert.That(passengers.Any(p => p.Id == additionalPassengerId), Is.True, "Should include additional passenger");
        }

        [Test]
        [Category("Trip.Query")]
        public async Task GetTripsQuery_ShouldREturnAllTripsWithDriverInfo()
        {
            // Arrange
            await CreateTestTrip("Bialystok", "Warszawa");
            await CreateTestTrip("Krakow", "Gdansk");
            
            // Act
            var query = _tripService.GetTripsQuery();
            var result = await query.ToListAsync();
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2), "Should return 2 trips");
            Assert.That(result.All(t => t.Driver != null), Is.True, "All trips should have driver info");
        }
    }
}