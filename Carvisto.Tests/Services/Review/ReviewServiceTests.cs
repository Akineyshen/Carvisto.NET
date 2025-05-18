using Carvisto.Data;
using Carvisto.Models;
using Carvisto.Services;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

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

        // Test for deleting a review when the user is not the reviewer or moderator
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

        // Test for adding a review and updating the driver's rating info
        [Test]
        public async Task AddReviewAsync_UpdatesDriverRatingInfo()
        {
            // Arrange
            var driverId = "driver1";
            var reviewerId = "reviewer2";

            _context.Users.Add(new ApplicationUser
            {
                Id = reviewerId,
                UserName = "reviewer2@example.com",
                ContactName = "Reviewer Two",
                ContactPhone = "0987654321"
            });
            await _context.SaveChangesAsync();
            
            var initialRantingInfo = await _reviewService.GetDriverRatingInfoAsync(driverId);
            
            // Act
            await _reviewService.AddReviewAsync(driverId, reviewerId, 2, "Bellow avarage service");
            
            // Assert
            var updatedRatingInfo = await _reviewService.GetDriverRatingInfoAsync(driverId);
            Assert.That(updatedRatingInfo.AverageRating, Is.EqualTo(3.0)); // (4 + 2) / 2 = 3
            Assert.That(updatedRatingInfo.ReviewsCount, Is.EqualTo(2));
        }

        // Test for adding a review with minimum rating
        [Test]
        public async Task AddReviewAsync_WithMinimumRating_CreatesReviewSuccessfully()
        {
            // Arrange
            var driverId = "driver1";
            var reviewerId = "reviewer2";

            _context.Users.Add(new ApplicationUser
            {
                Id = reviewerId,
                UserName = "reviewer2@example.com",
                ContactName = "Reviewer Two",
                ContactPhone = "0987654321"
            });
            await _context.SaveChangesAsync();
            
            // Act
            var result = await _reviewService.AddReviewAsync(driverId, reviewerId, 1, "Very pool service");
            
            // Assert
            Assert.That(result.Rating, Is.EqualTo(1));
            
            var savedReview = await _context.DriverReviews
                .FirstOrDefaultAsync(r => r.DriverId == driverId && r.ReviewerId == reviewerId);
            Assert.That(savedReview, Is.Not.Null);
            Assert.That(savedReview.Rating, Is.EqualTo(1));
        }

        // Test for adding a review with maximum rating
        [Test]
        public async Task GetDriverReviewsAsync_ReturnsSortedByCreateAtDescending()
        {
            // Arrange
            var driverId = "driver1";
            var reviewerId2 = "reviewer2";

            _context.Users.Add(new ApplicationUser
            {
                Id = reviewerId2,
                UserName = "reviewer2@example.com",
                ContactName = "Reviewer Two",
                ContactPhone = "0987654321"
            });

            var newerReview = new DriverReview
            {
                DriverId = driverId,
                ReviewerId = reviewerId2,
                Rating = 5,
                Comment = "Excellent service",
                CreatedAt = DateTime.Now
            };
            
            _context.DriverReviews.Add(newerReview);
            await _context.SaveChangesAsync();
            
            // Act
            var reviews = (await _reviewService.GetDriverReviewsAsync(driverId)).ToList();
            
            // Assert
            Assert.That(reviews.Count, Is.EqualTo(2));
            Assert.That(reviews[0].CreatedAt, Is.GreaterThan(reviews[1].CreatedAt));
        }

        // Test for get driver reviews when no reviews exist
        [Test]
        public async Task GetDriverReviewsAsync_ReturnsEmptyList_WhenNoReviewsExist()
        {
            // Arrange
            var nonExistentDriverId = "nonexistent";
            
            // Act
            var reviews = await _reviewService.GetDriverReviewsAsync(nonExistentDriverId);
            
            // Assert
            Assert.That(reviews, Is.Not.Null);
            Assert.That(reviews, Is.Empty);
        }

        // Test for getting driver reviews with reviewer details
        [Test]
        public async Task GetDriverReviewsAsync_IncudesReviewerDetails()
        {
            // Arrange
            var driverId = "driver1";
            
            // Act
            var reviews = (await _reviewService.GetDriverReviewsAsync(driverId)).ToList();
            
            // Assert
            Assert.That(reviews.Count, Is.EqualTo(1));
            Assert.That(reviews[0].Reviewer, Is.Not.Null);
            Assert.That(reviews[0].Reviewer.ContactName, Is.EqualTo("Reviewer One"));
        }

        // Test for checking if a user can review a driver when the user does not exist
        [Test]
        public async Task CanUserReviewDriverAsync_ReturnsTrue_WhenUserOrDriverDoesNotExist()
        {
            // Arrange
            var nonExistentDriverId = "nonexistent-driver";
            var nonExistentUserId = "nonexistent-user";
            
            // Act
            var result = await _reviewService.CanUserReviewDriverAsync(nonExistentDriverId, nonExistentUserId);
            
            // Assert
            Assert.That(result, Is.True);
        }

        // Test for multiple reviews for the same driver
        [Test]
        public async Task GetDriverRatingInfoAsync_CalculatesCorrectAverageForMultipleReviews()
        {
            // Arrange
            var driverId = "driver2";
            var driverUser = new ApplicationUser
            {
                Id = driverId,
                UserName = "driver2@example.com",
                ContactName = "Driver Two",
                ContactPhone = "1234567890"
            };
            _context.Users.Add(driverUser);
            
            // Add multiple reviews
            for (int i = 1; i <= 5; i++)
            {
                var reviewerId = $"multi-reviewer{i}";
                _context.Users.Add(new ApplicationUser
                {
                    Id = reviewerId,
                    UserName = $"multireviewer{i}@example.com",
                    ContactName = $"Multi Reviewer {i}",
                    ContactPhone = $"000000000{i}"
                });
                
                _context.DriverReviews.Add(new DriverReview
                {
                    DriverId = driverId,
                    ReviewerId = reviewerId,
                    Rating = i,
                    Comment = $"Review {i}",
                    CreatedAt = DateTime.Now.AddHours(-i)
                });
                await _context.SaveChangesAsync();
            }
            
            // Act
            var result = await _reviewService.GetDriverRatingInfoAsync(driverId);
            
            // Assert
            Assert.That(result.ReviewsCount, Is.EqualTo(5));
            Assert.That(result.AverageRating, Is.EqualTo(3.0)); // (1 + 2 + 3 + 4 + 5) / 5 = 3.0
        }
        
        // Test for checking if a user can review a driver when the user is the same as the driver
        [Test]
        public async Task CanUserReviewDriverAsync_ReturnsTrueForSameUserDifferentDriver()
        {
            // Arrange
            var reviewerId = "reviewer1";
            var differentDriverId = "driver2";

            _context.Users.Add(new ApplicationUser
            {
                Id = differentDriverId,
                UserName = "driver-different@example.com",
                ContactName = "Driver Different",
                ContactPhone = "1234567890"
            });
            await _context.SaveChangesAsync();
            
            // Act
            var result = await _reviewService.CanUserReviewDriverAsync(differentDriverId, reviewerId);
            
            // Assert
            Assert.That(result, Is.True, "User should be able to review a different driver");
            var resultForExistingReview = await _reviewService.CanUserReviewDriverAsync("driver1", reviewerId);
            Assert.That(resultForExistingReview, Is.False, "User should not be able to review the same driver again");
        }
        
        //
        [Test]
        public async Task AddReviewAsync_ValidatesAndStoresDataCorrectly()
        {
            // Arrange
            var driverId = "driver-validation";
            var reviewerId = "reviewer-validation";
            var comment = "This is a validation test review !@#$%^&*()";
            var rating = 4;

            _context.Users.Add(new ApplicationUser
            {
                Id = driverId,
                UserName = "driver-validation@example.com",
                ContactName = "Driver Validation",
                ContactPhone = "1234567890"
            });

            _context.Users.Add(new ApplicationUser
            {
                Id = reviewerId,
                UserName = "reviewer-validation@example.com",
                ContactName = "Reviewer Validation",
                ContactPhone = "0987654321"
            });
            await _context.SaveChangesAsync();

            var beforeCreation = DateTime.Now.AddSeconds(-1);
            
            // Act
            var result = await _reviewService.AddReviewAsync(driverId, reviewerId, rating, comment);
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.GreaterThan(0), "Review ID should be greater than 0");
            Assert.That(result.DriverId, Is.EqualTo(driverId));
            Assert.That(result.ReviewerId, Is.EqualTo(reviewerId));
            Assert.That(result.Rating, Is.EqualTo(rating));
            Assert.That(result.Comment, Is.EqualTo(comment));
            Assert.That(result.CreatedAt, Is.GreaterThan(beforeCreation), "CreatedAt should be greater than the time before creation");
            Assert.That(result.CreatedAt, Is.LessThan(DateTime.Now), "CreatedAt should be less than the current time");
            
            var savedReview = await _context.DriverReviews
                .FirstOrDefaultAsync(r => r.DriverId == driverId && r.ReviewerId == reviewerId);
            
            Assert.That(savedReview, Is.Not.Null);
            Assert.That(savedReview.Comment, Is.EqualTo(comment), "Comment should match");
            
            var ratingInfo = await _reviewService.GetDriverRatingInfoAsync(driverId);
            Assert.That(ratingInfo.ReviewsCount, Is.EqualTo(1), "Reviews count should be 1");
            Assert.That(ratingInfo.AverageRating, Is.EqualTo(rating), "Average rating should match the review rating");
        }
    }
}