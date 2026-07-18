import { useState } from "react";
import { loadStripe } from "@stripe/stripe-js";
import { PlaidLinkButton } from "./components/PlaidLinkButton";
import { StatusFlow } from "./components/StatusFlow";


const stripePromise = loadStripe(import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY);

type Stage = "form" | "linking" | "verifying" | "notifying" | "complete";

interface Status {
  accountLinked: boolean;
  institutionName: string | null;
  identityVerified: boolean;
  verificationStatus: string | null;
  notificationSent: boolean;
}

const initialStatus: Status = {
  accountLinked: false,
  institutionName: null,
  identityVerified: false,
  verificationStatus: null,
  notificationSent: false,
};

export default function App() {
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [sessionId, setSessionId] = useState<string | null>(null);
  const [linkToken, setLinkToken] = useState<string | null>(null);
  const [stage, setStage] = useState<Stage>("form");
  const [status, setStatus] = useState<Status>(initialStatus);
  const [error, setError] = useState<string | null>(null);

  async function startOnboarding(e: React.FormEvent) {
    e.preventDefault();
    setError(null);

    const sessionRes = await fetch("/api/onboarding/session", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ applicantName: name, applicantEmail: email }),
    });
    if (!sessionRes.ok) {
      setError("Could not start onboarding session.");
      return;
    }
    const { sessionId: newSessionId } = await sessionRes.json();
    setSessionId(newSessionId);

    const linkTokenRes = await fetch("/api/plaid/link-token", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ sessionId: newSessionId }),
    });
    if (!linkTokenRes.ok) {
      setError("Could not create Plaid link token.");
      return;
    }
    const { linkToken: newLinkToken } = await linkTokenRes.json();
    setLinkToken(newLinkToken);
    setStage("linking");
  }

  async function handlePlaidSuccess(publicToken: string) {
    if (!sessionId) return;

    const exchangeRes = await fetch("/api/plaid/exchange-token", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ sessionId, publicToken }),
    });
    if (!exchangeRes.ok) {
      setError("Could not link bank account.");
      return;
    }
    const { institutionName } = await exchangeRes.json();
    setStatus((prev) => ({ ...prev, accountLinked: true, institutionName }));
    setStage("verifying");

    await startIdentityVerification();
  }

  async function startIdentityVerification() {
    if (!sessionId) return;
    setStage("verifying");

    const verificationRes = await fetch("/api/identity/verification-session", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ sessionId }),
    });
    if (!verificationRes.ok) {
      setError("Could not start identity verification.");
      return;
    }

    const { verificationSessionId, clientSecret } = await verificationRes.json();

    const stripe = await stripePromise;
    if (!stripe) {
      setError("Stripe.js failed to load. Check your publishable key.");
      return;
    }

  
    const { error: verifyError } = await stripe.verifyIdentity(clientSecret);

    if (verifyError) {
      setError(`Identity verification was not completed: ${verifyError.message}`);
      return;
    }

    await pollVerificationStatus(verificationSessionId);
  }

  async function pollVerificationStatus(verificationSessionId: string, attempt = 0) {
    if (!sessionId) return;

    const res = await fetch(
      `/api/identity/verification-session/${verificationSessionId}?sessionId=${sessionId}`
    );
    if (!res.ok) {
      setError("Could not check verification status.");
      return;
    }

    const { status: verificationStatus } = await res.json();
    setStatus((prev) => ({
      ...prev,
      identityVerified: verificationStatus === "verified",
      verificationStatus,
    }));

    if (verificationStatus === "verified") {
      await sendConfirmation();
      return;
    }

    if (attempt < 10) {
      await new Promise((resolve) => setTimeout(resolve, 2000));
      await pollVerificationStatus(verificationSessionId, attempt + 1);
    } else {
      setError(
        `Verification is still processing (status: ${verificationStatus}). Refresh in a moment to check again.`
      );
    }
  }

  async function sendConfirmation() {
    if (!sessionId) return;
    setStage("notifying");

    const res = await fetch("/api/notifications/send-confirmation", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ sessionId }),
    });
    if (!res.ok) {
      setError("Could not send confirmation email.");
      return;
    }

    setStatus((prev) => ({ ...prev, notificationSent: true }));
    setStage("complete");
  }

  return (
    <div style={{ maxWidth: 480, margin: "3rem auto", fontFamily: "system-ui, sans-serif" }}>
      <h1>Account Opening Demo</h1>
      <p style={{ color: "#666" }}>
        A small end-to-end demo integrating Plaid, Stripe Identity, and SendGrid.
      </p>

      {stage === "form" && (
        <form onSubmit={startOnboarding}>
          <div style={{ marginBottom: "1rem" }}>
            <label>Full name</label>
            <input value={name} onChange={(e) => setName(e.target.value)} required style={{ display: "block", width: "100%" }} />
          </div>
          <div style={{ marginBottom: "1rem" }}>
            <label>Email</label>
            <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required style={{ display: "block", width: "100%" }} />
          </div>
          <button type="submit">Start onboarding</button>
        </form>
      )}

      {stage === "linking" && linkToken && (
        <PlaidLinkButton linkToken={linkToken} onSuccess={handlePlaidSuccess} />
      )}

      {error && <p style={{ color: "crimson" }}>{error}</p>}

      {stage !== "form" && <StatusFlow {...status} />}

      {stage === "complete" && <p>All set. Confirmation email sent to {email}.</p>}
    </div>
  );
}