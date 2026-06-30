using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public class Account
{
    public int Id { get; set; }

    [Required, MaxLength(20)]
    public string AccountNumber { get; set; } = string.Empty;

    public AccountType AccountType { get; set; } = AccountType.Checking;

    public decimal Balance { get; set; }

    public bool IsActive { get; set; } = true;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Transaction> Transactions { get; set; } = new();
}
