using System.Text;
using System.Text.Json;
using AccountOpeningDemo.Models;
using AccountOpeningDemo.Services;
using Microsoft.AspNetCore.Mvc;

namespace AccountOpeningDemo.Controllers;

[ApiController]
[Route("api/plaid")]
public class PlaidController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    private readonly OnboardingStatusService _statusService;

    public PlaidController(
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        OnboardingStatusService statusService)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
        _statusService = statusService;
    }

    /// <summary>
    /// Step 1: create a Link token, which the frontend uses to initialize
    /// the Plaid Link widget. This has to happen server-side because it
    /// requires the client_id/secret pair, which should never reach the browser.
    /// </summary>
    [HttpPost("link-token")]
    public async Task<IActionResult> CreateLinkToken([FromBody] CreateIdentitySessionRequest request)
    {
        var client = _httpClientFactory.CreateClient("Plaid");

        var payload = new
        {
            client_id = _config["Plaid:ClientId"],
            secret = _config["Plaid:Secret"],
            client_name = "Account Opening Demo",
            country_codes = new[] { "US" },
            language = "en",
            user = new { client_user_id = request.SessionId },
            products = new[] { "auth" }
        };

        var response = await client.PostAsync(
            "/link/token/create",
            new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));

        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode, body);
        }

        using var doc = JsonDocument.Parse(body);
        var linkToken = doc.RootElement.GetProperty("link_token").GetString();

        return Ok(new { linkToken });
    }

    /// <summary>
    /// Step 2: once the user completes the Plaid Link widget in the
    /// frontend, exchange the public_token it returns for a permanent
    /// access_token, then pull basic account info to confirm the link
    /// actually worked. In a real system the access_token would be
    /// encrypted at rest, never logged, and tied to the member's record.
    /// </summary>
    [HttpPost("exchange-token")]
    public async Task<IActionResult> ExchangePublicToken([FromBody] ExchangePublicTokenRequest request)
    {
        var client = _httpClientFactory.CreateClient("Plaid");

        var exchangePayload = new
        {
            client_id = _config["Plaid:ClientId"],
            secret = _config["Plaid:Secret"],
            public_token = request.PublicToken
        };

        var exchangeResponse = await client.PostAsync(
            "/item/public_token/exchange",
            new StringContent(JsonSerializer.Serialize(exchangePayload), Encoding.UTF8, "application/json"));

        var exchangeBody = await exchangeResponse.Content.ReadAsStringAsync();
        if (!exchangeResponse.IsSuccessStatusCode)
        {
            return StatusCode((int)exchangeResponse.StatusCode, exchangeBody);
        }

        using var exchangeDoc = JsonDocument.Parse(exchangeBody);
        var accessToken = exchangeDoc.RootElement.GetProperty("access_token").GetString();

        var accountsPayload = new
        {
            client_id = _config["Plaid:ClientId"],
            secret = _config["Plaid:Secret"],
            access_token = accessToken
        };

        var accountsResponse = await client.PostAsync(
            "/accounts/get",
            new StringContent(JsonSerializer.Serialize(accountsPayload), Encoding.UTF8, "application/json"));

        var accountsBody = await accountsResponse.Content.ReadAsStringAsync();
        if (!accountsResponse.IsSuccessStatusCode)
        {
            return StatusCode((int)accountsResponse.StatusCode, accountsBody);
        }

        using var accountsDoc = JsonDocument.Parse(accountsBody);
        var firstAccount = accountsDoc.RootElement.GetProperty("accounts")[0];
        var accountId = firstAccount.GetProperty("account_id").GetString() ?? "unknown";
        var institutionName = accountsDoc.RootElement
            .GetProperty("item")
            .GetProperty("institution_id")
            .GetString() ?? "Unknown Institution";

        _statusService.MarkAccountLinked(request.SessionId, accountId, institutionName);

        return Ok(new { accountId, institutionName });
    }
}
