namespace backend.DTOs;

public class UserResponse
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int AccountCount { get; set; }
}

public class DashboardResponse
{
    public int TotalUsers { get; set; }
    public int TotalAccounts { get; set; }
    public int TotalTransactions { get; set; }
    public decimal TotalDeposits { get; set; }
    public decimal TotalWithdrawals { get; set; }
}

public class UpdateAccountStatusRequest
{
    public bool IsActive { get; set; }
}
