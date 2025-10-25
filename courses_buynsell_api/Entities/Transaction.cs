using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace courses_buynsell_api.Entities;

public class Transaction
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    [MaxLength(50)]
    public string TransactionCode { get; set; } = String.Empty;
    [Required]
    [MaxLength(50)]
    public string PaymentMethod { get; set; } = String.Empty;
    [Required] 
    public decimal TotalAmount { get; set; }
    [Required] 
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int BuyerId { get; set; }
    public User? Buyer { get; set; }
    public ICollection<TransactionDetail> TransactionDetails { get; set; } = new List<TransactionDetail>();
}