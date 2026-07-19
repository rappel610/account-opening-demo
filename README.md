# Digital Account Opening Demo

A solo, end-to-end simulation of the account-opening flow used by real Banking-as-a-Service platforms: link a bank account, verify identity, and receive a confirmation email, built by integrating three real vendor APIs from scratch.

**Demo video:** https://www.loom.com/share/77f9d7dae4f040028c70ca4e279f3c4c
**Live app:** https://account-opening-demo.vercel.app

## Why I built this

I've spent three years implementing and debugging vendor integrations (Middesk, Socure, MoneyKit, Kinective, SendGrid) inside an existing production banking platform serving 400,000+ credit union members. That work is almost entirely about consuming and troubleshooting integrations someone else designed.

This project is the other half of that story: building one from scratch, alone, reading unfamiliar docs with no existing codebase or team to lean on. It's meant to show the same integration instincts applied end to end, from an empty repo to a working flow.

## What it does

A user fills out a short form, then moves through three real vendor integrations:

1. **Account linking** via [Plaid](https://plaid.com/docs/) — the Plaid Link widget connects a bank account and retrieves account details from a test institution.
2. **Identity verification** via [Stripe Identity](https://stripe.com/docs/identity) — a verification session launches Stripe's hosted document/selfie check and the backend polls for a verified result.
3. **Confirmation email** via [SendGrid](https://docs.sendgrid.com/) — an approval email sends automatically once both prior steps succeed.

The backend orchestrates all three integrations behind a simple status endpoint, so the frontend can show live progress through the flow.

## Stack

- **Backend:** ASP.NET Core Web API (C#)
- **Frontend:** React + TypeScript (Vite)
- **Vendors:** Plaid (sandbox), Stripe Identity (test mode), SendGrid (free tier)

## A few notes on the build

- Token handoffs (Plaid's link token/public token exchange, Stripe's client secret) are handled server-side only; vendor secrets never reach the browser.
- Identity verification status is polled rather than handled via webhook, a deliberate scope choice for a project without a stable public endpoint for vendors to call back to.
- This is a demo, not a product: no user authentication, no production-grade security hardening, and error handling covers the happy path rather than every edge case. The goal was clean, correct integration plumbing end to end, not a shippable onboarding product.

## Setup

Setup instructions, including how to get sandbox API keys for each vendor and run the project locally, are in [SETUP.md](./SETUP.md).