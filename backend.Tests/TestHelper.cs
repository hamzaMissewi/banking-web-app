using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using backend.Data;
using backend.Models;

namespace backend.Tests;

public static class TestHelper
{
    public static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"BankingTestDb_{Guid.NewGuid()}")
            .Options;
        return new AppDbContext(options);
    }

    public static IConfiguration CreateConfig()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "TestSuperSecretKeyThatIsLongEnoughForHmacSha256!",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience"
            })
            .Build();
    }

    public static async Task<User> CreateTestUser(AppDbContext context, string username = "testuser", string role = "User")
    {
        var user = new User
        {
            Username = username,
            Email = $"{username}@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            Role = role
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    public static async Task<Account> CreateTestAccount(AppDbContext context, int userId, AccountType type = AccountType.Checking)
    {
        var account = new Account
        {
            AccountNumber = "1234567890",
            AccountType = type,
            Balance = 500,
            UserId = userId
        };
        context.Accounts.Add(account);
        await context.SaveChangesAsync();
        return account;
    }
}
