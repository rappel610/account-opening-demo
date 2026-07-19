using AccountOpeningDemo.Models;
using AccountOpeningDemo.Services;
using Microsoft.AspNetCore.Mvc;

namespace AccountOpeningDemo.Controllers;

[ApiController]
[Route("api/onboarding")]
public class OnboardingController : ControllerBase
{
    private readonly OnboardingStatusService _statusService;

    public OnboardingController(OnboardingStatusService statusService)
    {
        _statusService = statusService;
    }

    [HttpPost("session")]
    public IActionResult CreateSession([FromBody] CreateSessionRequest request)
    {
        var session = _statusService.CreateSession(request.ApplicantName, request.ApplicantEmail);
        return Ok(new { sessionId = session.SessionId });
    }

    [HttpGet("session/{sessionId}/status")]
    public IActionResult GetStatus(string sessionId)
    {
        var session = _statusService.GetSession(sessionId);
        if (session is null) return NotFound();

        var response = new OnboardingStatusResponse(
            session.SessionId,
            session.AccountLinked,
            session.PlaidInstitutionName,
            session.IdentityVerified,
            session.StripeVerificationStatus,
            session.NotificationSent,
            session.IsComplete
        );

        return Ok(response);
    }
}
