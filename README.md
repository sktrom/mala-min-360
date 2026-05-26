# Mala Min 360

Mala Min 360 is an Arabic-first real estate SaaS MVP for agencies that need professional property pages, media management, 360 tours, visitor-facing galleries, simple stats, and manual subscription limits.

Brand: `Mala Min 360` / `مالا من 360`

Core message: `بيتك من كل زاوية`

## Tech Stack

- Backend: ASP.NET Core Minimal API, Entity Framework Core, PostgreSQL
- Frontend: Next.js App Router, TypeScript, RTL Arabic UI
- Database: PostgreSQL through Docker Compose
- Media: local development filesystem storage under `storage/uploads`
- 360 viewer: Photo Sphere Viewer

## Repository Layout

```text
apps/api/MalaMin.Api   ASP.NET Core API
apps/web               Next.js frontend
docs                   Architecture, API, database, QA, and deployment notes
docker-compose.yml     Local PostgreSQL
```

## Environment

Root development database values are documented in `.env.example`.

Frontend configuration:

```powershell
copy apps\web\.env.example apps\web\.env.local
```

Default frontend API base URL:

```text
NEXT_PUBLIC_API_BASE_URL=http://localhost:5000
```

Do not commit `.env`, `.env.local`, production JWT signing keys, database passwords, uploaded files, or generated build output.

## Local Backend Setup

Start PostgreSQL:

```powershell
docker compose up -d postgres
docker compose ps
```

Apply migrations:

```powershell
dotnet ef database update --project apps/api/MalaMin.Api/MalaMin.Api.csproj --startup-project apps/api/MalaMin.Api/MalaMin.Api.csproj
```

Build and run the API:

```powershell
dotnet build apps/api/MalaMin.Api/MalaMin.Api.csproj
dotnet run --project apps/api/MalaMin.Api/MalaMin.Api.csproj
```

Local API URL:

```text
http://localhost:5000
```

Health checks:

```powershell
Invoke-RestMethod http://localhost:5000/
Invoke-RestMethod http://localhost:5000/api/health
Invoke-RestMethod http://localhost:5000/api/health/database
Invoke-RestMethod http://localhost:5000/api/health/model
```

## Local Frontend Setup

```powershell
cd apps/web
npm install
npm run build
npm run dev
```

Local frontend URL:

```text
http://localhost:3000
```

If port `3000` is busy, Next.js may use `3001`. The development API CORS policy allows both `localhost:3000` and `localhost:3001`.

## Demo Login

```text
Email: owner@demo.local
Password: Demo12345!
```

The development seeder creates the demo tenant and owner user when the API runs in Development.

## Main Routes

Frontend:

- `/` entry gateway for offices and visitors
- `/visitor` public property gallery
- `/login` office login
- `/dashboard` protected dashboard
- `/properties` protected property, media, image, tour room, and hotspot management
- `/subscription` protected current plan and usage page
- `/a/[tenantSlug]/[propertySlug]` public property page
- `/a/[tenantSlug]/[propertySlug]/tour` public 360 tour viewer

API:

- `POST /api/auth/login`
- `GET /api/auth/me`
- `GET /api/properties`
- `POST /api/media/upload`
- `GET /api/public/properties`
- `GET /api/public/properties/{tenantSlug}/{propertySlug}`
- `GET /api/public/properties/{tenantSlug}/{propertySlug}/tour`
- `GET /api/subscription/me`

See `docs/api-contracts.md` for the full contract list.

## Current MVP Features

- Tenant-scoped authentication and current tenant context
- Property CRUD with publish/unpublish and soft delete
- Local media upload metadata and static file serving in Development
- Property image linking and cover image selection
- 360 tour rooms using uploaded panorama media
- Navigate and Info hotspots
- Public property pages and public 360 tours
- Visitor gallery for published properties
- Daily aggregated property stats
- Manual plans, subscriptions, and property/tour limits
- Arabic RTL responsive frontend

## Known Limitations

- Local storage is filesystem-based for development. Production should use durable object storage or a persistent mounted volume.
- Frontend token storage uses `localStorage` for MVP speed. Production should move to a safer session and refresh strategy.
- Best 360 quality requires 2:1 equirectangular panorama images.
- Online payments, invoices, advanced analytics, maps, advanced search/filtering, and QR generation UI are not implemented.

## QA And Deployment

- Final QA checklist: `docs/qa-checklist.md`
- Deployment checklist: `docs/deployment-checklist.md`
