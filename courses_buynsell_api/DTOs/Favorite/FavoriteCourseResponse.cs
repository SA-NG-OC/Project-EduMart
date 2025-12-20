namespace courses_buynsell_api.DTOs.Favorite
{
    public class FavoriteCourseResponse
    {
        public int UserId { get; set; }

        // Course info
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public decimal AverageRating { get; set; }
        public int TotalPurchased { get; set; }
        public int DurationHours { get; set; }
        public decimal Price { get; set; }
        public string Level { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }
}
