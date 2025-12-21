namespace courses_buynsell_api.DTOs.Cart
{
    public class CartItemDto
    {
        public int Id { get; set; } // ID của CartItem
        public int CourseId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string? Level { get; set; }
        public string? ImageUrl { get; set; }
        public decimal? AverageRating { get; set; }
        public int? TotalPurchased { get; set; }
        public string? TeacherName { get; set; }
        public int? SellerId { get; set; }
        public int? DurationHours { get; set; }
        public string? CategoryName { get; set; }
        public bool? IsApproved { get; set; }
        public bool? IsRestricted { get; set; }
    }
}