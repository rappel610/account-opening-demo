using AccountOpeningDemo.Services;
using Microsoft.AspNetCore.Mvc;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace AccountOpeningDemo.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly OnboardingStatusService _statusService;

    public NotificationController(IConfiguration config, OnboardingStatusService statusService)
    {
        _config = config;
        _statusService = statusService;
    }

    [HttpPost("send-confirmation")]
    public async Task<IActionResult> SendConfirmation([FromBody] SendConfirmationRequest request)
    {
        var session = _statusService.GetSession(request.SessionId);
        if (session is null) return NotFound();

        if (!session.AccountLinked || !session.IdentityVerified)
        {
            return BadRequest(new
            {
                error = "Cannot send confirmation before account linking and identity verification are both complete."
            });
        }

        var apiKey = _config["SendGrid:ApiKey"];
        var fromEmail = _config["SendGrid:FromEmail"];
        var fromName = _config["SendGrid:FromName"];

        var client = new SendGridClient(apiKey);
        var from = new EmailAddress(fromEmail, fromName);
        var to = new EmailAddress(session.ApplicantEmail, session.ApplicantName);

        var message = MailHelper.CreateSingleEmail(
            from,
            to,
            "Your account is approved",
            $"Hi {session.ApplicantName}, your account has been verified and your bank account " +
            $"at {session.PlaidInstitutionName} is linked. You're all set.",
            $"<p>Hi {session.ApplicantName},</p>" +
            $"<p>Your identity has been verified and your bank account at " +
            $"<strong>{session.PlaidInstitutionName}</strong> is linked.</p>" +
            $"<p>You're all set.</p>"
        );

        var response = await client.SendEmailAsync(message);

        if ((int)response.StatusCode >= 200 && (int)response.StatusCode < 300)
        {
            _statusService.MarkNotificationSent(request.SessionId);
            return Ok(new { sent = true });
        }

        var errorBody = await response.Body.ReadAsStringAsync();
        return StatusCode((int)response.StatusCode, errorBody);
    }
}

public record SendConfirmationRequest(string SessionId);
