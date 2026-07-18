using System.Collections.Concurrent;
using AccountOpeningDemo.Models;

namespace AccountOpeningDemo.Services;

/// <summary>
/// Tracks the state of each onboarding session as it moves through
/// account linking, identity verification, and notification.
/// In-memory and process-local by design, this is a demo, not a
/// system meant to survive a restart or run across multiple instances.
/// </summary>
public class OnboardingStatusService
{
    private readonly ConcurrentDictionary<string, OnboardingSession> _sessions = new();

    public OnboardingSession CreateSession(string name, string email)
    {
        var session = new OnboardingSession
        {
            ApplicantName = name,
            ApplicantEmail = email
        };
        _sessions[session.SessionId] = session;
        return session;
    }

    public OnboardingSession? GetSession(string sessionId)
    {
        _sessions.TryGetValue(sessionId, out var session);
        return session;
    }

    public void MarkAccountLinked(string sessionId, string plaidAccountId, string institutionName)
    {
        if (!_sessions.TryGetValue(sessionId, out var session)) return;
        session.AccountLinked = true;
        session.PlaidAccountId = plaidAccountId;
        session.PlaidInstitutionName = institutionName;
    }

    public void MarkIdentityVerification(string sessionId, string verificationSessionId, string status)
    {
        if (!_sessions.TryGetValue(sessionId, out var session)) return;
        session.StripeVerificationSessionId = verificationSessionId;
        session.StripeVerificationStatus = status;
        session.IdentityVerified = status == "verified";
    }

    public void MarkNotificationSent(string sessionId)
    {
        if (!_sessions.TryGetValue(sessionId, out var session)) return;
        session.NotificationSent = true;
    }
}
