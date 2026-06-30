using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using backend.Services;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly AccountService _accountService;

    public AccountsController(AccountService accountService)
    {
        _accountService = accountService;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAccounts()
    {
        var accounts = await _accountService.GetAccounts(GetUserId());
        return Ok(accounts);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAccount(CreateAccountRequest request)
    {
        var account = await _accountService.CreateAccount(GetUserId(), request);
        return CreatedAtAction(nameof(GetAccounts), new { id = account.Id }, account);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAccount(int id)
    {
        var account = await _accountService.GetAccount(id, GetUserId());
        if (account == null) return NotFound();
        return Ok(account);
    }

    [HttpPost("{id}/deposit")]
    public async Task<IActionResult> Deposit(int id, DepositRequest request)
    {
        try
        {
            var transaction = await _accountService.Deposit(id, GetUserId(), request);
            return Ok(transaction);
        }
        catch (InvalidOperationException e)
        {
            return BadRequest(new { message = e.Message });
        }
    }

    [HttpPost("{id}/withdraw")]
    public async Task<IActionResult> Withdraw(int id, WithdrawRequest request)
    {
        try
        {
            var transaction = await _accountService.Withdraw(id, GetUserId(), request);
            return Ok(transaction);
        }
        catch (InvalidOperationException e)
        {
            return BadRequest(new { message = e.Message });
        }
    }

    [HttpPost("{id}/transfer")]
    public async Task<IActionResult> Transfer(int id, TransferRequest request)
    {
        try
        {
            var transaction = await _accountService.Transfer(id, GetUserId(), request);
            return Ok(transaction);
        }
        catch (InvalidOperationException e)
        {
            return BadRequest(new { message = e.Message });
        }
    }

    [HttpGet("{id}/transactions")]
    public async Task<IActionResult> GetTransactions(int id)
    {
        try
        {
            var transactions = await _accountService.GetTransactions(id, GetUserId());
            return Ok(transactions);
        }
        catch (InvalidOperationException e)
        {
            return BadRequest(new { message = e.Message });
        }
    }
}
