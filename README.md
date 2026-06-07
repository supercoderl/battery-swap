# VinFast Battery Swapping Cabinet Management System

Enterprise platform for managing VinFast battery swapping cabinets for electric motorcycle
drivers — monitoring stations, cabinets, slots, batteries, telemetry, swapping sessions and
transactions.

## Tech Stack

| Layer    | Technology |
|----------|------------|
| Backend  | ASP.NET Core 8 Web API, EF Core 8, SQL Server, JWT, AutoMapper, FluentValidation, Swagger |
| Frontend | Next.js 16 (App Router), TypeScript, TailwindCSS v4, shadcn-style UI, TanStack Query, Zustand, React Hook Form, Zod, Axios, Recharts |
| Patterns | Clean Architecture, Repository + Unit of Work, Service Layer, DTOs, DI, SOLID |

## Solution Structure

```
battery-swap/
├─ backend/
│  ├─ BatterySwap.sln
│  └─ src/
│     ├─ BatterySwap.Domain/          # Entities, enums, base types (no dependencies)
│     ├─ BatterySwap.Application/     # DTOs, interfaces, services, validators, mapping
│     ├─ BatterySwap.Infrastructure/  # EF DbContext, configs, repositories, JWT, seed
│     └─ BatterySwap.API/             # Controllers, middleware, Program.cs, Swagger
└─ frontend/
   └─ src/
      ├─ app/                         # App Router pages (login + (dashboard) group)
      ├─ components/ (ui, layout, charts, shared)
      ├─ hooks/      (TanStack Query resource hooks, auth)
      ├─ services/   (Axios API clients)
      ├─ stores/     (Zustand auth store)
      ├─ lib/        (axios instance, helpers)
      └─ types/      (shared TypeScript models)
```

## Prerequisites

- .NET 8 SDK (or newer — projects target `net8.0`)
- Node.js 20.9+
- SQL Server (local instance, Docker, or LocalDB)

## Backend — Run

```bash
cd backend

# 1. Set your SQL Server connection string in src/BatterySwap.API/appsettings.json
#    ("ConnectionStrings:DefaultConnection")

# 2. Apply migrations (also runs automatically on startup) + run
dotnet run --project src/BatterySwap.API
```

- API:     http://localhost:5000
- Swagger: http://localhost:5000/swagger

On first launch the database is **migrated and seeded** automatically (5 stations, cabinets,
slots, ~150 batteries with telemetry, 40 users, 300 transactions, active sessions).
Toggle seeding with `SeedOnStartup` in `appsettings.json`.

### Demo login accounts

| Username   | Password         | Role       |
|------------|------------------|------------|
| admin      | Admin@123        | ADMIN      |
| operator   | Operator@123     | OPERATOR   |
| supervisor | Supervisor@123   | SUPERVISOR |

### Recreating migrations

```bash
dotnet ef migrations add <Name> \
  --project src/BatterySwap.Infrastructure \
  --startup-project src/BatterySwap.API \
  --output-dir Persistence/Migrations
```

## Frontend — Run

```bash
cd frontend
npm install
npm run dev      # http://localhost:3000
```

The API base URL is read from `.env.local` (`NEXT_PUBLIC_API_URL`, default
`http://localhost:5000/api`). CORS on the backend already allows `http://localhost:3000`.

## Features

- **Auth** — JWT login, transparent refresh-token rotation, change password, logout
- **Dashboard** — 8 stat cards, 6 charts, latest transactions / telemetry / active sessions
- **Stations / Cabinets / Slots / Batteries / Users** — full CRUD with search, filter, sort, pagination
- **Battery detail** — live SOC / temperature / voltage telemetry charts (auto-refresh)
- **Sessions** — live monitoring + force-close
- **Transactions** — search, date & status filters, detail view, duration
- **Reports** — daily/monthly swaps, battery utilization & health, cabinet utilization
- **UI** — responsive enterprise theme (primary `#0057B8`), dark mode, sidebar navigation

## API Surface (selected)

| Method | Route | Description |
|--------|-------|-------------|
| POST   | `/api/auth/login` | Authenticate, returns tokens |
| POST   | `/api/auth/refresh` | Rotate access token |
| GET    | `/api/dashboard/overview` | Stats + charts + latest tables |
| GET/POST/PUT/DELETE | `/api/stations` | Station CRUD |
| GET/POST/PUT/DELETE | `/api/cabinets` | Cabinet CRUD (filter by station/status) |
| GET/POST/PUT/DELETE | `/api/slots` | Slot CRUD |
| GET/POST/PUT/DELETE | `/api/batteries` | Battery CRUD |
| GET    | `/api/batteries/{id}/logs` | Telemetry history |
| GET/POST/PUT/DELETE | `/api/users` | User CRUD |
| GET/DELETE | `/api/sessions` | Active sessions + force close |
| GET    | `/api/transactions` | Transaction list + filters |
| GET    | `/api/reports/*` | Reporting endpoints |

All list endpoints support `?page=&pageSize=&search=&sortBy=&sortDescending=` and return a
uniform `ApiResponse<PagedResult<T>>` envelope.
