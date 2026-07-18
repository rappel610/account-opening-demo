# Digital Account Opening Demo

A solo end-to-end demo that mirrors the account-opening flow used by real Banking-as-a-Service platforms: connect a bank account, verify identity, and receive a confirmation email. Built to demonstrate hands-on integration work with production-grade vendor APIs, outside of a day job.

## What it does

A user fills out a short onboarding form, then moves through three steps:

1. **Account linking** via [Plaid](https://plaid.com/docs/) — connects a bank account using Plaid Link and retrieves basic account info from a test institution.
2. **Identity verification** via [Stripe Identity](https://stripe.com/docs/identity) — creates a verification session and confirms the user's identity in test mode.
3. **Confirmation notification** via [SendGrid](https://docs.sendgrid.com/) — sends an approval email once both steps succeed.

The backend orchestrates all three integrations and exposes a simple status endpoint so the frontend can show the user's progress through the flow.

## Why this exists

Three years of production experience has been implementing and debugging vendor integrations (Middesk, Socure, MoneyKit, Kinective, SendGrid) inside an existing banking platform. This project is the other half of that story: building an integration from scratch, alone, reading unfamiliar docs with no team or existing codebase to lean on.

## Stack

- **Backend:** ASP.NET Core Web API (C#)
- **Frontend:** React + TypeScript (Vite)
- **Vendors:** Plaid (sandbox), Stripe Identity (test mode), SendGrid (free tier)

## Project structure

```
account-opening-demo/
├── backend/
│   ├── Controllers/
│   │   ├── PlaidController.cs        # link token creation + public token exchange
│   │   ├── IdentityController.cs     # Stripe Identity verification sessions
│   │   └── NotificationController.cs # SendGrid confirmation email
│   ├── Services/
│   │   └── OnboardingStatusService.cs # in-memory status tracking per session
│   ├── Models/
│   │   └── OnboardingModels.cs
│   ├── Program.cs
│   ├── appsettings.Example.json      # copy to appsettings.Development.json and fill in keys
│   └── AccountOpeningDemo.csproj
└── frontend/
    ├── src/
    │   ├── components/
    │   │   ├── PlaidLinkButton.tsx
    │   │   └── StatusFlow.tsx
    │   ├── App.tsx
    │   └── main.tsx
    ├── index.html
    ├── package.json
    └── vite.config.ts
```

## Setup

### 1. Get sandbox/test API keys (all free, self-serve, no sales call)

- **Plaid:** sign up at dashboard.plaid.com, grab your `client_id` and `sandbox` secret from the Keys page.
- **Stripe:** sign up at dashboard.stripe.com, grab your test-mode secret key, and enable Identity in the dashboard.
- **SendGrid:** sign up at sendgrid.com, create an API key with Mail Send permission, and verify a single sender email.

### 2. Backend

```bash
cd backend
cp appsettings.Example.json appsettings.Development.json
# fill in your Plaid, Stripe, and SendGrid keys in appsettings.Development.json
dotnet restore
dotnet run
```

API runs on `https://localhost:5001` by default.

### 3. Frontend

```bash
cd frontend
npm install
npm run dev
```

App runs on `http://localhost:5173` and proxies API calls to the backend.

## Build order (recommended, roughly a weekend)

1. Plaid Link end-to-end (link token → Plaid Link widget → public token exchange → account info)
2. Stripe Identity verification session (create session → redirect/embed → poll for verified status)
3. SendGrid confirmation email, fired once both prior steps report success
4. Wire the three into one flow with a simple status view
5. Deploy: frontend to Vercel, backend to Render or Fly.io (both have free tiers)
6. Record a 60–90 second screen capture walking through the flow

## Explicit non-goals

This is a demo, not a product. Deliberately skipped: real user authentication, production-grade security hardening, a polished design system, and error handling beyond the happy path. The point is to show clean, correct integration plumbing, not to ship a real onboarding product.

## Live demo

_(add link once deployed)_

## Demo video

_(add link once recorded)_
