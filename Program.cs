<<<<<<< HEAD
﻿using Microsoft.EntityFrameworkCore;
using Npgsql;
=======
﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
>>>>>>> e46e0c31d60afb051716eeae43498b37a6584a53
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Projeto_Integrador2.Domain;
using Projeto_Integrador2.Persistence;
using Projeto_Integrador2.Security;

// Usa WebApplication.CreateBuilder para ter acesso aos métodos do ASP.NET Core
var builder = WebApplication.CreateBuilder(args);

// Remove todas as fontes de configuração padrão (inclusive appsettings.json) – isso elimina o erro de inotify
builder.Configuration.Sources.Clear();
builder.Configuration.AddEnvironmentVariables();

// Lê a connection string da variável de ambiente
var rawConnectionString = Environment.GetEnvironmentVariable("SUPABASE_CONNECTION_STRING")
    ?? Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? throw new InvalidOperationException("Connection string não configurada. Defina SUPABASE_CONNECTION_STRING.");
var connectionString = NormalizeConnectionString(rawConnectionString);

builder.Services.AddDbContext<ReservationDbContext>(options =>
    options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());

var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
    ?? throw new InvalidOperationException("JWT_SECRET_KEY não configurada. Use uma chave aleatória com pelo menos 32 caracteres.");

if (Encoding.UTF8.GetByteCount(jwtSecret) < 32)
    throw new InvalidOperationException("JWT_SECRET_KEY precisa ter pelo menos 32 bytes.");

const string jwtIssuer = "projeto-integrador2";
const string jwtAudience = "projeto-integrador2-users";
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// Configura a URL para escutar em todas as interfaces e na porta fornecida pelo Render (variável PORT)
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://0.0.0.0:{port}");

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

var frontendPath = Path.Combine(app.Environment.ContentRootPath, "frontend", "reserva-salas.html");
app.MapGet("/", () => Results.File(frontendPath, "text/html; charset=utf-8"));
app.MapGet("/reserva-salas.html", () => Results.File(frontendPath, "text/html; charset=utf-8"));

app.MapGet("/health", async (ReservationDbContext db, CancellationToken cancellationToken) =>
{
    var databaseConnected = await db.Database.CanConnectAsync(cancellationToken);
    return databaseConnected
        ? Results.Ok(new { status = "ok", database = "connected", timestamp = DateTime.UtcNow })
        : Results.Json(new { status = "degraded", database = "disconnected", timestamp = DateTime.UtcNow }, statusCode: StatusCodes.Status503ServiceUnavailable);
});

app.MapPost("/auth/login", async (LoginRequest input, ReservationDbContext db, CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(input.Email) || string.IsNullOrWhiteSpace(input.Password))
        return Results.BadRequest(new { error = "Informe e-mail e senha." });

    var user = await db.Users.SingleOrDefaultAsync(u => u.Email == input.Email && u.Active, cancellationToken);
    if (user is null || user.PasswordHash is null || !PasswordHasher.Verify(input.Password, user.PasswordHash))
        return Results.Unauthorized();

    var expiresAt = DateTime.UtcNow.AddHours(8);
    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id),
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Name, user.Name),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role.ToString())
    };
    var token = new JwtSecurityToken(
        issuer: jwtIssuer,
        audience: jwtAudience,
        claims: claims,
        expires: expiresAt,
        signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

    return Results.Ok(new LoginResponse(
        new JwtSecurityTokenHandler().WriteToken(token),
        expiresAt,
        new UserResponse(user.Id, user.Name, user.Email, user.Role.ToString(), user.Active, user.Floors.ToArray())));
});

// ===================== ROOMS =====================

