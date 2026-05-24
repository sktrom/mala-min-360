using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MalaMin.Api.Application.Auth;
using MalaMin.Api.Domain.Entities;
using MalaMin.Api.Infrastructure.Auth;
using MalaMin.Api.Infrastructure.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddSingleton<JwtTokenService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var issuer = builder.Configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("JWT issuer is not configured.");
        var audience = builder.Configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("JWT audience is not configured.");
        var signingKey = builder.Configuration["Jwt:SigningKey"]
            ?? throw new InvalidOperationException("JWT signing key is not configured.");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = JwtRegisteredClaimNames.Sub,
            RoleClaimType = "role"
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    await DevelopmentSeeder.SeedAsync(scope.ServiceProvider);
}

app.UseAuthentication();
app.UseAuthorization();

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

app.MapGet("/api/health/model", () => Results.Json(new
{
    success = true,
    entities = new[] { "Tenants", "Users" },
    timestamp = DateTimeOffset.UtcNow
}));

app.MapPost("/api/auth/login", async (
    LoginRequest request,
    AuthService authService,
    CancellationToken cancellationToken) =>
{
    var authResponse = await authService.LoginAsync(request, cancellationToken);

    if (authResponse is null)
    {
        return Results.Json(new
        {
            success = false,
            error = new
            {
                code = "INVALID_CREDENTIALS",
                message = "Invalid email or password."
            }
        }, statusCode: StatusCodes.Status401Unauthorized);
    }

    return Results.Json(new
    {
        success = true,
        data = authResponse
    });
});

app.MapGet("/api/auth/me", async (
    ClaimsPrincipal claimsPrincipal,
    AppDbContext db,
    CancellationToken cancellationToken) =>
{
    var subject = claimsPrincipal.FindFirstValue(JwtRegisteredClaimNames.Sub);

    if (!Guid.TryParse(subject, out var userId))
    {
        return Results.Unauthorized();
    }

    var user = await db.Users
        .Include(appUser => appUser.Tenant)
        .SingleOrDefaultAsync(appUser => appUser.Id == userId, cancellationToken);

    if (user is null || !user.IsActive)
    {
        return Results.Unauthorized();
    }

    return Results.Json(new
    {
        success = true,
        data = AuthService.CreateCurrentUserResponse(user)
    });
}).RequireAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapGet("/api/dev/seed-info", () => Results.Json(new
    {
        success = true,
        tenantSlug = DevelopmentSeeder.DemoTenantSlug,
        email = DevelopmentSeeder.DemoUserEmail,
        password = DevelopmentSeeder.DemoUserPassword
    }));
}

app.Run();
