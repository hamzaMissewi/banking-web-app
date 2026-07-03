using backend.DTOs;
using backend.Models;

namespace backend.Helpers;

public static class MappingHelper
{
    public static AccountResponse ToResponse(Account a)
    {
        return new AccountResponse
        {
            Id = a.Id,
            AccountNumber = a.AccountNumber,
            AccountType = a.AccountType.ToString(),
            Balance = a.Balance,
            IsActive = a.IsActive,
            UserId = a.UserId,
            CreatedAt = a.CreatedAt
        };
    }

    public static TransactionResponse ToResponse(Transaction t)
    {
        return new TransactionResponse
        {
            Id = t.Id,
            Type = t.Type.ToString(),
            Amount = t.Amount,
            BalanceBefore = t.BalanceBefore,
            BalanceAfter = t.BalanceAfter,
            Description = t.Description,
            TargetAccountId = t.TargetAccountId,
            CreatedAt = t.CreatedAt
        };
    }

    public static UserResponse ToResponse(User u)
    {
        return new UserResponse
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            Role = u.Role,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt,
            AccountCount = u.Accounts?.Count ?? 0
        };
    }

    public static AccountType ParseAccountType(string? value)
    {
        if (Enum.TryParse<AccountType>(value, true, out var parsed)
            && Enum.IsDefined(parsed))
            return parsed;
        return AccountType.Checking;
    }
}