var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", () => Results.Text("Mala Min 360 API is running", "text/plain"));

app.MapGet("/api/health", () => Results.Json(new
{
    success = true,
    service = "Mala Min 360 API",
    status = "Healthy",
    timestamp = DateTimeOffset.UtcNow
}));

app.Run();
