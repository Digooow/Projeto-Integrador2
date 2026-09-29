using System.Text.Json.Serialization;

namespace Projeto_Integrador2.Models;

public sealed class LoginRequest
{
    [JsonPropertyName("email")]
    public string Email { get; init; } = "";

    [JsonPropertyName("password")]
    public string Password { get; init; } = "";
}

public sealed record RegisterRequest(string Name, string Email, string Password);
public sealed record WeeklyRecurrenceRequest(int[] Days, DateTime Until);

public sealed record CreateReservationRequest(
    string RequesterId,
    string RoomId,
    DateTime Start,
    DateTime End,
    string Title,
    string? Responsavel,
    int Attendees,
    WeeklyRecurrenceRequest? Recurrence);

public sealed record UpsertRoomRequest(
    string? Id,
    string Name,
    string Floor,
    int Capacity,
    string? Description,
    string[] ResourceIds);

public sealed record UpsertResourceRequest(string? Id, string Name);

public sealed record UpsertUserRequest(
    string? Id,
    string Name,
    string Email,
    string Role,
    string[]? Floors,
    string? Password);

public sealed record LoginResponse(string AccessToken, DateTime ExpiresAt, UserResponse User);

public sealed record UserResponse(
    string Id,
    string Name,
    string Email,
    string Role,
    bool Active,
    string[] Floors);

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

public sealed record OccurrenceResponse(DateTime Start, DateTime End);

public sealed record ConflictResponse(
    Guid Id,
    string Title,
    string Responsavel,
    DateTime Start,
    DateTime End);

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

public sealed record PaginatedResult<T>(
    IReadOnlyList<T> Data,
    int Page,
    int PageSize,
    int Total,
    int TotalPages);
