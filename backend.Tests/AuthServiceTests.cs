using Microsoft.Extensions.Configuration;
using backend.Data;
using backend.DTOs;
using backend.Services;

namespace backend.Tests;

public class AuthServiceTests
{
    [Fact]
    public async Task Register_CreatesUserAndAccount()
    {
        using var context = TestHelper.CreateDbContext();
        var config = TestHelper.CreateConfig();
        var service = new AuthService(context, config);

        var request = new RegisterRequest
        {
            Username = "newuser",
            Email = "new@test.com",
            Password = "password123"
        };

        var result = await service.Register(request);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
        Assert.Equal("newuser", result.Username);
        Assert.Equal("new@test.com", result.Email);
        Assert.Equal("User", result.Role);

        var user = await context.Users.FindAsync(result.UserId);
        Assert.NotNull(user);
        Assert.Equal("newuser", user.Username);

        var accounts = context.Accounts.Where(a => a.UserId == result.UserId).ToList();
        Assert.Single(accounts);
    }

    [Fact]
    public async Task Register_DuplicateUsername_Throws()
    {
        using var context = TestHelper.CreateDbContext();
        var config = TestHelper.CreateConfig();
        await TestHelper.CreateTestUser(context, "existinguser");

        var service = new AuthService(context, config);
        var request = new RegisterRequest
        {
            Username = "existinguser",
            Email = "other@test.com",
            Password = "password123"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.Register(request));
    }

    [Fact]
    public async Task Register_DuplicateEmail_Throws()
    {
        using var context = TestHelper.CreateDbContext();
        var config = TestHelper.CreateConfig();
        var user = await TestHelper.CreateTestUser(context, "user1");
        user.Email = "dup@test.com";
        await context.SaveChangesAsync();

        var service = new AuthService(context, config);
        var request = new RegisterRequest
        {
            Username = "user2",
            Email = "dup@test.com",
            Password = "password123"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.Register(request));
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        using var context = TestHelper.CreateDbContext();
        var config = TestHelper.CreateConfig();
        await TestHelper.CreateTestUser(context);

        var service = new AuthService(context, config);
        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "password123"
        };

        var result = await service.Login(request);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
        Assert.Equal("testuser", result.Username);
    }

    [Fact]
    public async Task Login_WrongPassword_Throws()
    {
        using var context = TestHelper.CreateDbContext();
        var config = TestHelper.CreateConfig();
        await TestHelper.CreateTestUser(context);

        var service = new AuthService(context, config);
        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "wrongpassword"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.Login(request));
    }

    [Fact]
    public async Task Login_NonExistentUser_Throws()
    {
        using var context = TestHelper.CreateDbContext();
        var config = TestHelper.CreateConfig();
        var service = new AuthService(context, config);
        var request = new LoginRequest
        {
            Username = "nobody",
            Password = "password123"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.Login(request));
    }

    [Fact]
    public async Task Login_DeactivatedUser_Throws()
    {
        using var context = TestHelper.CreateDbContext();
        var config = TestHelper.CreateConfig();
        var user = await TestHelper.CreateTestUser(context);
        user.IsActive = false;
        await context.SaveChangesAsync();

        var service = new AuthService(context, config);
        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "password123"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.Login(request));
    }
}
