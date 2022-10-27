using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Server.Core._misc;
using Server.Core.Accounts.Entities;
using Server.Database;

namespace Server.Core.Accounts;

public class AccountManager
{
    private readonly GameDbContext _db;
    private readonly IConfiguration _configuration = null!;

    public AccountManager(GameDbContext db)
    {
        _db = db;
    }

    public AccountManager(GameDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    public async Task<Account?> GetByPlayerId(Guid playerId)
    {
        return await _db.Accounts.FirstOrDefaultAsync(a => a.PlayerId == playerId);
    }
    
    public async Task<Tuple<string, bool>?> Login(string email, string password)
    {
        var account = await _db.Accounts.FirstOrDefaultAsync(account => account.Email == email);
        if (account is null)
            return null;

        var hasher = new PasswordHasher<Account>();
        var result = hasher.VerifyHashedPassword(account, account.HashedPassword, password);

        if (result == PasswordVerificationResult.Failed)
            return null;

        var appSettings = _configuration.GetSection("AppSettings").GetValue<string>("Secret");
        // authentication successful so generate jwt token
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(appSettings);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, account.PlayerId.ToString())
        };
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var returnToken = tokenHandler.WriteToken(token);

        return new Tuple<string, bool>(returnToken, account.EmailVerified);
    }

    public async Task<bool> Create(string email, string password)
    {
        var exists = await _db.Accounts.FirstOrDefaultAsync(a => a.Email == email);

        if (exists is not null)
            return false;

        var account = new Account()
        {
            Email = email
        };

        var hasher = new PasswordHasher<Account>();
        account.HashedPassword = hasher.HashPassword(account, password);
        //account.EmailVerified = true;

        var code = await SendEmailVerification(email);

        var dbCode = new VerificationCode
        {
            Code = code,
            Account = account,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            Type = VerificationCodeType.VerifyEmail
        };
        await _db.VerificationCodes.AddAsync(dbCode);

        await _db.Accounts.AddAsync(account);
        return await _db.SaveChangesAsync() == 2;
    }

    public async Task<bool> IsUsernameTaken(string username)
    {
        return await _db.Accounts.FirstOrDefaultAsync(a => a.Username.ToLower() == username.ToLower()) is not null;
    }

    public async Task<Tuple<bool, string>> VerifyEmail(string code)
    {
        var verificationCode = await _db.VerificationCodes.Include(a => a.Account).FirstOrDefaultAsync(c => c.Code == code);
        if (verificationCode is null) return new Tuple<bool, string>(false, "No verification code.");
        if (verificationCode.Type != VerificationCodeType.VerifyEmail &&
            verificationCode.Type != VerificationCodeType.ResetCredentials)
            return new Tuple<bool, string>(false, "Wrong code type.");

        if (verificationCode.Code == code)
        {
            verificationCode.Account.EmailVerified = true;
            _db.Accounts.Update(verificationCode.Account);
            _db.VerificationCodes.Remove(verificationCode);
            await _db.SaveChangesAsync();
            return new Tuple<bool, string>(true, "");
        }

        return new Tuple<bool, string>(false, "Error server side.");
    }

    public async Task<bool> ChangeEmail(Guid playerId, string newEmail)
    {
        var account = await _db.Accounts.FirstOrDefaultAsync(a => a.PlayerId == playerId);
        if (account is null) return false;

        var code = GenerateVerificationCode();
        var subject = "Email Change";
        var body = $"The email address for {account.Username} has been changed to {newEmail}.  ";
        body += "If you did not request this change, please reset your email and password here: ";
        body += "https://eutania.herokuapp.com/verify-email/" + code;
        body += ".  ";
        body += "This message is being sent to your old email address only.";
        await SendEmailAsync(account.Email, subject, body);

        // Send account recovery email to OLD email
        var oldEmailCode = new VerificationCode
        {
            Code = code,
            Account = account,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            Type = VerificationCodeType.ResetCredentials
        };
        await _db.VerificationCodes.AddAsync(oldEmailCode);

        // Send verification email to NEW email
        var newCode = await SendEmailVerification(newEmail);
        var newEmailCode = new VerificationCode
        {
            Code = newCode,
            Account = account,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            Type = VerificationCodeType.ResetCredentials
        };
        await _db.VerificationCodes.AddAsync(newEmailCode);

        account.Email = newEmail;
        account.EmailVerified = false;

        _db.Accounts.Update(account);

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<Tuple<bool, string?>> ChangePassword(Guid playerId, string currentPassword,
        string newPassword,
        string confirmNewPassword)
    {
        if (newPassword != confirmNewPassword)
            return new Tuple<bool, string?>(false, "Password does not match confirm password.");

        var account = await _db.Accounts.FirstOrDefaultAsync(a => a.PlayerId == playerId);
        if (account is null) return new Tuple<bool, string?>(false, "Account not found.");

        var hasher = new PasswordHasher<Account>();
        var verifiedOldPassword = hasher.VerifyHashedPassword(account, account.HashedPassword, currentPassword);

        if (verifiedOldPassword == PasswordVerificationResult.Failed)
            return new Tuple<bool, string?>(false, "Old password is incorrect.");

        account.HashedPassword = hasher.HashPassword(account, newPassword);
        _db.Accounts.Update(account);

        // Send account recovery email
        var code = GenerateVerificationCode();
        var subject = "Password Change";
        var body = $"The password for {account.Username} has been changed.  ";
        body += "If you did not request this change, please reset your email and password here: ";
        body += "https://eutania.herokuapp.com/account-recovery/" + code;
        body += ".  ";
        await SendEmailAsync(account.Email, subject, body);
        var recoveryCode = new VerificationCode
        {
            Code = code,
            Account = account,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            Type = VerificationCodeType.ResetCredentials
        };
        await _db.VerificationCodes.AddAsync(recoveryCode);
        var updated = await _db.SaveChangesAsync();

        return new Tuple<bool, string?>(updated == 2, "Updated.");
    }

    public async Task<bool> ResetCredentials(string email, string password, string code)
    {
        var verificationCode = await _db.VerificationCodes.FirstOrDefaultAsync(c => c.Code == code);
        if (verificationCode is null) return false;
        if (verificationCode.Type != VerificationCodeType.ResetCredentials) return false;

        var account = verificationCode.Account;
        account.Email = email;
        account.EmailVerified = false;

        var hasher = new PasswordHasher<Account>();
        account.HashedPassword = hasher.HashPassword(account, password);

        var newCode = await SendEmailVerification(email);

        var dbCode = new VerificationCode
        {
            Code = newCode,
            Account = account,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            Type = VerificationCodeType.ResetCredentials
        };
        await _db.VerificationCodes.AddAsync(dbCode);

        _db.Accounts.Update(account);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> Initialize(Guid playerId, string username)
    {
        var account = await _db.Accounts.FirstOrDefaultAsync(a => a.PlayerId == playerId);
        if (account is null) return false;
        
        account.Username = username;
        account.Initialized = true;
        
        _db.Accounts.Update(account);
        return await _db.SaveChangesAsync() == 1;
    }


    private async Task<string> SendEmailVerification(string email)
    {
        // Send email verification
        var code = GenerateVerificationCode();
        var subject = "Email Verification";
        var body = "<div class=\"\">Welcome, explorer!  </div>" + 
                    "<div class=\"\">An account has been created for you with this email address.  </div>" + 
                   "<div class=\"\">Please follow the link below to activate your account: </div>" + 
                   "<div class=\"\">https://eutania.azurewebsites.net/verify-email/" + code + "</div>";
        await SendEmailAsync(email, subject, body);
        return code;
    }

    private async Task SendEmailAsync(string email, string subject, string body)
    {
        var serverEmail = _configuration.GetValue<string>("EmailService:Address");
        var serverPassword = _configuration.GetValue<string>("EmailService:Password");

        var fromAddress = new MailAddress(serverEmail, "Eutania");
        var toAddress = new MailAddress(email, email);


        var smtp = new SmtpClient
        {
            Host = "********",
            Port = 587,
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Credentials = new NetworkCredential("apikey",
                "***********")
        };
        using var message = new MailMessage(fromAddress, toAddress)
        {
            Subject = subject,
            Body = body
        };
        message.IsBodyHtml = true;
        await smtp.SendMailAsync(message);
    }

    private string GenerateVerificationCode()
    {
        return Guid.NewGuid().ToString().Replace("-", "");
    }
}