app.MapGet("/api/rooms", async (ReservationDbContext db, ClaimsPrincipal principal, bool? includeInactive, CancellationToken cancellationToken) =>
{
    if (includeInactive == true && !principal.IsInRole(UserRole.Administrator.ToString()))
        return Results.Forbid();

    var query = db.Rooms
        .AsNoTracking()
        .Include(room => room.Resources)
            .ThenInclude(link => link.Resource)
        .AsQueryable();

    if (includeInactive != true)
        query = query.Where(room => room.Active);

    var rooms = await query
        .OrderBy(room => room.Floor).ThenBy(room => room.Name)
        .Select(room => new RoomResponse(
            room.Id,
            room.Name,
            room.Floor,
            room.Capacity,
            room.Description,
            room.Active,
            room.Resources.Select(link => link.Resource.Id).ToArray(),
            room.Resources.Select(link => link.Resource.Name).ToArray()))
        .ToListAsync(cancellationToken);

    return Results.Ok(rooms);
});

app.MapPost("/api/rooms", async (UpsertRoomRequest input, ReservationDbContext db, CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(input.Id) || string.IsNullOrWhiteSpace(input.Name) || string.IsNullOrWhiteSpace(input.Floor) || input.Capacity <= 0)
        return Results.BadRequest(new { error = "Informe id, nome, andar e capacidade (> 0)." });

    if (await db.Rooms.AnyAsync(r => r.Id == input.Id, cancellationToken))
        return Results.Conflict(new { error = "Já existe uma sala com esse identificador." });

    var room = new RoomEntity
    {
        Id = input.Id,
        Name = input.Name,
        Floor = input.Floor,
        Capacity = input.Capacity,
        Description = input.Description ?? "",
        Active = true
    };

    await SyncRoomResourcesAsync(room, input.ResourceIds, db, cancellationToken);

    db.Rooms.Add(room);
    await db.SaveChangesAsync(cancellationToken);

    return Results.Created($"/api/rooms/{room.Id}", new { room.Id });
}).RequireAuthorization();

app.MapPut("/api/rooms/{id}", async (string id, UpsertRoomRequest input, ReservationDbContext db, CancellationToken cancellationToken) =>
{
    var room = await db.Rooms.Include(r => r.Resources).SingleOrDefaultAsync(r => r.Id == id, cancellationToken);
    if (room is null)
        return Results.NotFound();

    if (string.IsNullOrWhiteSpace(input.Name) || string.IsNullOrWhiteSpace(input.Floor) || input.Capacity <= 0)
        return Results.BadRequest(new { error = "Informe nome, andar e capacidade (> 0)." });

    room.Name = input.Name;
    room.Floor = input.Floor;
    room.Capacity = input.Capacity;
    room.Description = input.Description ?? "";

    room.Resources.Clear();
    await SyncRoomResourcesAsync(room, input.ResourceIds, db, cancellationToken);

    await db.SaveChangesAsync(cancellationToken);
    return Results.Ok(new { room.Id });
}).RequireAuthorization();

app.MapPost("/api/rooms/{id}/toggle-active", async (string id, ReservationDbContext db, CancellationToken cancellationToken) =>
{
    var room = await db.Rooms.SingleOrDefaultAsync(r => r.Id == id, cancellationToken);
    if (room is null)
        return Results.NotFound();

    room.Active = !room.Active;
    await db.SaveChangesAsync(cancellationToken);
    return Results.Ok(new { room.Id, room.Active });
}).RequireAuthorization(new AuthorizeAttribute { Roles = UserRole.Administrator.ToString() });

// ===================== RESOURCES =====================

app.MapGet("/api/resources", async (ReservationDbContext db, CancellationToken cancellationToken) =>
{
    var resources = await db.Resources
        .AsNoTracking()
        .OrderBy(r => r.Name)
        .Select(r => new ResourceResponse(r.Id, r.Name))
        .ToListAsync(cancellationToken);

    return Results.Ok(resources);
});

app.MapPost("/api/resources", async (UpsertResourceRequest input, ReservationDbContext db, CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(input.Id) || string.IsNullOrWhiteSpace(input.Name))
        return Results.BadRequest(new { error = "Informe id e nome do recurso." });

    if (await db.Resources.AnyAsync(r => r.Id == input.Id, cancellationToken))
        return Results.Conflict(new { error = "Já existe um recurso com esse identificador." });

    db.Resources.Add(new ResourceEntity { Id = input.Id, Name = input.Name });
    await db.SaveChangesAsync(cancellationToken);

    return Results.Created($"/api/resources/{input.Id}", new { input.Id });
}).RequireAuthorization(new AuthorizeAttribute { Roles = UserRole.Administrator.ToString() });

