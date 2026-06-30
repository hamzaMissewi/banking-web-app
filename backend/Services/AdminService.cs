using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.DTOs;
using backend.Models;

namespace backend.Services;

public class AdminService
{
    private readonly AppDbContext _context;

    public AdminService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserResponse>> GetUsers()
    {
        var users = await _context.Users
            .Include(u => u.Accounts)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

        return users.Select(MapUser).ToList();
    }

    public async Task<UserResponse?> GetUser(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Accounts)
            .FirstOrDefaultAsync(u => u.Id == userId);

        return user == null ? null : MapUser(user);
    }

    public async Task<UserResponse> ToggleUserStatus(int userId, ToggleUserStatusRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Accounts)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new InvalidOperationException("User not found");

        user.IsActive = request.IsActive;
        await _context.SaveChangesAsync();

        return MapUser(user);
    }

    public async Task<List<AccountResponse>> GetAccounts()
    {
        var accounts = await _context.Accounts
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return accounts.Select(MapAccount).ToList();
    }

    public async Task<AccountResponse> UpdateAccountStatus(int accountId, UpdateAccountStatusRequest request)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId)
            ?? throw new InvalidOperationException("Account not found");

        account.IsActive = request.IsActive;
        await _context.SaveChangesAsync();

        return MapAccount(account);
    }

    public async Task<DashboardResponse> GetDashboard()
    {
        var totalUsers = await _context.Users.CountAsync();
        var totalAccounts = await _context.Accounts.CountAsync();
        var totalTransactions = await _context.Transactions.CountAsync();
        var totalDeposits = await _context.Transactions
            .Where(t => t.Type == TransactionType.Deposit)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;
        var totalWithdrawals = await _context.Transactions
            .Where(t => t.Type == TransactionType.Withdrawal)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;

        return new DashboardResponse
        {
            TotalUsers = totalUsers,
            TotalAccounts = totalAccounts,
            TotalTransactions = totalTransactions,
            TotalDeposits = totalDeposits,
            TotalWithdrawals = totalWithdrawals
        };
    }

    public async Task<UserResponse> PromoteUser(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Accounts)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new InvalidOperationException("User not found");

        user.Role = "Admin";
        await _context.SaveChangesAsync();

        return MapUser(user);
    }

    public async Task<List<TransactionResponse>> GetTransactions()
    {
        var transactions = await _context.Transactions
            .OrderByDescending(t => t.CreatedAt)
            .Take(100)
            .ToListAsync();

        return transactions.Select(MapTransaction).ToList();
    }

    public async Task<List<TransactionResponse>> GetAccountTransactions(int accountId)
    {
        var account = await _context.Accounts.FindAsync(accountId)
            ?? throw new InvalidOperationException("Account not found");

        var transactions = await _context.Transactions
            .Where(t => t.AccountId == accountId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return transactions.Select(MapTransaction).ToList();
    }

    private static UserResponse MapUser(User u)
    {
        return new UserResponse
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            Role = u.Role,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt,
            AccountCount = u.Accounts.Count
        };
    }

    private static AccountResponse MapAccount(Account a)
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
}
