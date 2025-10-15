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

## Authentication tokens

Protected endpoints (for example `GET /api/brand-surveys`) require a bearer token.

1. Authenticate against the API: `POST /api/auth/login`.
2. Persist the tokens in `localStorage` so the dashboard can attach the `Authorization` header:
   ```js
   localStorage.setItem('ore:access-token', '<ACCESS_TOKEN>');
   localStorage.setItem('ore:refresh-token', '<REFRESH_TOKEN>');
   ```
3. Refresh the dashboard to retry pending requests.

The access/refresh storage layer lives at `src/services/authStorage.ts`.

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

1. Build a proper login flow that exchanges credentials for tokens and refreshes access in the background.
2. Connect remaining modules (content pipeline, scheduling, notifications) once backend endpoints are available.
3. Add a data-fetching layer (React Query, SWR) with caching and optimistic updates as the API stabilises.
