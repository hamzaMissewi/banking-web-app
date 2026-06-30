using backend.Data;
using backend.DTOs;
using backend.Models;
using backend.Services;

namespace backend.Tests;

public class AccountServiceTests
{
    [Fact]
    public async Task GetAccounts_ReturnsUserAccounts()
    {
        using var context = TestHelper.CreateDbContext();
        var user = await TestHelper.CreateTestUser(context);
        await TestHelper.CreateTestAccount(context, user.Id);
        await TestHelper.CreateTestAccount(context, user.Id, AccountType.Savings);

        var service = new AccountService(context);
        var accounts = await service.GetAccounts(user.Id);

        Assert.Equal(2, accounts.Count);
        Assert.All(accounts, a => Assert.Equal(user.Id, a.UserId));
    }

    [Fact]
    public async Task GetAccounts_OtherUserAccounts_NotReturned()
    {
        using var context = TestHelper.CreateDbContext();
        var user1 = await TestHelper.CreateTestUser(context, "user1");
        var user2 = await TestHelper.CreateTestUser(context, "user2");
        await TestHelper.CreateTestAccount(context, user1.Id);
        await TestHelper.CreateTestAccount(context, user2.Id);

        var service = new AccountService(context);
        var accounts = await service.GetAccounts(user1.Id);

        Assert.Single(accounts);
    }

    [Fact]
    public async Task CreateAccount_WithCheckingType()
    {
        using var context = TestHelper.CreateDbContext();
        var user = await TestHelper.CreateTestUser(context);

        var service = new AccountService(context);
        var request = new CreateAccountRequest { AccountType = "Checking" };

        var result = await service.CreateAccount(user.Id, request);

        Assert.Equal("Checking", result.AccountType);
        Assert.Equal(0, result.Balance);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task CreateAccount_WithSavingsType()
    {
        using var context = TestHelper.CreateDbContext();
        var user = await TestHelper.CreateTestUser(context);

        var service = new AccountService(context);
        var request = new CreateAccountRequest { AccountType = "Savings" };

        var result = await service.CreateAccount(user.Id, request);

        Assert.Equal("Savings", result.AccountType);
    }

    [Fact]
    public async Task CreateAccount_WithMoneyMarketType()
    {
        using var context = TestHelper.CreateDbContext();
        var user = await TestHelper.CreateTestUser(context);

        var service = new AccountService(context);
        var request = new CreateAccountRequest { AccountType = "MoneyMarket" };

        var result = await service.CreateAccount(user.Id, request);

        Assert.Equal("MoneyMarket", result.AccountType);
    }

    [Fact]
    public async Task CreateAccount_InvalidType_DefaultsToChecking()
    {
        using var context = TestHelper.CreateDbContext();
        var user = await TestHelper.CreateTestUser(context);

        var service = new AccountService(context);
        var request = new CreateAccountRequest { AccountType = "InvalidType" };

        var result = await service.CreateAccount(user.Id, request);

        Assert.Equal("Checking", result.AccountType);
    }

    [Fact]
    public async Task Deposit_IncreasesBalance()
    {
        using var context = TestHelper.CreateDbContext();
        var user = await TestHelper.CreateTestUser(context);
        var account = await TestHelper.CreateTestAccount(context, user.Id);

        var service = new AccountService(context);
        var request = new DepositRequest { Amount = 100, Description = "Test deposit" };

        var result = await service.Deposit(account.Id, user.Id, request);

        Assert.Equal(500, result.BalanceBefore);
        Assert.Equal(600, result.BalanceAfter);
        Assert.Equal(100, result.Amount);
        Assert.Equal("Deposit", result.Type);

        var updated = await context.Accounts.FindAsync(account.Id);
        Assert.Equal(600, updated!.Balance);
    }

    [Fact]
    public async Task Withdraw_DecreasesBalance()
    {
        using var context = TestHelper.CreateDbContext();
        var user = await TestHelper.CreateTestUser(context);
        var account = await TestHelper.CreateTestAccount(context, user.Id);

        var service = new AccountService(context);
        var request = new WithdrawRequest { Amount = 100, Description = "Test withdrawal" };

        var result = await service.Withdraw(account.Id, user.Id, request);

        Assert.Equal(500, result.BalanceBefore);
        Assert.Equal(400, result.BalanceAfter);
        Assert.Equal("Withdrawal", result.Type);
    }

    [Fact]
    public async Task Withdraw_InsufficientFunds_Throws()
    {
        using var context = TestHelper.CreateDbContext();
        var user = await TestHelper.CreateTestUser(context);
        var account = await TestHelper.CreateTestAccount(context, user.Id);

        var service = new AccountService(context);
        var request = new WithdrawRequest { Amount = 1000 };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.Withdraw(account.Id, user.Id, request));
    }

