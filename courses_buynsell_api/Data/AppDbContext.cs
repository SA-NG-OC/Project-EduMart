using Microsoft.EntityFrameworkCore;
using courses_buynsell_api.Entities;

namespace courses_buynsell_api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // DbSet cho tất cả các entity bạn có
    public DbSet<User> Users { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<CourseContent> CourseContents { get; set; }
    public DbSet<CourseSkill> CourseSkills { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<Favorite> Favorites { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<TargetLearner> TargetLearners { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<TransactionDetail> TransactionDetails { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ✅ Composite key cho Favorite
        modelBuilder.Entity<Favorite>()
            .HasKey(f => new { f.UserId, f.CourseId });
    }
}
