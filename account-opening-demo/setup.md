# Setup

## 1. Get sandbox/test API keys (all free, self-serve, no sales call)

- **Plaid:** sign up at dashboard.plaid.com, grab your `client_id` and `sandbox` secret from the Keys page.
- **Stripe:** sign up at dashboard.stripe.com, grab your test-mode secret key and publishable key, and enable Identity in the dashboard.
- **SendGrid:** sign up at sendgrid.com, create an API key with Mail Send permission, and verify a single sender email.

## 2. Backend

```bash
cd backend
cp appsettings.Example.json appsettings.Development.json
# fill in your Plaid, Stripe, and SendGrid keys in appsettings.Development.json
dotnet restore
dotnet run
```

API runs on `http://localhost:5000` by default (or `https://localhost:5001` if HTTPS is configured).

## 3. Frontend

```bash
cd frontend
cp .env.example .env
# fill in your Stripe publishable key in .env
npm install
npm run dev
```

App runs on `http://localhost:5173` and proxies API calls to the backend.

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
    ├── .env.example                  # copy to .env and fill in your Stripe publishable key
    ├── index.html
    ├── package.json
    └── vite.config.ts
```