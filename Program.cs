using System.Text;
using System.Threading.RateLimiting;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using Projeto_Integrador2.Endpoints;
using Projeto_Integrador2.Models;
using Projeto_Integrador2.Observability;
using Projeto_Integrador2.Persistence;
using Projeto_Integrador2.Services;
using Projeto_Integrador2.Validators;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

foreach (var jsonSource in builder.Configuration.Sources.OfType<JsonConfigurationSource>().ToList())
{
    builder.Configuration.Sources.Remove(jsonSource);
}
builder.Configuration.AddEnvironmentVariables();

var serilogLogger = SerilogBootstrap.CreateLogger();
Log.Logger = serilogLogger;
builder.Logging.ClearProviders();
builder.Logging.AddProvider(new SerilogLoggerProvider(serilogLogger));

if (!builder.Environment.IsEnvironment("Testing"))
{
    var connectionString = builder.Configuration["SUPABASE_CONNECTION_STRING"]
        ?? builder.Configuration["DATABASE_URL"]
        ?? throw new InvalidOperationException(
            "Connection string não configurada. Defina SUPABASE_CONNECTION_STRING.");

    builder.Services.AddDbContext<ReservationDbContext>(options =>
        options.UseNpgsql(NormalizeConnectionString(connectionString))
            .UseSnakeCaseNamingConvention());
}

var jwtSecret = builder.Configuration["JWT_SECRET_KEY"]
    ?? throw new InvalidOperationException(
        "JWT_SECRET_KEY não configurada. Use uma chave aleatória com pelo menos 32 caracteres.");

if (Encoding.UTF8.GetByteCount(jwtSecret) < 32)
    throw new InvalidOperationException("JWT_SECRET_KEY precisa ter pelo menos 32 bytes.");

var jwtSettings = new JwtSettings { SecretKey = jwtSecret };
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

builder.Services.AddSingleton(jwtSettings);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

var defaultAllowedOrigins = builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing")
    ? "http://localhost:5000,http://localhost:5173,http://localhost:3000,http://127.0.0.1:5000,http://127.0.0.1:5173"
    : "https://projeto-integrador2-latest.onrender.com";

var allowedOrigins = (builder.Configuration["CORS_ALLOWED_ORIGINS"] ?? defaultAllowedOrigins)
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

if (allowedOrigins.Length == 0 || allowedOrigins.Any(origin => origin == "*"))
{
    throw new InvalidOperationException(
        "CORS_ALLOWED_ORIGINS deve listar origens explícitas e não pode conter wildcard '*'.");
}

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
              .WithHeaders("Authorization", "Content-Type")
              .WithMethods("GET", "POST", "PUT", "OPTIONS")
              .AllowCredentials()));

var windowSeconds = ReadPositiveInt(builder.Configuration["RATE_LIMIT_WINDOW_SECONDS"], 60);
var authPermitLimit = ReadPositiveInt(builder.Configuration["RATE_LIMIT_AUTH_PERMITS"], 20);
var apiPermitLimit = ReadPositiveInt(builder.Configuration["RATE_LIMIT_API_PERMITS"], 120);

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.ContentType = "application/json; charset=utf-8";
        await context.HttpContext.Response.WriteAsJsonAsync(
            new { error = "Muitas requisições. Tente novamente em instantes." },
            cancellationToken: token);
    };

    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ResolveClientPartitionKey(httpContext),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = authPermitLimit,
                Window = TimeSpan.FromSeconds(windowSeconds),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.AddPolicy("api", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ResolveClientPartitionKey(httpContext),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = apiPermitLimit,
                Window = TimeSpan.FromSeconds(windowSeconds),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Projeto-Integrador2 API — Sistema de Reserva de Salas",
        Version = "v1",
        Description = "API REST para solicitação, aprovação, rejeição e cancelamento de reservas de salas."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe o token JWT no formato: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddSingleton<IValidator<LoginRequest>, LoginRequestValidator>();
builder.Services.AddSingleton<IValidator<RegisterRequest>, RegisterRequestValidator>();
builder.Services.AddSingleton<IValidator<CreateReservationRequest>, CreateReservationRequestValidator>();
builder.Services.AddSingleton<IValidator<UpsertRoomRequest>, UpsertRoomRequestValidator>();
builder.Services.AddSingleton<IValidator<UpsertResourceRequest>, UpsertResourceRequestValidator>();
builder.Services.AddSingleton<IValidator<UpsertUserRequest>, UpsertUserRequestValidator>();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<RoomService>();
builder.Services.AddScoped<ResourceService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ReservationAppService>();

var app = builder.Build();
app.Lifetime.ApplicationStopped.Register(Log.CloseAndFlush);

var cloudPort = builder.Configuration["PORT"];
if (!string.IsNullOrWhiteSpace(cloudPort) && !app.Environment.IsEnvironment("Testing"))
{
    app.Urls.Add($"http://0.0.0.0:{cloudPort}");
}

app.UseStructuredRequestLogging();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Reserva de Salas API v1");
});
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapApplicationEndpoints();

app.Run();

static int ReadPositiveInt(string? raw, int fallback) =>
    int.TryParse(raw, out var parsed) && parsed > 0 ? parsed : fallback;

static string ResolveClientPartitionKey(HttpContext context) =>
    context.Connection.RemoteIpAddress?.ToString() ?? "local-client";

static string NormalizeConnectionString(string value)
{
    if (!value.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase) &&
        !value.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
        return value;

    var uri = new Uri(value);
    var separator = uri.UserInfo.IndexOf(':');
    if (separator < 0)
        throw new InvalidOperationException("A URI do Supabase precisa conter usuário e senha.");

    return new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.IsDefaultPort ? 5432 : uri.Port,
        Database = uri.AbsolutePath.Trim('/'),
        Username = Uri.UnescapeDataString(uri.UserInfo[..separator]),
        Password = Uri.UnescapeDataString(uri.UserInfo[(separator + 1)..]),
        SslMode = SslMode.Require
    }.ConnectionString;
}

public partial class Program;
