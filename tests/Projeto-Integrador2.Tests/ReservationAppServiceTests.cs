using Microsoft.EntityFrameworkCore;
using Projeto_Integrador2.Domain;
using Projeto_Integrador2.Models;
using Projeto_Integrador2.Persistence;
using Projeto_Integrador2.Services;
using Xunit;

namespace Projeto_Integrador2.Tests;

public sealed class ReservationAppServiceTests
{
    [Fact]
    public async Task GetReservations_and_Approve_do_not_throw_when_occurrences_are_empty_or_multiple()
    {
        var options = new DbContextOptionsBuilder<ReservationDbContext>()
            .UseInMemoryDatabase($"reservation-app-service-{Guid.NewGuid():N}")
            .Options;

        await using var db = new ReservationDbContext(options);

        var emptyReservationId = Guid.NewGuid();
        var multiReservationId = Guid.NewGuid();
        var start = new DateTime(2026, 9, 10, 19, 0, 0, DateTimeKind.Utc);

        db.Reservations.Add(new ReservationEntity
        {
            Id = emptyReservationId,
            RequesterId = "teacher-1",
            RoomId = "room-204",
            Title = "Reserva sem ocorrência",
            Responsavel = "Docente 1",
            Attendees = 15,
            Status = ReservationStatus.Pending,
            CreatedAt = start
        });

        var multiReservation = new ReservationEntity
        {
            Id = multiReservationId,
            RequesterId = "teacher-1",
            RoomId = "room-204",
            Title = "Reserva com múltiplas ocorrências",
            Responsavel = "Docente 1",
            Attendees = 15,
            Status = ReservationStatus.Approved,
            CreatedAt = start.AddMinutes(1)
        };
        multiReservation.Occurrences.Add(new ReservationOccurrenceEntity
        {
            Id = Guid.NewGuid(),
            StartsAt = start,
            EndsAt = start.AddHours(2)
        });
        multiReservation.Occurrences.Add(new ReservationOccurrenceEntity
        {
            Id = Guid.NewGuid(),
            StartsAt = start.AddDays(7),
            EndsAt = start.AddDays(7).AddHours(2)
        });
        db.Reservations.Add(multiReservation);
        await db.SaveChangesAsync();

        var service = new ReservationAppService(db);

        var page = await service.GetReservationsAsync(null, 1, 20, CancellationToken.None);
        Assert.Equal(2, page.Total);
        var mappedMulti = Assert.Single(page.Data, r => r.Id == multiReservationId);
        Assert.Equal(start, mappedMulti.Occurrence.Start);

        var emptyApproval = await service.ApproveAsync(emptyReservationId, "admin-1", force: false, CancellationToken.None);
        Assert.Equal(ResultStatus.BadRequest, emptyApproval.Status);
    }

    [Fact]
    public async Task Approve_detects_conflict_on_subsequent_weekly_occurrences()
    {
        var options = new DbContextOptionsBuilder<ReservationDbContext>()
            .UseInMemoryDatabase($"reservation-app-service-recurring-{Guid.NewGuid():N}")
            .Options;

        await using var db = new ReservationDbContext(options);

        var week1Start = new DateTime(2026, 9, 10, 19, 0, 0, DateTimeKind.Utc);
        var week2Start = week1Start.AddDays(7);

        var approvedWeek2 = new ReservationEntity
        {
            Id = Guid.NewGuid(),
            RequesterId = "teacher-1",
            RoomId = "room-204",
            Title = "Aula já aprovada na Semana 2",
            Responsavel = "Docente 1",
            Attendees = 20,
            Status = ReservationStatus.Approved,
            CreatedAt = week1Start
        };
        approvedWeek2.Occurrences.Add(new ReservationOccurrenceEntity
        {
            Id = Guid.NewGuid(),
            StartsAt = week2Start,
            EndsAt = week2Start.AddHours(2)
        });

        var pendingRecurringId = Guid.NewGuid();
        var pendingRecurring = new ReservationEntity
        {
            Id = pendingRecurringId,
            RequesterId = "teacher-2",
            RoomId = "room-204",
            Title = "Curso Semanal (Semanas 1 e 2)",
            Responsavel = "Docente 2",
            Attendees = 20,
            Status = ReservationStatus.Pending,
            CreatedAt = week1Start.AddMinutes(5)
        };
        pendingRecurring.Occurrences.Add(new ReservationOccurrenceEntity
        {
            Id = Guid.NewGuid(),
            StartsAt = week1Start,
            EndsAt = week1Start.AddHours(2)
        });
        pendingRecurring.Occurrences.Add(new ReservationOccurrenceEntity
        {
            Id = Guid.NewGuid(),
            StartsAt = week2Start.AddMinutes(30),
            EndsAt = week2Start.AddHours(2)
        });

        db.Reservations.AddRange(approvedWeek2, pendingRecurring);
        await db.SaveChangesAsync();

        var service = new ReservationAppService(db);
        var result = await service.ApproveAsync(pendingRecurringId, "admin-1", force: false, CancellationToken.None);

        Assert.Equal(ResultStatus.Conflict, result.Status);
        var conflicts = Assert.IsType<List<ConflictResponse>>(result.Value.Conflicts);
        var conflict = Assert.Single(conflicts);
        Assert.Equal(approvedWeek2.Id, conflict.Id);
        Assert.Equal(week2Start, conflict.Start);
    }
}
