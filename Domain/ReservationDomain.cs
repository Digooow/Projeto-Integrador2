namespace Projeto_Integrador2.Domain;

public enum ReservationStatus
{
    Pending,
    Approved,
    Rejected,
    Cancelled
}

public enum UserRole
{
    Teacher,
    Collaborator,
    Coordinator,
    Administrator
}

public sealed record User(string Id, UserRole Role);
public sealed record RoomId(string Value);

public sealed record Room(
    RoomId Id,
    string Name,
    int Floor,
    int Capacity,
    IReadOnlyCollection<string> Resources);

public sealed record WeeklyRecurrence(
    IReadOnlyCollection<DayOfWeek> Days,
    DateTime Until);

public sealed record ReservationRequest(
    string UserId,
    RoomId RoomId,
    DateTime Start,
    DateTime End,
    string Purpose,
    int Attendees,
    WeeklyRecurrence? Recurrence = null);

public sealed record ReservationOccurrence(DateTime Start, DateTime End);

public sealed class Reservation
{
    internal Reservation(ReservationRequest request, IReadOnlyList<ReservationOccurrence> occurrences)
    {
        Id = Guid.NewGuid();
        UserId = request.UserId;
        RoomId = request.RoomId;
        Purpose = request.Purpose;
        Attendees = request.Attendees;
        Occurrences = occurrences;
    }

    public Guid Id { get; }
    public string UserId { get; }
    public RoomId RoomId { get; }
    public string Purpose { get; }
    public int Attendees { get; }
    public ReservationStatus Status { get; internal set; } = ReservationStatus.Pending;
    public IReadOnlyList<ReservationOccurrence> Occurrences { get; }
}

public sealed class ReservationConflictException(string message) : InvalidOperationException(message);
public sealed class CapacityExceededException(string message) : InvalidOperationException(message);

public sealed class ReservationService
{
    private readonly IReadOnlyDictionary<RoomId, Room> rooms;
    private readonly List<Reservation> reservations = [];

    public ReservationService(IEnumerable<Room> rooms)
    {
        this.rooms = rooms.ToDictionary(room => room.Id);
    }

    public Reservation Submit(ReservationRequest request)
    {
        if (!rooms.TryGetValue(request.RoomId, out var room))
            throw new KeyNotFoundException($"Room '{request.RoomId.Value}' was not found.");

        if (request.Attendees <= 0)
            throw new ArgumentOutOfRangeException(nameof(request.Attendees));

        if (request.Attendees > room.Capacity)
            throw new CapacityExceededException($"Room '{room.Name}' capacity is {room.Capacity}.");

        var reservation = new Reservation(request, ExpandOccurrences(request));
        reservations.Add(reservation);
        return reservation;
    }

    public Reservation Approve(Guid reservationId, User approver)
    {
        EnsureApprover(approver);
        var reservation = Get(reservationId);

        if (reservations
            .Where(existing => existing.Status == ReservationStatus.Approved && existing.RoomId == reservation.RoomId)
            .SelectMany(existing => existing.Occurrences)
            .Any(existingOccurrence => reservation.Occurrences.Any(newOccurrence => Overlaps(existingOccurrence, newOccurrence))))
        {
            throw new ReservationConflictException($"Room '{reservation.RoomId.Value}' has an overlapping reservation.");
        }

        reservation.Status = ReservationStatus.Approved;
        return reservation;
    }

    public Reservation Cancel(Guid reservationId, User user)
    {
        var reservation = Get(reservationId);
        if (reservation.UserId != user.Id && user.Role is not (UserRole.Coordinator or UserRole.Administrator))
            throw new UnauthorizedAccessException("Only the owner can cancel this reservation.");

        reservation.Status = ReservationStatus.Cancelled;
        return reservation;
    }

    public Reservation Get(Guid reservationId) =>
        reservations.SingleOrDefault(reservation => reservation.Id == reservationId)
        ?? throw new KeyNotFoundException("Reservation was not found.");

    public IReadOnlyList<Reservation> PendingRequests() =>
        reservations.Where(reservation => reservation.Status == ReservationStatus.Pending).ToArray();

    public IReadOnlyList<Reservation> ConfirmedReservations() =>
        reservations.Where(reservation => reservation.Status == ReservationStatus.Approved).ToArray();

    public IReadOnlyList<Reservation> History() => reservations.ToArray();

    private static void EnsureApprover(User user)
    {
        if (user.Role is not (UserRole.Coordinator or UserRole.Administrator))
            throw new UnauthorizedAccessException("Only users with approval permission can approve reservations.");
    }

    private static bool Overlaps(ReservationOccurrence first, ReservationOccurrence second) =>
        first.Start < second.End && second.Start < first.End;

    private static IReadOnlyList<ReservationOccurrence> ExpandOccurrences(ReservationRequest request)
    {
        if (request.End <= request.Start)
            throw new ArgumentException("Reservation end must be after its start.", nameof(request));

        if (request.Recurrence is null)
            return [new ReservationOccurrence(request.Start, request.End)];

        var occurrences = new List<ReservationOccurrence>();
        var duration = request.End - request.Start;
        for (var date = request.Start.Date; date <= request.Recurrence.Until.Date; date = date.AddDays(1))
        {
            if (request.Recurrence.Days.Contains(date.DayOfWeek))
                occurrences.Add(new ReservationOccurrence(date.Add(request.Start.TimeOfDay), date.Add(request.Start.TimeOfDay + duration)));
        }

        return occurrences;
    }
}
