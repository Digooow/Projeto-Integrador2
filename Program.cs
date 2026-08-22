using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Projeto_Integrador2.Domain;
using Projeto_Integrador2.Persistence;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Supabase")
	?? Environment.GetEnvironmentVariable("SUPABASE_CONNECTION_STRING");

if (string.IsNullOrWhiteSpace(connectionString))
	throw new InvalidOperationException("Configure a connection string em ConnectionStrings:Supabase ou SUPABASE_CONNECTION_STRING.");

builder.Services.AddDbContext<ReservationDbContext>(options => options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
app.UseCors();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/api/rooms", async (ReservationDbContext db, CancellationToken cancellationToken) =>
	Results.Ok(await db.Rooms.AsNoTracking().Include(room => room.Resources).ThenInclude(link => link.Resource)
		.Where(room => room.Active).Select(room => new RoomResponse(room.Id, room.Name, room.Floor, room.Capacity,
			room.Description, room.Resources.Select(link => link.Resource.Name).ToArray())).ToListAsync(cancellationToken)));

app.MapGet("/api/reservations", async (ReservationDbContext db, ReservationStatus? status, CancellationToken cancellationToken) =>
{
	var query = db.Reservations.AsNoTracking().Include(reservation => reservation.Occurrences).AsQueryable();
	if (status is not null) query = query.Where(reservation => reservation.Status == status);
	return Results.Ok(await query.OrderBy(reservation => reservation.CreatedAt).Select(reservation =>
		new ReservationResponse(reservation.Id, reservation.RoomId, reservation.RequesterId, reservation.Title,
			reservation.Attendees, reservation.Status.ToString(), reservation.Occurrences.OrderBy(occurrence => occurrence.StartsAt)
				.Select(occurrence => new OccurrenceResponse(occurrence.StartsAt, occurrence.EndsAt)).ToArray())).ToListAsync(cancellationToken));
});

app.MapPost("/api/reservations", async (CreateReservationRequest input, ReservationDbContext db, CancellationToken cancellationToken) =>
{
	var room = await db.Rooms.AsNoTracking().SingleOrDefaultAsync(item => item.Id == input.RoomId && item.Active, cancellationToken);
	if (room is null) return Results.NotFound(new { error = "Sala não encontrada." });
	if (!await db.Users.AnyAsync(user => user.Id == input.RequesterId && user.Active, cancellationToken))
		return Results.BadRequest(new { error = "Usuário solicitante não encontrado ou inativo." });

	try
	{
		var service = new ReservationService([new Room(new RoomId(room.Id), room.Name, 0, room.Capacity, [])]);
		var domainReservation = service.Submit(new ReservationRequest(input.RequesterId, new RoomId(input.RoomId),
			input.Start, input.End, input.Title, input.Attendees, input.Recurrence));
		var entity = new ReservationEntity
		{
			Id = domainReservation.Id, RequesterId = input.RequesterId, RoomId = input.RoomId, Title = input.Title,
			Attendees = input.Attendees, SeriesId = input.Recurrence is null ? null : Guid.NewGuid(), CreatedAt = DateTime.UtcNow
		};
		foreach (var occurrence in domainReservation.Occurrences)
			entity.Occurrences.Add(new ReservationOccurrenceEntity { Id = Guid.NewGuid(), StartsAt = occurrence.Start, EndsAt = occurrence.End });
		db.Reservations.Add(entity);
		await db.SaveChangesAsync(cancellationToken);
		return Results.Created($"/api/reservations/{entity.Id}", new { entity.Id, status = entity.Status.ToString() });
	}
	catch (Exception exception) when (exception is ArgumentException or CapacityExceededException)
	{
		return Results.BadRequest(new { error = exception.Message });
	}
});

app.MapPost("/api/reservations/{id:guid}/approve", async (Guid id, DecideReservationRequest input, ReservationDbContext db, CancellationToken cancellationToken) =>
{
	if (input.Role is not (UserRole.Coordinator or UserRole.Administrator)) return Results.Forbid();
	var reservation = await db.Reservations.Include(item => item.Occurrences).SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
	if (reservation is null) return Results.NotFound();
	var conflict = await db.ReservationOccurrences.Include(item => item.Reservation).AnyAsync(item =>
		item.ReservationId != id && item.Reservation.RoomId == reservation.RoomId && item.Reservation.Status == ReservationStatus.Approved &&
		reservation.Occurrences.Any(newItem => newItem.StartsAt < item.EndsAt && item.StartsAt < newItem.EndsAt), cancellationToken);
	if (conflict) return Results.Conflict(new { error = "Já existe uma reserva aprovada para esse horário e sala." });
	reservation.Status = ReservationStatus.Approved;
	reservation.DecidedBy = input.UserId;
	reservation.DecidedAt = DateTime.UtcNow;
	await db.SaveChangesAsync(cancellationToken);
	return Results.Ok(new { reservation.Id, status = reservation.Status.ToString() });
});

app.MapPost("/api/reservations/{id:guid}/cancel", async (Guid id, DecideReservationRequest input, ReservationDbContext db, CancellationToken cancellationToken) =>
{
	var reservation = await db.Reservations.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
	if (reservation is null) return Results.NotFound();
	if (reservation.RequesterId != input.UserId && input.Role is not (UserRole.Coordinator or UserRole.Administrator)) return Results.Forbid();
	reservation.Status = ReservationStatus.Cancelled;
	await db.SaveChangesAsync(cancellationToken);
	return Results.Ok(new { reservation.Id, status = reservation.Status.ToString() });
});

app.Run();

public sealed record CreateReservationRequest(string RequesterId, string RoomId, DateTime Start, DateTime End, string Title, int Attendees, WeeklyRecurrence? Recurrence);
public sealed record DecideReservationRequest(string UserId, UserRole Role);
public sealed record RoomResponse(string Id, string Name, string Floor, int Capacity, string Description, string[] Resources);
public sealed record OccurrenceResponse(DateTime Start, DateTime End);
public sealed record ReservationResponse(Guid Id, string RoomId, string RequesterId, string Title, int Attendees, string Status, OccurrenceResponse[] Occurrences);
