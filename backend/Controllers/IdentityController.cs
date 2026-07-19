using System.Text.Json;
using AccountOpeningDemo.Models;
using AccountOpeningDemo.Services;
using Microsoft.AspNetCore.Mvc;

namespace AccountOpeningDemo.Controllers;

[ApiController]
[Route("api/identity")]
public class IdentityController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OnboardingStatusService _statusService;

    public IdentityController(IHttpClientFactory httpClientFactory, OnboardingStatusService statusService)
    {
        _httpClientFactory = httpClientFactory;
        _statusService = statusService;
    }


    [HttpPost("verification-session")]
    public async Task<IActionResult> CreateVerificationSession([FromBody] CreateIdentitySessionRequest request)
    {
        var client = _httpClientFactory.CreateClient("Stripe");

        var form = new List<KeyValuePair<string, string>>
        {
            new("type", "document"),
            new("metadata[session_id]", request.SessionId)
        };

        var response = await client.PostAsync(
            "identity/verification_sessions",
            new FormUrlEncodedContent(form));

        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode, body);
        }

        using var doc = JsonDocument.Parse(body);
        var verificationSessionId = doc.RootElement.GetProperty("id").GetString();
        var clientSecret = doc.RootElement.GetProperty("client_secret").GetString();
        var status = doc.RootElement.GetProperty("status").GetString() ?? "requires_input";

        _statusService.MarkIdentityVerification(request.SessionId, verificationSessionId ?? "", status);

        return Ok(new { verificationSessionId, clientSecret, status });
    }

    [HttpGet("verification-session/{verificationSessionId}")]
    public async Task<IActionResult> GetVerificationStatus(string verificationSessionId, [FromQuery] string sessionId)
    {
        var client = _httpClientFactory.CreateClient("Stripe");

        var response = await client.GetAsync($"identity/verification_sessions/{verificationSessionId}");
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode, body);
        }

        using var doc = JsonDocument.Parse(body);
        var status = doc.RootElement.GetProperty("status").GetString() ?? "requires_input";

        _statusService.MarkIdentityVerification(sessionId, verificationSessionId, status);

        return Ok(new { verificationSessionId, status });
    }
}
