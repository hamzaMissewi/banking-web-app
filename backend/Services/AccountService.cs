using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.DTOs;
using backend.Models;

namespace backend.Services;

public class AccountService
{
    private readonly AppDbContext _context;

    public AccountService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<AccountResponse>> GetAccounts(int userId)
    {
        return await _context.Accounts
            .Where(a => a.UserId == userId)
            .Select(a => new AccountResponse
            {
                Id = a.Id,
                AccountNumber = a.AccountNumber,
                AccountType = a.AccountType,
                Balance = a.Balance,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<AccountResponse> CreateAccount(int userId, CreateAccountRequest request)
    {
        var account = new Account
        {
            AccountNumber = GenerateAccountNumber(),
            AccountType = request.AccountType,
            Balance = 0,
            UserId = userId
        };

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        return new AccountResponse
        {
            Id = account.Id,
            AccountNumber = account.AccountNumber,
            AccountType = account.AccountType,
            Balance = account.Balance,
            CreatedAt = account.CreatedAt
        };
    }

    public async Task<AccountResponse?> GetAccount(int accountId, int userId)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId);

        if (account == null) return null;

        return new AccountResponse
        {
            Id = account.Id,
            AccountNumber = account.AccountNumber,
            AccountType = account.AccountType,
            Balance = account.Balance,
            CreatedAt = account.CreatedAt
        };
    }

    public async Task<TransactionResponse> Deposit(int accountId, int userId, DepositRequest request)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId)
            ?? throw new InvalidOperationException("Account not found");

        var balanceBefore = account.Balance;
        account.Balance += request.Amount;

        var transaction = new Transaction
        {
            Type = TransactionType.Deposit,
            Amount = request.Amount,
            BalanceBefore = balanceBefore,
            BalanceAfter = account.Balance,
            Description = request.Description,
            AccountId = account.Id
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        return MapTransaction(transaction);
    }

    public async Task<TransactionResponse> Withdraw(int accountId, int userId, WithdrawRequest request)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId)
            ?? throw new InvalidOperationException("Account not found");

        if (account.Balance < request.Amount)
            throw new InvalidOperationException("Insufficient funds");

        var balanceBefore = account.Balance;
        account.Balance -= request.Amount;

        var transaction = new Transaction
        {
            Type = TransactionType.Withdrawal,
            Amount = request.Amount,
            BalanceBefore = balanceBefore,
            BalanceAfter = account.Balance,
            Description = request.Description,
            AccountId = account.Id
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        return MapTransaction(transaction);
    }

    public async Task<TransactionResponse> Transfer(int accountId, int userId, TransferRequest request)
    {
        var sourceAccount = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId)
            ?? throw new InvalidOperationException("Source account not found");

        if (sourceAccount.AccountNumber == request.TargetAccountNumber)
            throw new InvalidOperationException("Cannot transfer to the same account");

        var targetAccount = await _context.Accounts
            .FirstOrDefaultAsync(a => a.AccountNumber == request.TargetAccountNumber)
            ?? throw new InvalidOperationException("Target account not found");

        if (sourceAccount.Balance < request.Amount)
            throw new InvalidOperationException("Insufficient funds");

        var sourceBalanceBefore = sourceAccount.Balance;
        sourceAccount.Balance -= request.Amount;
        targetAccount.Balance += request.Amount;

        var transaction = new Transaction
        {
            Type = TransactionType.Transfer,
            Amount = request.Amount,
            BalanceBefore = sourceBalanceBefore,
            BalanceAfter = sourceAccount.Balance,
            Description = request.Description,
            AccountId = sourceAccount.Id,
            TargetAccountId = targetAccount.Id
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        return MapTransaction(transaction);
    }

    public async Task<List<TransactionResponse>> GetTransactions(int accountId, int userId)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId)
            ?? throw new InvalidOperationException("Account not found");

        return await _context.Transactions
            .Where(t => t.AccountId == accountId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TransactionResponse
            {
                Id = t.Id,
                Type = t.Type.ToString(),
                Amount = t.Amount,
                BalanceBefore = t.BalanceBefore,
                BalanceAfter = t.BalanceAfter,
                Description = t.Description,
                TargetAccountId = t.TargetAccountId,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();
    }

    private static TransactionResponse MapTransaction(Transaction t)
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

    private static string GenerateAccountNumber()
    {
        var random = Random.Shared;
        var digits = new char[10];
        for (int i = 0; i < 10; i++)
            digits[i] = (char)('0' + random.Next(0, 10));
        return new string(digits);
    }
}
