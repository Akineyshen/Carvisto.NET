using Carvisto.Models;

namespace Carvisto.Services
{
    public interface IReviewService
    {
        Task<IEnumerable<DriverReview>> GetDriverReviewsAsync(string driverId);
        Task<bool> CanUserReviewDriverAsync(string driverId, string userId);
        Task<DriverReview> AddReviewAsync(string driverId, string reviewerId, int rating, string comment);
        Task<(double AverageRating, int ReviewsCount)> GetDriverRatingInfoAsync(string driverId);
        Task<bool> DeleteReviewAsync(int reviewId, string userId, bool isModerator);
    }
}