    [Fact]
    public async Task Transfer_MovesMoneyBetweenAccounts()
    {
        using var context = TestHelper.CreateDbContext();
        var user = await TestHelper.CreateTestUser(context);
        var source = await TestHelper.CreateTestAccount(context, user.Id);

        var target = new Account
        {
            AccountNumber = "0987654321",
            AccountType = AccountType.Checking,
            Balance = 100,
            UserId = user.Id
        };
        context.Accounts.Add(target);
        await context.SaveChangesAsync();

        var service = new AccountService(context);
        var request = new TransferRequest
        {
            TargetAccountNumber = "0987654321",
            Amount = 200,
            Description = "Test transfer"
        };

        var result = await service.Transfer(source.Id, user.Id, request);

        Assert.Equal(500, result.BalanceBefore);
        Assert.Equal(300, result.BalanceAfter);
        Assert.Equal("Transfer", result.Type);

        var targetReloaded = await context.Accounts.FindAsync(target.Id);
        Assert.Equal(300, targetReloaded!.Balance);
    }

    [Fact]
    public async Task Transfer_SameAccount_Throws()
    {
        using var context = TestHelper.CreateDbContext();
        var user = await TestHelper.CreateTestUser(context);
        var account = await TestHelper.CreateTestAccount(context, user.Id);

        var service = new AccountService(context);
        var request = new TransferRequest
        {
            TargetAccountNumber = "1234567890",
            Amount = 50
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.Transfer(account.Id, user.Id, request));
    }

    [Fact]
    public async Task Transfer_InsufficientFunds_Throws()
    {
        using var context = TestHelper.CreateDbContext();
        var user = await TestHelper.CreateTestUser(context);
        var source = await TestHelper.CreateTestAccount(context, user.Id);

        var target = new Account
        {
            AccountNumber = "0987654321",
            AccountType = AccountType.Checking,
            Balance = 100,
            UserId = user.Id
        };
        context.Accounts.Add(target);
        await context.SaveChangesAsync();

        var service = new AccountService(context);
        var request = new TransferRequest
        {
            TargetAccountNumber = "0987654321",
            Amount = 5000
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.Transfer(source.Id, user.Id, request));
    }

    [Fact]
    public async Task GetTransactions_ReturnsOrderedList()
    {
        using var context = TestHelper.CreateDbContext();
        var user = await TestHelper.CreateTestUser(context);
        var account = await TestHelper.CreateTestAccount(context, user.Id);

        var service = new AccountService(context);
        await service.Deposit(account.Id, user.Id, new DepositRequest { Amount = 100 });
        await service.Withdraw(account.Id, user.Id, new WithdrawRequest { Amount = 50 });

        var transactions = await service.GetTransactions(account.Id, user.Id);

        Assert.Equal(2, transactions.Count);
        Assert.Equal("Withdrawal", transactions[0].Type);
        Assert.Equal("Deposit", transactions[1].Type);
    }

    [Fact]
    public async Task Deposit_OtherUserAccount_Throws()
    {
        using var context = TestHelper.CreateDbContext();
        var user1 = await TestHelper.CreateTestUser(context, "user1");
        var user2 = await TestHelper.CreateTestUser(context, "user2");
        var account = await TestHelper.CreateTestAccount(context, user1.Id);

        var service = new AccountService(context);
        var request = new DepositRequest { Amount = 100 };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.Deposit(account.Id, user2.Id, request));
    }
}
