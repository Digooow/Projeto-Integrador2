using Projeto_Integrador2.Domain;

namespace Projeto_Integrador2.Persistence;

public sealed class UserEntity
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public UserRole Role { get; set; }
    public bool Active { get; set; } = true;

    /// <summary>
    /// Floors this user is allowed to approve reservations for. Only meaningful
    /// for <see cref="UserRole.Coordinator"/>; empty for the other roles
    /// (Administrator sees every floor, Teacher/Collaborator only request rooms).
    /// </summary>
    public List<string> Floors { get; set; } = [];

    public ICollection<ReservationEntity> Reservations { get; } = [];
}

public sealed class RoomEntity
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Floor { get; set; }
    public required string Description { get; set; }
    public int Capacity { get; set; }
    public bool Active { get; set; } = true;
    public ICollection<RoomResourceEntity> Resources { get; } = [];
    public ICollection<ReservationEntity> Reservations { get; } = [];
}

public sealed class ResourceEntity
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public ICollection<RoomResourceEntity> Rooms { get; } = [];
}

public sealed class RoomResourceEntity
{
    public required string RoomId { get; set; }
    public RoomEntity Room { get; set; } = null!;
    public required string ResourceId { get; set; }
    public ResourceEntity Resource { get; set; } = null!;
}

public sealed class ReservationEntity
{
    public Guid Id { get; set; }
    public required string RequesterId { get; set; }
    public UserEntity Requester { get; set; } = null!;
    public required string RoomId { get; set; }
    public RoomEntity Room { get; set; } = null!;
    public required string Title { get; set; }

    /// <summary>Free-text name of the person responsible for the activity (may differ from the requester).</summary>
    public required string Responsavel { get; set; }
    public int Attendees { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
    public Guid? SeriesId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DecidedAt { get; set; }
    public string? DecidedBy { get; set; }
    public ICollection<ReservationOccurrenceEntity> Occurrences { get; } = [];
}

public sealed class ReservationOccurrenceEntity
{
    public Guid Id { get; set; }
    public Guid ReservationId { get; set; }
    public ReservationEntity Reservation { get; set; } = null!;
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
}