// ===================== USERS =====================

app.MapGet("/api/users", async (ReservationDbContext db, CancellationToken cancellationToken) =>
{
    var users = await db.Users
        .AsNoTracking()
        .OrderBy(u => u.Name)
        .Select(u => new UserResponse(u.Id, u.Name, u.Email, u.Role.ToString(), u.Active, u.Floors.ToArray()))
        .ToListAsync(cancellationToken);

    return Results.Ok(users);
}).RequireAuthorization();

app.MapPost("/api/users", async (UpsertUserRequest input, ReservationDbContext db, CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(input.Id) || string.IsNullOrWhiteSpace(input.Name) || string.IsNullOrWhiteSpace(input.Email))
        return Results.BadRequest(new { error = "Informe id, nome e e-mail." });

    if (!Enum.TryParse<UserRole>(input.Role, ignoreCase: true, out var role))
        return Results.BadRequest(new { error = $"Papel inválido: {input.Role}." });

    if (await db.Users.AnyAsync(u => u.Id == input.Id || u.Email == input.Email, cancellationToken))
        return Results.Conflict(new { error = "Já existe um usuário com esse identificador ou e-mail." });

    if (string.IsNullOrWhiteSpace(input.Password))
        return Results.BadRequest(new { error = "Informe uma senha." });

    db.Users.Add(new UserEntity
    {
        Id = input.Id,
        Name = input.Name,
        Email = input.Email,
        PasswordHash = PasswordHasher.Hash(input.Password),
        Role = role,
        Active = true,
        Floors = role == UserRole.Coordinator ? (input.Floors ?? []).ToList() : []
    });
    await db.SaveChangesAsync(cancellationToken);

    return Results.Created($"/api/users/{input.Id}", new { input.Id });
}).RequireAuthorization(new AuthorizeAttribute { Roles = UserRole.Administrator.ToString() });

app.MapPut("/api/users/{id}", async (string id, UpsertUserRequest input, ReservationDbContext db, CancellationToken cancellationToken) =>
{
    var user = await db.Users.SingleOrDefaultAsync(u => u.Id == id, cancellationToken);
    if (user is null)
        return Results.NotFound();

    if (string.IsNullOrWhiteSpace(input.Name) || string.IsNullOrWhiteSpace(input.Email))
        return Results.BadRequest(new { error = "Informe nome e e-mail." });

    if (!Enum.TryParse<UserRole>(input.Role, ignoreCase: true, out var role))
        return Results.BadRequest(new { error = $"Papel inválido: {input.Role}." });

    user.Name = input.Name;
    user.Email = input.Email;
    user.Role = role;
    user.Floors = role == UserRole.Coordinator ? (input.Floors ?? []).ToList() : [];

    await db.SaveChangesAsync(cancellationToken);
    return Results.Ok(new { user.Id });
}).RequireAuthorization(new AuthorizeAttribute { Roles = UserRole.Administrator.ToString() });

app.MapPost("/api/users/{id}/toggle-active", async (string id, ReservationDbContext db, CancellationToken cancellationToken) =>
{
    var user = await db.Users.SingleOrDefaultAsync(u => u.Id == id, cancellationToken);
    if (user is null)
        return Results.NotFound();

    user.Active = !user.Active;
    await db.SaveChangesAsync(cancellationToken);
    return Results.Ok(new { user.Id, user.Active });
}).RequireAuthorization(new AuthorizeAttribute { Roles = UserRole.Administrator.ToString() });

// ===================== RESERVATIONS =====================
// Each occurrence (a single date+time slot) is stored as its own reservation
// row; a recurring request creates several rows that share the same
// SeriesId. That is what lets the UI approve/reject one date of a series
// independently of the others.

