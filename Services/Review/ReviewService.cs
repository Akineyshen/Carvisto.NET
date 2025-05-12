using Carvisto.Data;
using Carvisto.Models;
using Microsoft.EntityFrameworkCore;

namespace Carvisto.Services
{
    public class ReviewService : IReviewService
    {
        private readonly CarvistoDbContext _context;
        
        public ReviewService(CarvistoDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DriverReview>> GetDriverReviewsAsync(string driverId)
        {
            return await _context.DriverReviews
                .Include(r => r.Reviewer)
                .Where(r => r.DriverId == driverId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> CanUserReviewDriverAsync(string driverId, string userId)
        {
            var hasExistingReview = await _context.DriverReviews
                .AnyAsync(r => r.ReviewerId == userId && r.DriverId == driverId);
            
            return !hasExistingReview;
        }

        public async Task<DriverReview> AddReviewAsync(string driverId, string reviewerId, int rating, string comment)
        {
            var review = new DriverReview
            {
                DriverId = driverId,
                ReviewerId = reviewerId,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.Now
            };
            
            _context.DriverReviews.Add(review);
            await _context.SaveChangesAsync();
            return review;
        }
        
        public async Task<(double AverageRating, int ReviewsCount)> GetDriverRatingInfoAsync(string driverId)
        {
            var reviews = await _context.DriverReviews
                .Where(r => r.DriverId == driverId)
                .ToListAsync();

            var count = reviews.Count;
            var average = count > 0 ? reviews.Average(r => r.Rating) : 0;
            
            return (average, count);
        }

        public async Task<bool> DeleteReviewAsync(int reviewId, string userId, bool isModerator)
        {
            var review = await _context.DriverReviews.FindAsync(reviewId);
            
            if (review == null)
            {
                return false;
            }
            
            if (!isModerator && review.ReviewerId != userId)
            {
                return false;
            }
            
            _context.DriverReviews.Remove(review);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

