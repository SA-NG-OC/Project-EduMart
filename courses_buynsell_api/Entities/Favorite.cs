namespace courses_buynsell_api.Entities;

public class Favorite
{
    public int UserId { get; set; }
    public User? User { get; set; }

    public int CourseId { get; set; }
    public Course? Course { get; set; }
}
