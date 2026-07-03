using Microsoft.EntityFrameworkCore;
using backend.Data;

namespace backend.Helpers;

public static class AccountNumberGenerator
{
    public static async Task<string> GenerateUnique(AppDbContext context)
    {
        var random = Random.Shared;
        var digits = new char[10];
        for (int attempt = 0; attempt < 50; attempt++)
        {
            for (int i = 0; i < 10; i++)
                digits[i] = (char)('0' + random.Next(0, 10));
            var number = new string(digits);
            if (!await context.Accounts.AnyAsync(a => a.AccountNumber == number))
                return number;
        }
        throw new InvalidOperationException("Failed to generate a unique account number after 50 attempts");
    }
}