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

        // Test for getting driver rating info
        [Test]
        public async Task GetDriverRatingInfoAsync_ReturnsCorrectAverageAndCount()
        {
            // Arrange
            var driverId = "driver1";
            var reviewerId2 = "reviewer2";

            // Add a new review
            _context.Users.Add(new ApplicationUser
            {
                Id = reviewerId2,
                UserName = "reviewer2@example.com",
                ContactName = "Reviewer Two",
                ContactPhone = "0987654321"
            });

            _context.DriverReviews.Add(new DriverReview
            {
                DriverId = driverId,
                ReviewerId = reviewerId2,
                Rating = 2,
                Comment = "Average service",
                CreatedAt = DateTime.Now
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _reviewService.GetDriverRatingInfoAsync(driverId);

            // Assert
            Assert.That(result.ReviewsCount, Is.EqualTo(2));
            Assert.That(result.AverageRating, Is.EqualTo(3.0));
        }

        // Test for getting driver rating info when no reviews exist
        [Test]
        public async Task GetDriverRatingInfoAsync_ReturnsZeroForNoReviews()
        {
            // Arrange
            var driverId = "nonexistent";
            
            // Act
            var result = await _reviewService.GetDriverRatingInfoAsync(driverId);

            // Assert
            Assert.That(result.ReviewsCount, Is.EqualTo(0));
            Assert.That(result.AverageRating, Is.EqualTo(0));
        }

        // Test for deleting a review when the user is the reviewer
        [Test]
        public async Task DeleteReviewAsync_ReturnsTrueAndDeletesReview_WhenUserIsReviewer()
        {
            // Arrange
            var reviewerId = "reviewer1";
            var review = await _context.DriverReviews.FirstAsync();

            // Act
            var result = await _reviewService.DeleteReviewAsync(review.Id, reviewerId, false);

            // Assert
            Assert.That(result, Is.True);
            var reviewExists = await _context.DriverReviews.AnyAsync(r => r.Id == review.Id);
            Assert.That(reviewExists, Is.False);
        }

        // Test for deleting a review when the user is a moderator
        [Test]
        public async Task DeleteReviewAsync_ReturnsTrueAndDeletesReview_WhenUserIdIsModerator()
        {
            // Arrange
            var moderatorId = "moderator1";
            var review = await _context.DriverReviews.FirstAsync();

            // Act
            var result = await _reviewService.DeleteReviewAsync(review.Id, moderatorId, true);

            // Assert
            Assert.That(result, Is.True);
            var reviewExists = await _context.DriverReviews.AnyAsync(r => r.Id == review.Id);
            Assert.That(reviewExists, Is.False);
        }

        // Test for deleting a review when the review does not exist
        [Test]
        public async Task DeleteReviewAsync_ReturnsFalse_WhenReviewDoesNotExist()
        {
            var nonExistentReviewId = 999;
            var userId = "reviewer1";

            // Act
            var result = await _reviewService.DeleteReviewAsync(nonExistentReviewId, userId, false);

            // Assert
            Assert.That(result, Is.False);
        }

        //
        [Test]
        public async Task DeleteReviewAsync_ReturnsFalse_WhenUserIsNotReviewerOrModerator()
        {
            // Arrange
            var review = await _context.DriverReviews.FirstAsync();
            var unauthorizedUserId = "unauthorized";

            // Act
            var result = await _reviewService.DeleteReviewAsync(review.Id, unauthorizedUserId, false);

            // Assert
            Assert.That(result, Is.False);
            var reviewExists = await _context.DriverReviews.AnyAsync(r => r.Id == review.Id);
            Assert.That(reviewExists, Is.True);
        }
    }
}