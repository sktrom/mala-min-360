# Deployment Checklist

This project is prepared for deployment review, but no provider-specific production deployment is configured yet.

## Required Production Environment Variables

Backend:

- `ASPNETCORE_ENVIRONMENT=Production`
- `ConnectionStrings__DefaultConnection`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__SigningKey`
- `Jwt__AccessTokenMinutes`
- storage configuration for durable uploads if local filesystem is not used
- production CORS allowed origins

Frontend:

- `NEXT_PUBLIC_API_BASE_URL`

Do not reuse development placeholder secrets in production.

## Database

- Provision a production PostgreSQL database.
- Create a production database user with least required privileges.
- Configure `ConnectionStrings__DefaultConnection`.
- Apply migrations manually:

```powershell
dotnet ef database update --project apps/api/MalaMin.Api/MalaMin.Api.csproj --startup-project apps/api/MalaMin.Api/MalaMin.Api.csproj
```

- Confirm `__EFMigrationsHistory` contains the latest migration.
- Confirm tables exist for tenants, users, properties, media, tours, stats, plans, and subscriptions.

## Storage

Current development storage writes to `storage/uploads`.

Production must choose one durable strategy:

- object storage such as S3/R2-compatible storage, or
- a persistent mounted volume if deploying the API as a long-running service.

Uploaded files should not be stored in ephemeral container filesystems unless data loss is acceptable.

## CORS

Development allows:

- `http://localhost:3000`
- `http://127.0.0.1:3000`
- `http://localhost:3001`
- `http://127.0.0.1:3001`

Production should allow only the real frontend domain or domains. Uploaded media responses must also include valid CORS headers because the 360 viewer fetches panorama textures.

## JWT

- Use a strong production signing key.
- Keep issuer and audience stable.
- Rotate keys intentionally.
- Do not commit JWT secrets.
- Review token lifetime before launch.

## Build Commands

Backend:

```powershell
dotnet build apps/api/MalaMin.Api/MalaMin.Api.csproj
```

Frontend:

```powershell
cd apps/web
npm install
npm run build
```

## Pre-Deployment Checklist

- Backend build succeeds.
- Frontend build succeeds.
- PostgreSQL is reachable from the API host.
- EF migrations apply successfully.
- Production environment variables are configured outside source control.
- Production CORS origins are explicit.
- Uploaded media storage is durable.
- `.env`, `.env.local`, uploaded files, `node_modules`, `bin`, and `obj` are not committed.
- Demo credentials are not treated as production credentials.
- Any production seed data is intentional.

## Post-Deployment Smoke Tests

Backend:

- `GET /`
- `GET /api/health`
- `GET /api/health/database`
- `GET /api/health/model`
- `POST /api/auth/login`
- `GET /api/auth/me`

Frontend:

- `/`
- `/visitor`
- `/login`
- `/dashboard`
- `/properties`
- `/subscription`
- `/a/{tenantSlug}/{propertySlug}`
- `/a/{tenantSlug}/{propertySlug}/tour`

Media and 360:

- Upload a normal image.
- Confirm `/uploads/...` or production media URL loads from the frontend origin.
- Upload a Panorama360 image.
- Confirm the public 360 viewer loads, drags, zooms, and displays hotspots.

## Known Production Limitations

- No online payment provider is implemented.
- No invoice workflow is implemented.
- No advanced analytics or visitor identity tracking is implemented.
- No maps or advanced property search are implemented.
- MVP frontend auth uses `localStorage`; a hardened session strategy should replace it before high-risk production use.