app.MapGet("/api/reservations", async (ReservationDbContext db, ClaimsPrincipal principal, ReservationStatus? status, int? page, int? pageSize, CancellationToken cancellationToken) =>
{
    if (status != ReservationStatus.Approved && principal.Identity?.IsAuthenticated != true)
        return Results.Unauthorized();

    var currentPage = Math.Max(page ?? 1, 1);
    var currentPageSize = Math.Clamp(pageSize ?? 20, 1, 100);
    var query = db.Reservations
        .AsNoTracking()
        .Include(r => r.Occurrences)
        .AsQueryable();

    if (status is not null)
        query = query.Where(r => r.Status == status);

    var total = await query.CountAsync(cancellationToken);
    var reservations = await query
        .OrderBy(r => r.CreatedAt)
        .ThenBy(r => r.Id)
        .Skip((currentPage - 1) * currentPageSize)
        .Take(currentPageSize)
        .Select(r => new ReservationResponse(
            r.Id,
            r.SeriesId,
            r.RoomId,
            r.RequesterId,
            r.Title,
            r.Responsavel,
            r.Attendees,
            r.Status.ToString(),
            r.CreatedAt,
            r.DecidedBy,
            r.DecidedAt,
            r.Occurrences.Select(o => new OccurrenceResponse(o.StartsAt, o.EndsAt)).Single()))
        .ToListAsync(cancellationToken);

    return Results.Ok(new
    {
        data = reservations,
        pagination = new
        {
            page = currentPage,
            pageSize = currentPageSize,
            total,
            totalPages = (int)Math.Ceiling(total / (double)currentPageSize)
        }
    });
});
app.MapPost("/api/reservations", async (CreateReservationRequest input, ClaimsPrincipal principal, ReservationDbContext db, CancellationToken cancellationToken) =>
{
    var authenticatedUserId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
    if (authenticatedUserId is null || authenticatedUserId != input.RequesterId)
        return Results.Forbid();
    var room = await db.Rooms
        .AsNoTracking()
        .SingleOrDefaultAsync(r => r.Id == input.RoomId && r.Active, cancellationToken);

    if (room is null)
        return Results.NotFound(new { error = "Sala não encontrada." });

    var requesterExists = await db.Users
        .AnyAsync(u => u.Id == input.RequesterId && u.Active, cancellationToken);

    if (!requesterExists)
        return Results.BadRequest(new { error = "Usuário solicitante não encontrado ou inativo." });

    if (input.Attendees <= 0)
        return Results.BadRequest(new { error = "Informe ao menos 1 participante." });

    if (input.Attendees > room.Capacity)
        return Results.BadRequest(new { error = $"A sala '{room.Name}' tem capacidade para {room.Capacity} pessoas." });

    if (input.End <= input.Start)
        return Results.BadRequest(new { error = "O horário final precisa ser depois do inicial." });

    IReadOnlyList<(DateTime Start, DateTime End)> occurrences;
    try
    {
        occurrences = ExpandOccurrences(input.Start, input.End, input.Recurrence);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }

    if (occurrences.Count == 0)
        return Results.BadRequest(new { error = "Nenhuma data cai nos dias selecionados dentro do período informado." });

    var seriesId = occurrences.Count > 1 ? Guid.NewGuid() : (Guid?)null;
    var now = DateTime.UtcNow;
    var created = new List<ReservationEntity>();

    foreach (var occurrence in occurrences)
    {
        var entity = new ReservationEntity
        {
            Id = Guid.NewGuid(),
            RequesterId = input.RequesterId,
            RoomId = input.RoomId,
            Title = input.Title,
            Responsavel = string.IsNullOrWhiteSpace(input.Responsavel) ? input.RequesterId : input.Responsavel,
            Attendees = input.Attendees,
            SeriesId = seriesId,
            CreatedAt = now
        };
        entity.Occurrences.Add(new ReservationOccurrenceEntity
        {
            Id = Guid.NewGuid(),
            StartsAt = occurrence.Start,
            EndsAt = occurrence.End
        });
        db.Reservations.Add(entity);
        created.Add(entity);
    }

    await db.SaveChangesAsync(cancellationToken);

    return Results.Created($"/api/reservations/{created[0].Id}", new
    {
        seriesId,
        count = created.Count,
        ids = created.Select(r => r.Id)
    });
});

