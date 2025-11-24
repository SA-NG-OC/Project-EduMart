using courses_buynsell_api.DTOs.Review;
using courses_buynsell_api.Entities;

namespace courses_buynsell_api.Interfaces;

public interface IReviewService
{
    Task<IEnumerable<ReviewResponseDto>> GetByCourseId(int courseId);
    Task<IEnumerable<ReviewResponseDto>> GetBySellerCourses(int courseId, int sellerId);
    Task DeleteReview(int reviewId);
    Task DeleteReviewByUser(int userId, int reviewId);
    Task CreateReview(ReviewRequestDto reviewDto, int buyerId);
    Task UpdateReview(int reviewId, int userId, ReviewUpdateRequest reviewUpdateDto);
}