using Microsoft.EntityFrameworkCore;
using Projeto_Integrador2.Domain;
using Projeto_Integrador2.Models;
using Projeto_Integrador2.Persistence;

namespace Projeto_Integrador2.Services;
public sealed class ReservationAppService(ReservationDbContext db)
{
    public async Task<PaginatedResult<ReservationResponse>> GetReservationsAsync(
        ReservationStatus? status, int page, int pageSize, CancellationToken ct)
    {
        var currentPage = Math.Max(page, 1);
        var currentPageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.Reservations
            .AsNoTracking()
            .Include(r => r.Occurrences)
            .AsQueryable();

        if (status is not null)
            query = query.Where(r => r.Status == status);

        var total = await query.CountAsync(ct);

        var data = await query
            .OrderBy(r => r.CreatedAt).ThenBy(r => r.Id)
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
            .ToListAsync(ct);

        return new PaginatedResult<ReservationResponse>(
            data, currentPage, currentPageSize, total,
            (int)Math.Ceiling(total / (double)currentPageSize));
    }
    public async Task<ServiceResult<(List<Guid> Ids, Guid? SeriesId)>> CreateAsync(
        CreateReservationRequest input, string authenticatedUserId, CancellationToken ct)
    {
        if (authenticatedUserId != input.RequesterId)
            return ServiceResult<(List<Guid>, Guid?)>.Forbidden();

        var room = await db.Rooms.AsNoTracking()
            .SingleOrDefaultAsync(r => r.Id == input.RoomId && r.Active, ct);

        if (room is null)
            return ServiceResult<(List<Guid>, Guid?)>.NotFound();

        if (!await db.Users.AnyAsync(u => u.Id == input.RequesterId && u.Active, ct))
            return ServiceResult<(List<Guid>, Guid?)>.BadRequest("Usuário solicitante não encontrado ou inativo.");

        if (input.Attendees <= 0)
            return ServiceResult<(List<Guid>, Guid?)>.BadRequest("Informe ao menos 1 participante.");

        if (input.Attendees > room.Capacity)
            return ServiceResult<(List<Guid>, Guid?)>.BadRequest(
                $"A sala '{room.Name}' tem capacidade para {room.Capacity} pessoas.");

        if (input.End <= input.Start)
            return ServiceResult<(List<Guid>, Guid?)>.BadRequest("O horário final precisa ser depois do inicial.");

        IReadOnlyList<(DateTime Start, DateTime End)> occurrences;
        try
        {
            occurrences = ExpandOccurrences(input.Start, input.End, input.Recurrence);
        }
        catch (ArgumentException ex)
        {
            return ServiceResult<(List<Guid>, Guid?)>.BadRequest(ex.Message);
        }

        if (occurrences.Count == 0)
            return ServiceResult<(List<Guid>, Guid?)>.BadRequest(
                "Nenhuma data cai nos dias selecionados dentro do período informado.");

        var seriesId = occurrences.Count > 1 ? Guid.NewGuid() : (Guid?)null;
        var now = DateTime.UtcNow;
        var createdIds = new List<Guid>(occurrences.Count);

        foreach (var (start, end) in occurrences)
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
                StartsAt = start,
                EndsAt = end
            });
            db.Reservations.Add(entity);
            createdIds.Add(entity.Id);
        }

        await db.SaveChangesAsync(ct);

        return ServiceResult<(List<Guid>, Guid?)>.Ok((createdIds, seriesId));
    }
    public async Task<ServiceResult<(Guid Id, List<ConflictResponse>? Conflicts)>> ApproveAsync(
        Guid id, string deciderId, bool force, CancellationToken ct)
    {
        var reservation = await db.Reservations
            .Include(r => r.Occurrences)
            .SingleOrDefaultAsync(r => r.Id == id, ct);

        if (reservation is null)
            return ServiceResult<(Guid, List<ConflictResponse>?)>.NotFound();

        if (reservation.Status != ReservationStatus.Pending)
            return ServiceResult<(Guid, List<ConflictResponse>?)>.BadRequest("Este pedido já foi decidido.");

        if (!force)
        {
            var occurrence = reservation.Occurrences.Single();

            var conflicts = await db.Reservations
                .AsNoTracking()
                .Include(r => r.Occurrences)
                .Where(r =>
                    r.Id != id &&
                    r.RoomId == reservation.RoomId &&
                    r.Status == ReservationStatus.Approved &&
                    r.Occurrences.Any(o => o.StartsAt < occurrence.EndsAt && occurrence.StartsAt < o.EndsAt))
                .Select(r => new ConflictResponse(
                    r.Id,
                    r.Title,
                    r.Responsavel,
                    r.Occurrences.Select(o => o.StartsAt).Single(),
                    r.Occurrences.Select(o => o.EndsAt).Single()))
                .ToListAsync(ct);

            if (conflicts.Count > 0)
                return ServiceResult<(Guid, List<ConflictResponse>?)>.Conflict(
                    "Já existe uma reserva aprovada para esse horário e sala.")
                    with { Value = (id, conflicts) };
        }

        reservation.Status = ReservationStatus.Approved;
        reservation.DecidedBy = deciderId;
        reservation.DecidedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return ServiceResult<(Guid, List<ConflictResponse>?)>.Ok((id, null));
    }
    public async Task<ServiceResult<Guid>> RejectAsync(Guid id, string deciderId, CancellationToken ct)
    {
        var reservation = await db.Reservations.SingleOrDefaultAsync(r => r.Id == id, ct);
        if (reservation is null) return ServiceResult<Guid>.NotFound();
        if (reservation.Status != ReservationStatus.Pending)
            return ServiceResult<Guid>.BadRequest("Este pedido já foi decidido.");

        reservation.Status = ReservationStatus.Rejected;
        reservation.DecidedBy = deciderId;
        reservation.DecidedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return ServiceResult<Guid>.Ok(id);
    }
    public async Task<ServiceResult<Guid>> CancelAsync(
        Guid id, string authenticatedUserId, bool canOverride, CancellationToken ct)
    {
        var reservation = await db.Reservations.SingleOrDefaultAsync(r => r.Id == id, ct);
        if (reservation is null) return ServiceResult<Guid>.NotFound();

        var isOwner = reservation.RequesterId == authenticatedUserId;
        if (!isOwner && !canOverride) return ServiceResult<Guid>.Forbidden();

        reservation.Status = ReservationStatus.Cancelled;
        await db.SaveChangesAsync(ct);

        return ServiceResult<Guid>.Ok(id);
    }
    private static IReadOnlyList<(DateTime Start, DateTime End)> ExpandOccurrences(
        DateTime start, DateTime end, WeeklyRecurrenceRequest? recurrence)
    {
        if (recurrence is null) return [(start, end)];

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
}
