namespace courses_buynsell_api.Services;

using courses_buynsell_api.Entities;
using courses_buynsell_api.Interfaces;
using courses_buynsell_api.DTOs.Review;
using courses_buynsell_api.Data;
using Microsoft.EntityFrameworkCore;
using courses_buynsell_api.Exceptions;

public class ReviewService : IReviewService
{
    private readonly AppDbContext _context;
    public ReviewService(AppDbContext context)
    {
        _context = context;
    }

    /*
    Phương thức GET
    */
    public async Task<IEnumerable<ReviewResponseDto>> GetByCourseId(int courseId)
    {
        try
        {
            var review = await _context.Reviews
                .Include(r => r.Buyer)
                .Where(r => r.CourseId == courseId)
                .Select(r => new ReviewResponseDto
                {
                    Id = r.Id,
                    UserId = r.BuyerId, // NEW
                    UserName = r.Buyer!.FullName ?? "Unknown user",
                    Image = r.Buyer!.AvatarUrl, // NEW
                    Rating = r.Star,
                    Comment = r.Comment ?? string.Empty,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return review;
        }
        catch
        {
            throw;
        }
    }


    public async Task<IEnumerable<ReviewResponseDto>> GetBySellerCourses(int courseId, int sellerId)
    {
        var course = await _context.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null)
            throw new NotFoundException("Course not found.");

        if (course.SellerId != sellerId)
            throw new ForbiddenException("You are not authorized to view reviews for this course.");

        var reviews = await _context.Reviews
            .Include(r => r.Buyer)
            .Where(r => r.CourseId == courseId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewResponseDto
            {
                Id = r.Id,
                UserId = r.BuyerId, // NEW
                UserName = r.Buyer != null ? r.Buyer.FullName : "Unknown",
                Image = r.Buyer != null ? r.Buyer.AvatarUrl : null, // NEW
                Rating = r.Star,
                Comment = r.Comment ?? string.Empty,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return reviews;
    }


    /*
    Phương thức DELETE
    */
    public async Task DeleteReview(int reviewId)
    {
        var review = await _context.Reviews.FindAsync(reviewId);
        if (review == null)
        {
            throw new NotFoundException("Review not found.");
        }

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteReviewByUser(int userId, int reviewId)
    {
        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.Id == reviewId);

        if (review == null)
            throw new NotFoundException("Review not found.");

        if (review.BuyerId != userId)
            throw new ForbiddenException("You are not authorized to delete this review.");

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
    }

    /*
    Phương thức POST
    */
    public async Task CreateReview(ReviewRequestDto reviewDto, int buyerId)
    {
        var review = new Review
        {
            BuyerId = buyerId,
            CourseId = reviewDto.CourseId,
            Star = reviewDto.Rating,
            Comment = reviewDto.Comment,
            CreatedAt = DateTime.UtcNow
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();
    }

    /*
    Phương thức PUT
    */
    public async Task UpdateReview(int reviewId, int userId, ReviewUpdateRequest reviewUpdateDto)
    {
        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.Id == reviewId);

        if (review == null)
            throw new NotFoundException("Review not found.");

        if (review.BuyerId != userId)
            throw new ForbiddenException("You are not authorized to update this review.");

        // Kiểm tra nếu cả hai trường đều không có
        if (reviewUpdateDto.Rating == null && string.IsNullOrWhiteSpace(reviewUpdateDto.Comment))
            throw new BadRequestException("At least one field (Rating or Comment) must be provided.");

        // Cập nhật các trường có giá trị
        if (reviewUpdateDto.Rating.HasValue)
            review.Star = reviewUpdateDto.Rating.Value;

        if (!string.IsNullOrWhiteSpace(reviewUpdateDto.Comment))
            review.Comment = reviewUpdateDto.Comment;

        _context.Reviews.Update(review);
        await _context.SaveChangesAsync();
    }

}
