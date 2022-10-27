using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Core.Accounts.Requests;
using Server.Database;
using Server.Web;

namespace Server.Core.Accounts;

[ApiController]
[AllowAnonymous]
[Route("api/{controller}")]
[Produces("application/json")]
public class AccountController : ControllerBase
{
    private readonly AccountManager _accountManager;

    public AccountController(GameDbContext dbContext, IConfiguration configuration)
    {
        _accountManager = new AccountManager(dbContext, configuration);
    }

    [HttpPost, Route("create")]
    public async Task<IActionResult> Create(CreateAccountRequest request)
    {
        if (!await _accountManager.Create(request.Email, request.Password))
            return BadRequest(new ApiResponse(false, "An account with that email address already exists.", null));

        return Ok(new ApiResponse(true, "Your account has been created.  Please check your email inbox to continue.",
            null));
    }

    [HttpPost, Route("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var loginResult = await _accountManager.Login(request.Email, request.Password);
        if (loginResult is null)
        {
            return BadRequest(new ApiResponse(false, "There was a problem with your login credentials.", null));
        }

        var (authToken, isEmailVerified) = loginResult;

        return Ok(new ApiResponse(true, "Logged in.", new
        {
            Token = authToken,
            EmailVerified = isEmailVerified
        }));
    }

    [HttpGet, Route("verify-email/{code}")]
    public async Task<IActionResult> VerifyEmail(string code)
    {
        var (verified, message) = await _accountManager.VerifyEmail(code);

        return Ok(new ApiResponse(verified, message, null));
    }

    [HttpPost, Route("change-email")]
    public async Task<IActionResult> ChangeEmail(ChangeEmailRequest request)
    {
        var playerId = this.GetPlayerId(HttpContext);
        var verified = await _accountManager.ChangeEmail(playerId, request.NewEmail);

        return Ok(new ApiResponse(verified, "Email changed.", null));
    }

    [HttpPost, Route("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var playerId = this.GetPlayerId(HttpContext);
        var (success, message) = await _accountManager.ChangePassword(playerId, request.CurrentPassword,
            request.NewPassword, request.ConfirmNewPassword);

        return Ok(new ApiResponse(success, message, null));
    }

    [HttpPost, Route("account-recovery")]
    public async Task<IActionResult> ResetCredentials(AccountRecoveryRequest request)
    {
        var verified = await _accountManager.ResetCredentials(request.Email, request.Password, request.Code);

        return Ok(new ApiResponse(verified, "Email changed.", null));
    }

    [HttpGet, Route("check-username/{username}")]
    public async Task<IActionResult> CheckIsUsernameTaken(string username)
    {
        var isTaken = await _accountManager.IsUsernameTaken(username);

        var response = new ApiResponse()
        {
            Success = isTaken,
            Message = isTaken ? "That username is already taken." : "",
            Data = null
        };


        return Ok(response);
    }
}