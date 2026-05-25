# Mala Min 360

## Local API Database

Start PostgreSQL:

```powershell
docker compose up -d postgres
```

Apply EF Core migrations:

```powershell
dotnet ef database update --project apps/api/MalaMin.Api/MalaMin.Api.csproj --startup-project apps/api/MalaMin.Api/MalaMin.Api.csproj
```

Run the API:

```powershell
dotnet run --project apps/api/MalaMin.Api/MalaMin.Api.csproj
```

Verify the database connection:

```powershell
Invoke-RestMethod http://localhost:5000/api/health/database
```

## Local Web Frontend

Install dependencies:

```powershell
cd apps/web
copy .env.example .env.local
npm install
```

The backend API must be running at `http://localhost:5000`, or set `NEXT_PUBLIC_API_BASE_URL` in `apps/web/.env.local`.

Run the frontend:

```powershell
npm run dev
```

Build the frontend:

```powershell
npm run build
```

Local URL:

```text
http://localhost:3000
```

The `/properties` frontend page uses the backend property API and requires login.
