namespace AccountOpeningDemo.Models;

public class OnboardingSession
{
    public string SessionId { get; set; } = Guid.NewGuid().ToString();
    public string ApplicantEmail { get; set; } = string.Empty;
    public string ApplicantName { get; set; } = string.Empty;

    public bool AccountLinked { get; set; }
    public string? PlaidAccountId { get; set; }
    public string? PlaidInstitutionName { get; set; }

    public bool IdentityVerified { get; set; }
    public string? StripeVerificationSessionId { get; set; }
    public string? StripeVerificationStatus { get; set; }

    public bool NotificationSent { get; set; }

    public bool IsComplete => AccountLinked && IdentityVerified && NotificationSent;
}

public record CreateSessionRequest(string ApplicantName, string ApplicantEmail);

public record ExchangePublicTokenRequest(string SessionId, string PublicToken);

public record CreateIdentitySessionRequest(string SessionId);

public record OnboardingStatusResponse(
    string SessionId,
    bool AccountLinked,
    string? InstitutionName,
    bool IdentityVerified,
    string? VerificationStatus,
    bool NotificationSent,
    bool IsComplete
);
