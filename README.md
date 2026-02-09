# Business Dashboard

Portfolio project: a realistic business dashboard for a small family business.

It covers the essentials you would expect in a real system: product catalog, optional stock tracking with inventory movements, sales, and a dashboard with metrics and charts.

## Features (current)

- Products
  - Create / edit / delete
  - Optional stock tracking (`stock = null` means *untracked*, useful for services)
- Inventory
  - Manual stock adjustments
  - Movements list (filter by product and date range)
- Sales
  - Create sales with multiple items
  - Validates stock and discounts it (tracked products only)
- Costs
  - Create / edit / delete
- Dashboard
  - Summary: revenue total, sales count, average ticket
  - Sales by period (day/month/year/all time)
  - Top products (by revenue)
  - Costs by period
  - Total Gains (Revenue - Costs) by period
- API
  - Swagger in Development
  - Consistent error payloads (ProblemDetails via middleware)

## Tech stack

- Backend: .NET 8 Web API
- Frontend: Next.js (App Router) + Tailwind CSS
- Database: PostgreSQL 16 (Docker)
- Tests: MSTest

## Getting started (local dev)

### Prerequisites

- .NET SDK 8
- Node.js (recommended: 20+)
- Docker (for Postgres)

### 1) Start Postgres (Docker)

From the repo root:

```bash
docker compose up -d
```

Postgres settings (from `docker-compose.yml`):

- Host: `localhost`
- Port: `5433`
- Database: `business_dashboard`
- User: `postgres`
- Password: `postgres`

### 2) Run backend API

```bash
cd backend
dotnet restore
dotnet run --project src/BusinessDashboard.Api
```

- API base URL: check console output (Kestrel picks an available port)
- Swagger: `http://localhost:<port>/swagger`

Connection string is in:

- `backend/src/BusinessDashboard.Api/appsettings.json`

#### Apply migrations

If you have the EF Core tools installed:

```bash
dotnet ef database update \
  --project src/BusinessDashboard.Infrastructure \
  --startup-project src/BusinessDashboard.Api
```

If you don't:

```bash
dotnet tool install --global dotnet-ef
```

### 3) Run frontend (Next.js)

```bash
cd frontend
npm install
```

Create `frontend/.env.local`:

```bash
NEXT_PUBLIC_API_BASE_URL=http://localhost:<api-port>
```

Then:

```bash
npm run dev
```

Open:

- `http://localhost:3000`

## Running tests

From `backend/`:

```bash
dotnet test
```

## Notes

- Timestamps are stored in the database as UTC. The UI renders dates using the browser's local timezone.

