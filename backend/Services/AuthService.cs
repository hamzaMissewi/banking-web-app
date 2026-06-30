using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using backend.Data;
using backend.DTOs;
using backend.Models;

namespace backend.Services;

public class AuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public async Task<AuthResponse> Register(RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            throw new InvalidOperationException("Username already exists");

        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            throw new InvalidOperationException("Email already exists");

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var account = new Account
        {
            AccountNumber = GenerateAccountNumber(),
            AccountType = "Checking",
            Balance = 0,
            UserId = user.Id
        };

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            Token = GenerateJwt(user),
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email
        };
    }

    public async Task<AuthResponse> Login(LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username)
            ?? throw new InvalidOperationException("Invalid credentials");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new InvalidOperationException("Invalid credentials");

        return new AuthResponse
        {
            Token = GenerateJwt(user),
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email
        };
    }

    private string GenerateJwt(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
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
