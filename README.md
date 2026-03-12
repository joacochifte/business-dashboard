# Business Dashboard

Full-stack portfolio app that simulates a small-business control panel: product catalog, inventory, sales (with debts and internal notes), customers, costs, and a metrics dashboard.

## Features

- **Products**: CRUD with optional stock tracking (items with `stock = null` don’t affect inventory).
- **Inventory**: manual adjustments plus a movement log filtered by product and date range.
- **Sales**:
  - Multiple items per sale with optional “special price”.
  - Optional customer, notes and payment method.
  - Debt flag per sale.
  - Stock validation/discount for tracked products.
- **Customers**: CRUD, search/sort, contact info, lifetime value and last purchase stats.
- **Debts**: dedicated page (`/debts`) backed by the sales debt filter.
- **Costs**: CRUD for operational expenses.
- **Dashboard**:
  - KPIs (revenue, sales count, average ticket, debts).
  - Sales by period (daily/monthly/yearly/all time).
  - Costs by period (same range as sales).
  - Top products (by revenue or quantity).
  - Sales + costs widgets rearranged for a denser layout.
- **API**: ASP.NET Core 8 with ProblemDetails responses and Swagger in Development.

## Tech Stack

- **Backend**: .NET 8 Web API + EF Core.
- **Frontend**: Next.js (App Router) + Tailwind CSS.
- **Database**: PostgreSQL 16 (Docker).
- **Tests**: MSTest.

## Getting Started

### Environment files

- Copy `.env.local.example` → `.env` (repo root) for Docker services (ports, Postgres credentials, etc.).
- Copy `frontend/.env.local.example` → `frontend/.env.local` and set `NEXT_PUBLIC_API_BASE_URL` to the backend URL you’ll run.

### Option 1 — Everything via Docker

```bash
# from repo root
docker compose up -d              # reuse existing images
# or force rebuild if the code changed
docker compose up -d --build
```

On Windows you can run `ops\start-tester.bat` (pass `--build` to trigger image rebuild).

Default ports (see `.env`):

- Frontend: http://localhost:3000
- Backend: http://localhost:5126
- Postgres: localhost:5433 (`business_dashboard` / `postgres` / `postgres`)

### Option 2 — Hybrid dev (Postgres in Docker, apps local)

1. Start DB only:
   ```bash
   docker compose up -d postgres
   ```
2. Backend:
   ```bash
   cd backend
   dotnet restore
   dotnet run --project src/BusinessDashboard.Api
   ```
3. Frontend:
   ```bash
   cd frontend
   npm install
   npm run dev
   ```
4. Ensure `frontend/.env.local` points to the actual backend port (`NEXT_PUBLIC_API_BASE_URL=http://localhost:<kestrel-port>`).

### EF Core migrations

Apply the latest migration (e.g., the `Notes` column on `Sales`) with:

```bash
dotnet ef database update \
  -p backend/src/BusinessDashboard.Infrastructure/BusinessDashboard.Infrastructure.csproj \
  -s backend/src/BusinessDashboard.Api/BusinessDashboard.Api.csproj
```

Install the CLI if needed: `dotnet tool install --global dotnet-ef`.

## Useful scripts

- `ops/start-tester.bat` → spin up the whole stack (pass `--build` to rebuild images).
- `ops/stop-tester.bat` → stop containers.
- `ops/backup-data.bat` / `ops/restore-data.bat` → backup/restore Postgres inside Docker.

## Tests

```bash
cd backend
dotnet test
```

## Additional notes

- Timestamps are stored in UTC; the UI renders them using the browser’s locale.
- There’s a single responsive navbar (`AppNav`). Avoid duplicating it.
- When using `npm run dev`, stop the `frontend` container to ensure you’re hitting the Vite/Next dev server and not stale Docker bundles.
