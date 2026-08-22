using Projeto_Integrador2.Domain;
using Xunit;

namespace Projeto_Integrador2.Tests;

public class ReservationServiceTests
{
    private static readonly DateTime Start = new(2026, 9, 1, 19, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime End = new(2026, 9, 1, 22, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Submit_creates_a_pending_request_without_reserving_the_room()
    {
        var service = NewService();
        var request = Request("teacher-1", new RoomId("204"));

        var result = service.Submit(request);

        Assert.Equal(ReservationStatus.Pending, result.Status);
        Assert.Single(service.PendingRequests());
        Assert.Empty(service.ConfirmedReservations());
    }

    [Fact]
    public void Submit_expands_a_weekly_request_until_the_end_date()
    {
        var service = NewService();
        var request = Request(
            "teacher-1",
            new RoomId("204"),
            recurrence: new WeeklyRecurrence(
                Days: [DayOfWeek.Tuesday, DayOfWeek.Thursday],
                Until: new DateTime(2026, 9, 17, 22, 0, 0, DateTimeKind.Utc)));

        var result = service.Submit(request);

        Assert.Equal(6, result.Occurrences.Count);
        Assert.Equal(
            [DayOfWeek.Tuesday, DayOfWeek.Thursday, DayOfWeek.Tuesday,
             DayOfWeek.Thursday, DayOfWeek.Tuesday, DayOfWeek.Thursday],
            result.Occurrences.Select(occurrence => occurrence.Start.DayOfWeek));
    }

    [Fact]
    public void Approve_is_restricted_to_users_with_approval_permission()
    {
        var service = NewService();
        var request = service.Submit(Request("teacher-1", new RoomId("204")));

        var exception = Assert.Throws<UnauthorizedAccessException>(() =>
            service.Approve(request.Id, new User("teacher-2", UserRole.Teacher)));

        Assert.Contains("approval", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Approve_confirms_the_request_and_makes_its_occurrences_visible()
    {
        var service = NewService();
        var request = service.Submit(Request("teacher-1", new RoomId("204")));

        var result = service.Approve(request.Id, new User("admin-1", UserRole.Administrator));

        Assert.Equal(ReservationStatus.Approved, result.Status);
        Assert.Single(service.ConfirmedReservations());
    }

    [Fact]
    public void Approve_rejects_an_overlapping_confirmed_reservation()
    {
        var service = NewService();
        var first = service.Submit(Request("teacher-1", new RoomId("204")));
        service.Approve(first.Id, new User("admin-1", UserRole.Administrator));
        var second = service.Submit(Request("teacher-2", new RoomId("204"), Start.AddHours(2)));

        var exception = Assert.Throws<ReservationConflictException>(() =>
            service.Approve(second.Id, new User("admin-1", UserRole.Administrator)));

        Assert.Equal(ReservationStatus.Pending, service.Get(second.Id).Status);
        Assert.Contains("204", exception.Message);
    }

    [Fact]
    public void Submit_rejects_a_request_that_exceeds_room_capacity()
    {
        var service = NewService();

        var exception = Assert.Throws<CapacityExceededException>(() =>
            service.Submit(Request("teacher-1", new RoomId("204"), attendees: 31)));

        Assert.Contains("capacity", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Owner_can_cancel_own_request_but_another_user_cannot()
    {
        var service = NewService();
        var request = service.Submit(Request("teacher-1", new RoomId("204")));

        Assert.Throws<UnauthorizedAccessException>(() =>
            service.Cancel(request.Id, new User("teacher-2", UserRole.Teacher)));

        var cancelled = service.Cancel(request.Id, new User("teacher-1", UserRole.Teacher));

        Assert.Equal(ReservationStatus.Cancelled, cancelled.Status);
        Assert.NotEmpty(service.History());
    }

    private static ReservationRequest Request(
        string userId,
        RoomId roomId,
        DateTime? start = null,
        int attendees = 20,
        WeeklyRecurrence? recurrence = null) =>
        new(
            userId,
            roomId,
            start ?? Start,
            (start ?? Start).Date.AddHours(End.Hour),
            "Extra class",
            attendees,
            recurrence);

    private static ReservationService NewService() =>
        new([new Room(new RoomId("204"), "Sala 204", 2, 30, ["Projector", "Whiteboard"])]);
}
