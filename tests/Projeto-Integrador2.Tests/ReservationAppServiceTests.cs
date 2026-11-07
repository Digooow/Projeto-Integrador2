using Microsoft.EntityFrameworkCore;
using Projeto_Integrador2.Domain;
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
}
