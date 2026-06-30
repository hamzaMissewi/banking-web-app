using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public class Account
{
    public int Id { get; set; }

    [Required, MaxLength(20)]
    public string AccountNumber { get; set; } = string.Empty;

    [MaxLength(20)]
    public string AccountType { get; set; } = "Checking";

    public decimal Balance { get; set; } = 0;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Transaction> Transactions { get; set; } = new();
}
