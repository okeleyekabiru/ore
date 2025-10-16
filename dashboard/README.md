# Ore Dashboard

React + Vite front-end for the Ore platform control center. The dashboard surfaces onboarding, content workflow, and analytics data from the .NET API.

## Prerequisites

- Node.js 20+
- Ore API running locally or remotely (see repository root README for backend setup)

## Environment configuration

1. Copy the sample environment file and set the API base URL:
   ```powershell
   cd dashboard
   Copy-Item .env.example .env
   ```
2. Update `VITE_API_BASE_URL` in `.env` to match the API origin (omit the trailing slash). Defaults to `http://localhost:5000` if not provided.

## Authentication

Use the login screen at `/auth/login` to exchange credentials for access and refresh tokens. After a successful sign-in the tokens are stored in `localStorage` via `src/services/authStorage.ts`, refreshed automatically before expiry, and user details are rehydrated on page reload so protected routes (such as the brand survey list) stay accessible.

- Create accounts via the API endpoint `POST /api/auth/register` (or seed users in the backend).
- Existing tokens can be cleared with the **Sign out** action in the dashboard header.

## Install & run

```powershell
npm install
npm run dev
```

Open the Vite dev server (default `http://localhost:5173`).

To run the dashboard alongside the API and infrastructure, use the root-level `docker compose up --build` workflow documented in `../README.md`.

## Current views

- **Overview**: Initial metrics cards and milestones placeholders.
- **Brand Survey**: Calls `GET /api/brand-surveys` and displays survey templates or helpful error messaging when authentication is missing.
- **Content Pipeline**: Layout stub ready for future API wiring.

## Next steps

1. Connect remaining modules (content pipeline, scheduling, notifications) once backend endpoints are available.
2. Add a data-fetching layer (React Query, SWR) with caching and optimistic updates as the API stabilises.
3. Implement automatic refresh token rotation on navigation errors (retry after 401) and surface session expiry warnings when refresh fails.
