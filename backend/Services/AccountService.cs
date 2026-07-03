using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.DTOs;
using backend.Helpers;
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
        var accounts = await _context.Accounts
            .Where(a => a.UserId == userId)
            .ToListAsync();

        return accounts.Select(MappingHelper.ToResponse).ToList();
    }

    public async Task<AccountResponse> CreateAccount(int userId, CreateAccountRequest request)
    {
        var account = new Account
        {
            AccountNumber = await AccountNumberGenerator.GenerateUnique(_context),
            AccountType = MappingHelper.ParseAccountType(request.AccountType),
            UserId = userId
        };

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        return MappingHelper.ToResponse(account);
    }

    public async Task<AccountResponse?> GetAccount(int accountId, int userId)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId);

        return account == null ? null : MappingHelper.ToResponse(account);
    }

    public async Task<AccountResponse> UpdateAccount(int accountId, int userId, UpdateAccountRequest request)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId)
            ?? throw new InvalidOperationException("Account not found");

        account.AccountType = MappingHelper.ParseAccountType(request.AccountType);

        await _context.SaveChangesAsync();
        return MappingHelper.ToResponse(account);
    }

    public async Task<AccountResponse> CloseAccount(int accountId, int userId)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId)
            ?? throw new InvalidOperationException("Account not found");

        if (!account.IsActive)
            throw new InvalidOperationException("Account is already closed");

        if (account.Balance != 0)
            throw new InvalidOperationException("Cannot close account with non-zero balance");

        account.IsActive = false;
        await _context.SaveChangesAsync();

        return MappingHelper.ToResponse(account);
    }

    public async Task<TransactionResponse> Deposit(int accountId, int userId, DepositRequest request)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId)
            ?? throw new InvalidOperationException("Account not found");

        if (!account.IsActive)
            throw new InvalidOperationException("Cannot deposit into a closed account");

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

        return MappingHelper.ToResponse(transaction);
    }

    public async Task<TransactionResponse> Withdraw(int accountId, int userId, WithdrawRequest request)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId)
            ?? throw new InvalidOperationException("Account not found");

        if (!account.IsActive)
            throw new InvalidOperationException("Cannot withdraw from a closed account");

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

        return MappingHelper.ToResponse(transaction);
    }

    public async Task<TransactionResponse> Transfer(int accountId, int userId, TransferRequest request)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        var sourceAccount = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId)
            ?? throw new InvalidOperationException("Source account not found");

        if (!sourceAccount.IsActive)
            throw new InvalidOperationException("Cannot transfer from a closed account");

        if (sourceAccount.AccountNumber == request.TargetAccountNumber)
            throw new InvalidOperationException("Cannot transfer to the same account");

        var targetAccount = await _context.Accounts
            .FirstOrDefaultAsync(a => a.AccountNumber == request.TargetAccountNumber)
            ?? throw new InvalidOperationException("Target account not found");

        if (!targetAccount.IsActive)
            throw new InvalidOperationException("Cannot transfer to a closed account");

        if (sourceAccount.Balance < request.Amount)
            throw new InvalidOperationException("Insufficient funds");

        var sourceBalanceBefore = sourceAccount.Balance;
        var targetBalanceBefore = targetAccount.Balance;
        sourceAccount.Balance -= request.Amount;
        targetAccount.Balance += request.Amount;

        var sourceTransaction = new Transaction
        {
            Type = TransactionType.Transfer,
            Amount = request.Amount,
            BalanceBefore = sourceBalanceBefore,
            BalanceAfter = sourceAccount.Balance,
            Description = request.Description,
            AccountId = sourceAccount.Id,
            TargetAccountId = targetAccount.Id
        };

        var targetTransaction = new Transaction
        {
            Type = TransactionType.Transfer,
            Amount = request.Amount,
            BalanceBefore = targetBalanceBefore,
            BalanceAfter = targetAccount.Balance,
            Description = request.Description,
            AccountId = targetAccount.Id,
            TargetAccountId = sourceAccount.Id
        };

        _context.Transactions.AddRange(sourceTransaction, targetTransaction);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return MappingHelper.ToResponse(sourceTransaction);
    }

    public async Task<List<TransactionResponse>> GetTransactions(int accountId, int userId)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId)
            ?? throw new InvalidOperationException("Account not found");

        var transactions = await _context.Transactions
            .Where(t => t.AccountId == accountId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return transactions.Select(MappingHelper.ToResponse).ToList();
    }
}
