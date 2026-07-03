using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class CreateAccountRequest
{
    [RegularExpression("^(Checking|Savings|MoneyMarket)$", ErrorMessage = "AccountType must be Checking, Savings, or MoneyMarket")]
    public string AccountType { get; set; } = "Checking";
}

public class AccountResponse
{
    public int Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public bool IsActive { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DepositRequest
{
    [Required, Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [MaxLength(200)]
    public string Description { get; set; } = "Deposit";
}

public class WithdrawRequest
{
    [Required, Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [MaxLength(200)]
    public string Description { get; set; } = "Withdrawal";
}

public class TransferRequest
{
    [Required, MaxLength(20)]
    public string TargetAccountNumber { get; set; } = string.Empty;

    [Required, Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [MaxLength(200)]
    public string Description { get; set; } = "Transfer";
}

public class UpdateAccountRequest
{
    [RegularExpression("^(Checking|Savings|MoneyMarket)$", ErrorMessage = "AccountType must be Checking, Savings, or MoneyMarket")]
    public string AccountType { get; set; } = "Checking";
}

public class TransactionResponse
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    public string Description { get; set; } = string.Empty;
    public int? TargetAccountId { get; set; }
    public DateTime CreatedAt { get; set; }
}