app.MapPost("/api/reservations/{id:guid}/approve", async (Guid id, bool? force, ClaimsPrincipal principal, ReservationDbContext db, CancellationToken cancellationToken) =>
{
    var deciderId = principal.FindFirstValue(ClaimTypes.NameIdentifier)!;

    var reservation = await db.Reservations
        .Include(r => r.Occurrences)
        .SingleOrDefaultAsync(r => r.Id == id, cancellationToken);

    if (reservation is null)
        return Results.NotFound();

    if (reservation.Status != ReservationStatus.Pending)
        return Results.BadRequest(new { error = "Este pedido já foi decidido." });

    var occurrence = reservation.Occurrences.Single();

    if (force != true)
    {
        var conflicts = await db.Reservations
            .AsNoTracking()
            .Include(r => r.Occurrences)
            .Where(r => r.Id != id && r.RoomId == reservation.RoomId && r.Status == ReservationStatus.Approved)
            .Where(r => r.Occurrences.Any(o => o.StartsAt < occurrence.EndsAt && occurrence.StartsAt < o.EndsAt))
            .Select(r => new ConflictResponse(r.Id, r.Title, r.Responsavel, r.Occurrences.Select(o => o.StartsAt).Single(), r.Occurrences.Select(o => o.EndsAt).Single()))
            .ToListAsync(cancellationToken);

        if (conflicts.Count > 0)
            return Results.Conflict(new { error = "Já existe uma reserva aprovada para esse horário e sala.", conflicts });
    }

    reservation.Status = ReservationStatus.Approved;
    reservation.DecidedBy = deciderId;
    reservation.DecidedAt = DateTime.UtcNow;

    await db.SaveChangesAsync(cancellationToken);

    return Results.Ok(new { reservation.Id, status = reservation.Status.ToString() });
}).RequireAuthorization(new AuthorizeAttribute { Roles = $"{UserRole.Coordinator},{UserRole.Administrator}" });

app.MapPost("/api/reservations/{id:guid}/reject", async (Guid id, ClaimsPrincipal principal, ReservationDbContext db, CancellationToken cancellationToken) =>
{
    var reservation = await db.Reservations.SingleOrDefaultAsync(r => r.Id == id, cancellationToken);
    if (reservation is null)
        return Results.NotFound();

    if (reservation.Status != ReservationStatus.Pending)
        return Results.BadRequest(new { error = "Este pedido já foi decidido." });

    reservation.Status = ReservationStatus.Rejected;
    reservation.DecidedBy = principal.FindFirstValue(ClaimTypes.NameIdentifier);
    reservation.DecidedAt = DateTime.UtcNow;

    await db.SaveChangesAsync(cancellationToken);
    return Results.Ok(new { reservation.Id, status = reservation.Status.ToString() });
}).RequireAuthorization(new AuthorizeAttribute { Roles = $"{UserRole.Coordinator},{UserRole.Administrator}" });

