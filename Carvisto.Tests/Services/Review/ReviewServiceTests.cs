using Carvisto.Data;
using Carvisto.Models;
using Carvisto.Services;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Emit;

namespace Carvisto.Tests.Services.Review
{
    [TestFixture]
    public class ReviewServiceTests
    {
        private CarvistoDbContext _context;
        private ReviewService _reviewService;
        
        [SetUp]
        public void Setup()
        {
            // Create an in-memory database for testing
            var options = new DbContextOptionsBuilder<CarvistoDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _context = new CarvistoDbContext(options);
            _reviewService = new ReviewService(_context);
            
            // Seed the database with test data
            SeedDatabase();
        }
        
        [TearDown]
        public void TearDown()
        {
            // Dispose of the context to clean up the in-memory database
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private void SeedDatabase()
        {
            var driver = new ApplicationUser { Id = "driver1", UserName = "driver1@example.com", ContactName = "Driver One", ContactPhone = "1234567890" };
            var reviewer = new ApplicationUser { Id = "reviewer1", UserName = "reviewer1@example.com", ContactName = "Reviewer One", ContactPhone = "0987654321" };

            _context.Users.Add(driver);
            _context.Users.Add(reviewer);
            
            // Add test review
            _context.DriverReviews.Add(new DriverReview
            {
                Id = 1,
                DriverId = "driver1",
                ReviewerId = "reviewer1",
                Rating = 4,
                Comment = "Great driver!",
                CreatedAt = DateTime.Now.AddDays(-1)
            });

            _context.SaveChanges();
        }

        // Test for adding a review
        [Test]
        public async Task GetDriverReviewAsync_ReturnsAllDriverReviews()
        {
            // Arrange
            var driverId = "driver1";
            
            // Act
            var reviews = await _reviewService.GetDriverReviewsAsync(driverId);
            
            // Assert
            Assert.That(reviews, Is.Not.Null);
            Assert.That(reviews.Count(), Is.EqualTo(1));
            Assert.That(reviews.First().Comment, Is.EqualTo("Great driver!"));
        }

        // Test for checking if a user can review a driver
        [Test]
        public async Task CanUserReviewDriverAsync_ReturnsFalse_WhenReviewExists()
        {
            // Arrange
            var driverId = "driver1";
            var reviewerId = "reviewer1";
            
            // Act
            var result = await _reviewService.CanUserReviewDriverAsync(driverId, reviewerId);
            
            // Assert
            Assert.That(result, Is.False);
        }

        // Test for checking if a user can review a driver
        [Test]
        public async Task AddReviewAsync_CreatesNewReview()
        {
            // Arrange
            var driverId = "driver1";
            var reviewerId = "reviewer1";
            var newReview = "This is a new review";
            
            // Act
            var result = await _reviewService.AddReviewAsync(driverId, reviewerId, 5, newReview);
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Comment, Is.EqualTo(newReview));
            Assert.That(result.Rating, Is.EqualTo(5));
            
            // Check if the review is saved in the database
            var reviewsCount = await _context.DriverReviews.CountAsync();
            Assert.That(reviewsCount, Is.EqualTo(2));
        }
    }
}