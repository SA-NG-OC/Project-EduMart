using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace courses_buynsell_api.Entities;

public class TransactionDetail
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required] 
    public decimal Price { get; set; }
    [Required] 
    public int TransactionId { get; set; }
    public Transaction? Transaction { get; set; }
    [Required] 
    public int CourseId { get; set; }
    public Course? Course { get; set; }
}