using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using backend.Services;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AdminService _adminService;

    public AdminController(AdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var dashboard = await _adminService.GetDashboard();
        return Ok(dashboard);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _adminService.GetUsers();
        return Ok(users);
    }

    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _adminService.GetUser(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPost("users/{id}/promote")]
    public async Task<IActionResult> PromoteUser(int id)
    {
        try
        {
            var user = await _adminService.PromoteUser(id);
            return Ok(user);
        }
        catch (InvalidOperationException e)
        {
            return NotFound(new { message = e.Message });
        }
    }

    [HttpGet("accounts")]
    public async Task<IActionResult> GetAccounts()
    {
        var accounts = await _adminService.GetAccounts();
        return Ok(accounts);
    }

    [HttpPut("accounts/{id}/status")]
    public async Task<IActionResult> UpdateAccountStatus(int id, UpdateAccountStatusRequest request)
    {
        try
        {
            var account = await _adminService.UpdateAccountStatus(id, request);
            return Ok(account);
        }
        catch (InvalidOperationException e)
        {
            return NotFound(new { message = e.Message });
        }
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions()
    {
        var transactions = await _adminService.GetTransactions();
        return Ok(transactions);
    }
}
