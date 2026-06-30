using backend.DTOs;
using backend.Models;
using backend.Services;

namespace backend.Tests;

public class AdminServiceTests
{
    [Fact]
    public async Task GetUsers_ReturnsAllUsers()
    {
        using var context = TestHelper.CreateDbContext();
        await TestHelper.CreateTestUser(context, "user1");
        await TestHelper.CreateTestUser(context, "user2");
        await TestHelper.CreateTestUser(context, "user3");

        var service = new AdminService(context);
        var users = await service.GetUsers();

        Assert.Equal(3, users.Count);
    }

    [Fact]
    public async Task GetUser_ReturnsUserWithAccountCount()
    {
        using var context = TestHelper.CreateDbContext();
        var user = await TestHelper.CreateTestUser(context, "testuser");
        await TestHelper.CreateTestAccount(context, user.Id);
        await TestHelper.CreateTestAccount(context, user.Id, AccountType.Savings);

        var service = new AdminService(context);
        var result = await service.GetUser(user.Id);

        Assert.NotNull(result);
        Assert.Equal("testuser", result!.Username);
        Assert.Equal("User", result.Role);
        Assert.Equal(2, result.AccountCount);
    }

    [Fact]
    public async Task GetUser_NotFound_ReturnsNull()
    {
        using var context = TestHelper.CreateDbContext();
        var service = new AdminService(context);
        var result = await service.GetUser(999);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAccounts_ReturnsAllAccounts()
    {
        using var context = TestHelper.CreateDbContext();
        var user1 = await TestHelper.CreateTestUser(context, "user1");
        var user2 = await TestHelper.CreateTestUser(context, "user2");
        await TestHelper.CreateTestAccount(context, user1.Id);
        await TestHelper.CreateTestAccount(context, user1.Id, AccountType.Savings);
        await TestHelper.CreateTestAccount(context, user2.Id);

        var service = new AdminService(context);
        var accounts = await service.GetAccounts();

        Assert.Equal(3, accounts.Count);
    }

    [Fact]
    public async Task UpdateAccountStatus_DeactivatesAccount()
    {
        using var context = TestHelper.CreateDbContext();
        var user = await TestHelper.CreateTestUser(context);
        var account = await TestHelper.CreateTestAccount(context, user.Id);

        var service = new AdminService(context);
        var result = await service.UpdateAccountStatus(account.Id, new UpdateAccountStatusRequest
        {
            IsActive = false
        });

        Assert.False(result.IsActive);

        var reloaded = await context.Accounts.FindAsync(account.Id);
        Assert.False(reloaded!.IsActive);
    }

    [Fact]
    public async Task UpdateAccountStatus_NotFound_Throws()
    {
        using var context = TestHelper.CreateDbContext();
        var service = new AdminService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateAccountStatus(999, new UpdateAccountStatusRequest { IsActive = false }));
    }

    [Fact]
    public async Task PromoteUser_MakesUserAdmin()
    {
        using var context = TestHelper.CreateDbContext();
        var user = await TestHelper.CreateTestUser(context, "regularuser");

        var service = new AdminService(context);
        var result = await service.PromoteUser(user.Id);

        Assert.Equal("Admin", result.Role);

        var reloaded = await context.Users.FindAsync(user.Id);
        Assert.Equal("Admin", reloaded!.Role);
    }

    [Fact]
    public async Task PromoteUser_NotFound_Throws()
    {
        using var context = TestHelper.CreateDbContext();
        var service = new AdminService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.PromoteUser(999));
    }

    [Fact]
    public async Task GetDashboard_ReturnsCorrectCounts()
    {
        using var context = TestHelper.CreateDbContext();
        var user = await TestHelper.CreateTestUser(context);
        var account = await TestHelper.CreateTestAccount(context, user.Id);

        var accountService = new AccountService(context);
        await accountService.Deposit(account.Id, user.Id, new DepositRequest { Amount = 1000 });
        await accountService.Withdraw(account.Id, user.Id, new WithdrawRequest { Amount = 200 });

        var adminService = new AdminService(context);
        var dashboard = await adminService.GetDashboard();

        Assert.Equal(1, dashboard.TotalUsers);
        Assert.Equal(1, dashboard.TotalAccounts);
        Assert.Equal(2, dashboard.TotalTransactions);
        Assert.Equal(1000, dashboard.TotalDeposits);
        Assert.Equal(200, dashboard.TotalWithdrawals);
    }

    [Fact]
    public async Task GetTransactions_ReturnsAllTransactions()
    {
        using var context = TestHelper.CreateDbContext();
        var user = await TestHelper.CreateTestUser(context);
        var account = await TestHelper.CreateTestAccount(context, user.Id);

        var accountService = new AccountService(context);
        await accountService.Deposit(account.Id, user.Id, new DepositRequest { Amount = 100 });

        var adminService = new AdminService(context);
        var transactions = await adminService.GetTransactions();

        Assert.Single(transactions);
        Assert.Equal("Deposit", transactions[0].Type);
    }
}
