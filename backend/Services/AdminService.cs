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

        return users.Select(u => new UserResponse
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            Role = u.Role,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt,
            AccountCount = u.Accounts.Count
        }).ToList();
    }

    public async Task<UserResponse?> GetUser(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Accounts)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return null;

        return new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            AccountCount = user.Accounts.Count
        };
    }

    public async Task<List<AccountResponse>> GetAccounts()
    {
        var accounts = await _context.Accounts
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return accounts.Select(a => new AccountResponse
        {
            Id = a.Id,
            AccountNumber = a.AccountNumber,
            AccountType = a.AccountType.ToString(),
            Balance = a.Balance,
            IsActive = a.IsActive,
            UserId = a.UserId,
            CreatedAt = a.CreatedAt
        }).ToList();
    }

    public async Task<AccountResponse> UpdateAccountStatus(int accountId, UpdateAccountStatusRequest request)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId)
            ?? throw new InvalidOperationException("Account not found");

        account.IsActive = request.IsActive;
        await _context.SaveChangesAsync();

        return new AccountResponse
        {
            Id = account.Id,
            AccountNumber = account.AccountNumber,
            AccountType = account.AccountType.ToString(),
            Balance = account.Balance,
            IsActive = account.IsActive,
            UserId = account.UserId,
            CreatedAt = account.CreatedAt
        };
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

        return new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            AccountCount = user.Accounts.Count
        };
    }

    public async Task<List<TransactionResponse>> GetTransactions()
    {
        var transactions = await _context.Transactions
            .OrderByDescending(t => t.CreatedAt)
            .Take(100)
            .ToListAsync();

        return transactions.Select(t => new TransactionResponse
        {
            Id = t.Id,
            Type = t.Type.ToString(),
            Amount = t.Amount,
            BalanceBefore = t.BalanceBefore,
            BalanceAfter = t.BalanceAfter,
            Description = t.Description,
            TargetAccountId = t.TargetAccountId,
            CreatedAt = t.CreatedAt
        }).ToList();
    }
}
