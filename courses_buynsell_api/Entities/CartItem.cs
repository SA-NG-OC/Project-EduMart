using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace courses_buynsell_api.Entities;

public class CartItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int CartId { get; set; }
    public Cart? Cart { get; set; }
    public int CourseId { get; set; }
    public Course? Course { get; set; }
}