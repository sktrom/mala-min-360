using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MalaMin.Api.Application.Auth;
using MalaMin.Api.Application.Common;
using MalaMin.Api.Application.Properties;
using MalaMin.Api.Application.Public;
using MalaMin.Api.Application.Tenants;
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
builder.Services.AddScoped<PropertyService>();
builder.Services.AddScoped<PublicPropertyService>();
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, TenantContext>();

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
    entities = new[] { "Tenants", "Users", "Properties" },
    timestamp = DateTimeOffset.UtcNow
}));

app.MapGet("/api/public/properties/{tenantSlug}/{propertySlug}", async (
    string tenantSlug,
    string propertySlug,
    PublicPropertyService publicPropertyService,
    CancellationToken cancellationToken) =>
{
    var property = await publicPropertyService.GetPublishedPropertyAsync(
        tenantSlug,
        propertySlug,
        cancellationToken);

    if (property is null)
    {
        return Results.Json(new
        {
            success = false,
            error = new
            {
                code = "PUBLIC_PROPERTY_NOT_FOUND",
                message = "Property was not found or is not published."
            }
        }, statusCode: StatusCodes.Status404NotFound);
    }

    return Results.Json(new
    {
        success = true,
        data = property
    });
});

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

app.MapGet("/api/tenant/me", async (
    ITenantContext tenantContext,
    AppDbContext db,
    CancellationToken cancellationToken) =>
{
    var tenant = await db.Tenants
        .SingleOrDefaultAsync(existingTenant => existingTenant.Id == tenantContext.TenantId, cancellationToken);

    if (tenant is null)
    {
        return Results.Json(new
        {
            success = false,
            error = new
            {
                code = "TENANT_NOT_FOUND",
                message = "Tenant was not found."
            }
        }, statusCode: StatusCodes.Status404NotFound);
    }

    return Results.Json(new
    {
        success = true,
        data = new TenantResponse(
            tenant.Id,
            tenant.Name,
            tenant.Slug,
            tenant.Phone,
            tenant.WhatsAppNumber,
            tenant.LogoUrl,
            tenant.Address,
            tenant.City,
            tenant.Status)
    });
}).RequireAuthorization();

app.MapGet("/api/properties", async (
    PropertyService propertyService,
    CancellationToken cancellationToken) =>
{
    var properties = await propertyService.ListAsync(cancellationToken);

    return Results.Json(new
    {
        success = true,
        data = properties
    });
}).RequireAuthorization();

app.MapPost("/api/properties", async (
    CreatePropertyRequest request,
    PropertyService propertyService,
    CancellationToken cancellationToken) =>
{
    var result = await propertyService.CreateAsync(request, cancellationToken);

    if (!result.IsSuccess)
    {
        return CreateErrorResult(result);
    }

    return Results.Json(new
    {
        success = true,
        data = result.Data
    }, statusCode: StatusCodes.Status201Created);
}).RequireAuthorization();

app.MapGet("/api/properties/{id:guid}", async (
    Guid id,
    PropertyService propertyService,
    CancellationToken cancellationToken) =>
{
    var property = await propertyService.GetAsync(id, cancellationToken);

    if (property is null)
    {
        return Results.Json(new
        {
            success = false,
            error = new
            {
                code = "PROPERTY_NOT_FOUND",
                message = "Property was not found."
            }
        }, statusCode: StatusCodes.Status404NotFound);
    }

    return Results.Json(new
    {
        success = true,
        data = property
    });
}).RequireAuthorization();

app.MapPut("/api/properties/{id:guid}", async (
    Guid id,
    UpdatePropertyRequest request,
    PropertyService propertyService,
    CancellationToken cancellationToken) =>
{
    var result = await propertyService.UpdateAsync(id, request, cancellationToken);

    if (!result.IsSuccess)
    {
        return CreateErrorResult(result);
    }

    return Results.Json(new
    {
        success = true,
        data = result.Data
    });
}).RequireAuthorization();

app.MapDelete("/api/properties/{id:guid}", async (
    Guid id,
    PropertyService propertyService,
    CancellationToken cancellationToken) =>
{
    var deleted = await propertyService.SoftDeleteAsync(id, cancellationToken);

    if (!deleted)
    {
        return Results.Json(new
        {
            success = false,
            error = new
            {
                code = "PROPERTY_NOT_FOUND",
                message = "Property was not found."
            }
        }, statusCode: StatusCodes.Status404NotFound);
    }

    return Results.Json(new
    {
        success = true
    });
}).RequireAuthorization();

app.MapPatch("/api/properties/{id:guid}/publish", async (
    Guid id,
    PropertyService propertyService,
    CancellationToken cancellationToken) =>
{
    var result = await propertyService.PublishAsync(id, cancellationToken);

    if (!result.IsSuccess)
    {
        return CreateErrorResult(result);
    }

    return Results.Json(new
    {
        success = true,
        data = result.Data
    });
}).RequireAuthorization();

app.MapPatch("/api/properties/{id:guid}/unpublish", async (
    Guid id,
    PropertyService propertyService,
    CancellationToken cancellationToken) =>
{
    var result = await propertyService.UnpublishAsync(id, cancellationToken);

    if (!result.IsSuccess)
    {
        return CreateErrorResult(result);
    }

    return Results.Json(new
    {
        success = true,
        data = result.Data
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

    app.MapGet("/api/dev/tenant-context", (ITenantContext tenantContext) => Results.Json(new
    {
        success = true,
        userId = tenantContext.UserId,
        tenantId = tenantContext.TenantId,
        role = tenantContext.UserRole,
        isAuthenticated = tenantContext.IsAuthenticated
    })).RequireAuthorization();
}

app.Run();

static IResult CreateErrorResult<T>(PropertyServiceResult<T> result)
{
    var statusCode = result.ErrorCode == "PROPERTY_NOT_FOUND"
        ? StatusCodes.Status404NotFound
        : StatusCodes.Status400BadRequest;

    return Results.Json(new
    {
        success = false,
        error = new
        {
            code = result.ErrorCode,
            message = result.ErrorMessage
        }
    }, statusCode: statusCode);
}
