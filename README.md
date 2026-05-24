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
