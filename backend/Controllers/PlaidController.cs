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
