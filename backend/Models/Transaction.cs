using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public enum TransactionType
{
    Deposit,
    Withdrawal,
    Transfer
}

public class Transaction
{
    public int Id { get; set; }

    public TransactionType Type { get; set; }

    public decimal Amount { get; set; }

    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }

    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;

    public int? TargetAccountId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