app.MapPost("/api/reservations/{id:guid}/cancel", async (Guid id, ClaimsPrincipal principal, ReservationDbContext db, CancellationToken cancellationToken) =>
{
    var reservation = await db.Reservations
        .SingleOrDefaultAsync(r => r.Id == id, cancellationToken);

    if (reservation is null)
        return Results.NotFound();

    var authenticatedUserId = principal.FindFirstValue(ClaimTypes.NameIdentifier)!;
    var isOwner = reservation.RequesterId == authenticatedUserId;
    var canOverride = principal.IsInRole(UserRole.Coordinator.ToString()) || principal.IsInRole(UserRole.Administrator.ToString());
    if (!isOwner && !canOverride)
        return Results.Forbid();

    reservation.Status = ReservationStatus.Cancelled;
    await db.SaveChangesAsync(cancellationToken);

    return Results.Ok(new { reservation.Id, status = reservation.Status.ToString() });
}).RequireAuthorization();

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

    var builder = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.IsDefaultPort ? 5432 : uri.Port,
        Database = uri.AbsolutePath.Trim('/'),
        Username = Uri.UnescapeDataString(uri.UserInfo[..separator]),
        Password = Uri.UnescapeDataString(uri.UserInfo[(separator + 1)..]),
        SslMode = SslMode.Require
    };

    return builder.ConnectionString;
}

static async Task SyncRoomResourcesAsync(RoomEntity room, IReadOnlyCollection<string>? resourceIds, ReservationDbContext db, CancellationToken cancellationToken)
{
    if (resourceIds is null || resourceIds.Count == 0)
        return;

    var validIds = await db.Resources
        .Where(r => resourceIds.Contains(r.Id))
        .Select(r => r.Id)
        .ToListAsync(cancellationToken);

    foreach (var resourceId in validIds)
    {
        room.Resources.Add(new RoomResourceEntity { RoomId = room.Id, ResourceId = resourceId });
    }
}

static IReadOnlyList<(DateTime Start, DateTime End)> ExpandOccurrences(DateTime start, DateTime end, WeeklyRecurrenceRequest? recurrence)
{
    if (recurrence is null)
        return [(start, end)];

    if (recurrence.Days.Length == 0)
        throw new ArgumentException("Selecione ao menos um dia da semana.");

    var days = recurrence.Days.Select(d => (DayOfWeek)d).ToHashSet();
    var occurrences = new List<(DateTime, DateTime)>();
    var duration = end - start;
    for (var date = start.Date; date <= recurrence.Until.Date; date = date.AddDays(1))
    {
        if (days.Contains(date.DayOfWeek))
            occurrences.Add((date.Add(start.TimeOfDay), date.Add(start.TimeOfDay + duration)));
    }

    return occurrences;
}

// Days encoded as .NET DayOfWeek ints (0 = Sunday ... 6 = Saturday), same
// convention as JavaScript's Date#getDay(), so the frontend can send them
// straight from its day-picker without translating anything.
public sealed record WeeklyRecurrenceRequest(int[] Days, DateTime Until);

public sealed record LoginRequest(string Email, string Password);

public sealed record LoginResponse(string AccessToken, DateTime ExpiresAt, UserResponse User);

public sealed record CreateReservationRequest(
    string RequesterId,
    string RoomId,
    DateTime Start,
    DateTime End,
    string Title,
    string? Responsavel,
    int Attendees,
    WeeklyRecurrenceRequest? Recurrence);

public sealed record UpsertRoomRequest(string? Id, string Name, string Floor, int Capacity, string? Description, string[] ResourceIds);

public sealed record UpsertResourceRequest(string? Id, string Name);

public sealed record UpsertUserRequest(string? Id, string Name, string Email, string Role, string[]? Floors, string? Password);

public sealed record RoomResponse(
    string Id,
    string Name,
    string Floor,
    int Capacity,
    string Description,
    bool Active,
    string[] ResourceIds,
    string[] Resources);

public sealed record ResourceResponse(string Id, string Name);

public sealed record UserResponse(string Id, string Name, string Email, string Role, bool Active, string[] Floors);

public sealed record OccurrenceResponse(DateTime Start, DateTime End);

public sealed record ConflictResponse(Guid Id, string Title, string Responsavel, DateTime Start, DateTime End);

public sealed record ReservationResponse(
    Guid Id,
    Guid? SeriesId,
    string RoomId,
    string RequesterId,
    string Title,
    string Responsavel,
    int Attendees,
    string Status,
    DateTime CreatedAt,
    string? DecidedBy,
    DateTime? DecidedAt,
    OccurrenceResponse Occurrence);
