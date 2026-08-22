using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Projeto_Integrador2.Domain;
using Projeto_Integrador2.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Desabilita o recarregamento de arquivos para evitar o erro de inotify no Render
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

var connectionString = builder.Configuration.GetConnectionString("Supabase")
    ?? Environment.GetEnvironmentVariable("SUPABASE_CONNECTION_STRING");

if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Connection string não configurada.");

builder.Services.AddDbContext<ReservationDbContext>(options =>
    options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseCors();

// Endpoint de saúde
app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    database = "connected",
    timestamp = DateTime.UtcNow
}));

// ---- Endpoints (iguais aos originais, sem fallback) ----

app.MapGet("/api/rooms", async (ReservationDbContext db, CancellationToken cancellationToken) =>
{
    var rooms = await db.Rooms
        .AsNoTracking()
        .Include(room => room.Resources)
            .ThenInclude(link => link.Resource)
        .Where(room => room.Active)
        .Select(room => new RoomResponse(
            room.Id,
            room.Name,
            room.Floor,
            room.Capacity,
            room.Description,
            room.Resources.Select(link => link.Resource.Name).ToArray()))
        .ToListAsync(cancellationToken);

    return Results.Ok(rooms);
});

app.MapGet("/api/reservations", async (ReservationDbContext db, ReservationStatus? status, CancellationToken cancellationToken) =>
{
    var query = db.Reservations
        .AsNoTracking()
        .Include(r => r.Occurrences)
        .AsQueryable();

    if (status is not null)
        query = query.Where(r => r.Status == status);

    var reservations = await query
        .OrderBy(r => r.CreatedAt)
        .Select(r => new ReservationResponse(
            r.Id,
            r.RoomId,
            r.RequesterId,
            r.Title,
            r.Attendees,
            r.Status.ToString(),
            r.Occurrences
                .OrderBy(o => o.StartsAt)
                .Select(o => new OccurrenceResponse(o.StartsAt, o.EndsAt))
                .ToArray()))
        .ToListAsync(cancellationToken);

    return Results.Ok(reservations);
});

app.MapPost("/api/reservations", async (CreateReservationRequest input, ReservationDbContext db, CancellationToken cancellationToken) =>
{
    var room = await db.Rooms
        .AsNoTracking()
        .SingleOrDefaultAsync(r => r.Id == input.RoomId && r.Active, cancellationToken);

    if (room is null)
        return Results.NotFound(new { error = "Sala não encontrada." });

    var requesterExists = await db.Users
        .AnyAsync(u => u.Id == input.RequesterId && u.Active, cancellationToken);

    if (!requesterExists)
        return Results.BadRequest(new { error = "Usuário solicitante não encontrado ou inativo." });

    try
    {
        var service = new ReservationService([new Room(new RoomId(room.Id), room.Name, 0, room.Capacity, [])]);
        var domainReservation = service.Submit(new ReservationRequest(
            input.RequesterId,
            new RoomId(input.RoomId),
            input.Start,
            input.End,
            input.Title,
            input.Attendees,
            input.Recurrence));

        var entity = new ReservationEntity
        {
            Id = domainReservation.Id,
            RequesterId = input.RequesterId,
            RoomId = input.RoomId,
            Title = input.Title,
            Attendees = input.Attendees,
            SeriesId = input.Recurrence is null ? null : Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };

        foreach (var occurrence in domainReservation.Occurrences)
        {
            entity.Occurrences.Add(new ReservationOccurrenceEntity
            {
                Id = Guid.NewGuid(),
                StartsAt = occurrence.Start,
                EndsAt = occurrence.End
            });
        }

        db.Reservations.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/reservations/{entity.Id}", new
        {
            entity.Id,
            status = entity.Status.ToString()
        });
    }
    catch (Exception ex) when (ex is ArgumentException or CapacityExceededException)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/reservations/{id:guid}/approve", async (Guid id, DecideReservationRequest input, ReservationDbContext db, CancellationToken cancellationToken) =>
{
    if (input.Role is not (UserRole.Coordinator or UserRole.Administrator))
        return Results.Forbid();

    var reservation = await db.Reservations
        .Include(r => r.Occurrences)
        .SingleOrDefaultAsync(r => r.Id == id, cancellationToken);

    if (reservation is null)
        return Results.NotFound();

    var hasConflict = await db.ReservationOccurrences
        .Include(o => o.Reservation)
        .AnyAsync(o =>
            o.ReservationId != id &&
            o.Reservation.RoomId == reservation.RoomId &&
            o.Reservation.Status == ReservationStatus.Approved &&
            reservation.Occurrences.Any(occ => occ.StartsAt < o.EndsAt && o.StartsAt < occ.EndsAt),
            cancellationToken);

    if (hasConflict)
        return Results.Conflict(new { error = "Já existe uma reserva aprovada para esse horário e sala." });

    reservation.Status = ReservationStatus.Approved;
    reservation.DecidedBy = input.UserId;
    reservation.DecidedAt = DateTime.UtcNow;

    await db.SaveChangesAsync(cancellationToken);

    return Results.Ok(new { reservation.Id, status = reservation.Status.ToString() });
});

app.MapPost("/api/reservations/{id:guid}/cancel", async (Guid id, DecideReservationRequest input, ReservationDbContext db, CancellationToken cancellationToken) =>
{
    var reservation = await db.Reservations
        .SingleOrDefaultAsync(r => r.Id == id, cancellationToken);

    if (reservation is null)
        return Results.NotFound();

    if (reservation.RequesterId != input.UserId && input.Role is not (UserRole.Coordinator or UserRole.Administrator))
        return Results.Forbid();

    reservation.Status = ReservationStatus.Cancelled;
    await db.SaveChangesAsync(cancellationToken);

    return Results.Ok(new { reservation.Id, status = reservation.Status.ToString() });
});

app.Run();

// ---- Records (mantidos) ----

public sealed record CreateReservationRequest(
    string RequesterId,
    string RoomId,
    DateTime Start,
    DateTime End,
    string Title,
    int Attendees,
    WeeklyRecurrence? Recurrence);

public sealed record DecideReservationRequest(string UserId, UserRole Role);

public sealed record RoomResponse(
    string Id,
    string Name,
    string Floor,
    int Capacity,
    string Description,
    string[] Resources);

public sealed record OccurrenceResponse(DateTime Start, DateTime End);

public sealed record ReservationResponse(
    Guid Id,
    string RoomId,
    string RequesterId,
    string Title,
    int Attendees,
    string Status,
    OccurrenceResponse[] Occurrences);