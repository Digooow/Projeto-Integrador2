using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Projeto_Integrador2.Endpoints;
using Projeto_Integrador2.Persistence;
using Projeto_Integrador2.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.Sources.Clear();
builder.Configuration.AddEnvironmentVariables();

var connectionString = Environment.GetEnvironmentVariable("SUPABASE_CONNECTION_STRING")
    ?? Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? throw new InvalidOperationException(
        "Connection string não configurada. Defina SUPABASE_CONNECTION_STRING.");

builder.Services.AddDbContext<ReservationDbContext>(options =>
    options.UseNpgsql(NormalizeConnectionString(connectionString))
        .UseSnakeCaseNamingConvention());

var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
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
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<RoomService>();
builder.Services.AddScoped<ResourceService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ReservationAppService>();

var app = builder.Build();
app.Urls.Add($"http://0.0.0.0:{Environment.GetEnvironmentVariable("PORT") ?? "5000"}");

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapApplicationEndpoints();
app.Run();

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
