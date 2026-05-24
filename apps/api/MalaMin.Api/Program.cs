using MalaMin.Api.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.MapGet("/", () => Results.Text("Mala Min 360 API is running", "text/plain"));

app.MapGet("/api/health", () => Results.Json(new
{
    success = true,
    service = "Mala Min 360 API",
    status = "Healthy",
    timestamp = DateTimeOffset.UtcNow
}));

app.MapGet("/api/health/database", async (AppDbContext db) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync();

        if (!canConnect)
        {
            return Results.Json(new
            {
                success = false,
                database = "PostgreSQL",
                status = "Disconnected",
                error = "Database connection check failed."
            }, statusCode: StatusCodes.Status500InternalServerError);
        }

        return Results.Json(new
        {
            success = true,
            database = "PostgreSQL",
            status = "Connected",
            timestamp = DateTimeOffset.UtcNow
        });
    }
    catch (Exception ex)
    {
        return Results.Json(new
        {
            success = false,
            database = "PostgreSQL",
            status = "Disconnected",
            error = ex.Message
        }, statusCode: StatusCodes.Status500InternalServerError);
    }
});

app.Run();
