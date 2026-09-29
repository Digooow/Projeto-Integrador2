using Microsoft.EntityFrameworkCore;
using Projeto_Integrador2.Models;
using Projeto_Integrador2.Persistence;

namespace Projeto_Integrador2.Services;
public sealed class RoomService(ReservationDbContext db)
{
    public async Task<List<RoomResponse>> GetRoomsAsync(bool includeInactive, CancellationToken ct)
    {
        var query = db.Rooms
            .AsNoTracking()
            .Include(r => r.Resources)
                .ThenInclude(link => link.Resource)
            .AsQueryable();

        if (!includeInactive)
            query = query.Where(r => r.Active);

        return await query
            .OrderBy(r => r.Floor).ThenBy(r => r.Name)
            .Select(r => new RoomResponse(
                r.Id,
                r.Name,
                r.Floor,
                r.Capacity,
                r.Description,
                r.Active,
                r.Resources.Select(link => link.Resource.Id).ToArray(),
                r.Resources.Select(link => link.Resource.Name).ToArray()))
            .ToListAsync(ct);
    }
    public async Task<ServiceResult<string>> CreateRoomAsync(UpsertRoomRequest input, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(input.Id) || string.IsNullOrWhiteSpace(input.Name)
            || string.IsNullOrWhiteSpace(input.Floor) || input.Capacity <= 0)
            return ServiceResult<string>.BadRequest("Informe id, nome, andar e capacidade (> 0).");

        if (await db.Rooms.AnyAsync(r => r.Id == input.Id, ct))
            return ServiceResult<string>.Conflict("Já existe uma sala com esse identificador.");

        var room = new RoomEntity
        {
            Id = input.Id,
            Name = input.Name,
            Floor = input.Floor,
            Capacity = input.Capacity,
            Description = input.Description ?? "",
            Active = true
        };

        await SyncResourcesAsync(room, input.ResourceIds, ct);
        db.Rooms.Add(room);
        await db.SaveChangesAsync(ct);

        return ServiceResult<string>.Ok(room.Id);
    }
    public async Task<ServiceResult> UpdateRoomAsync(string id, UpsertRoomRequest input, CancellationToken ct)
    {
        var room = await db.Rooms.Include(r => r.Resources).SingleOrDefaultAsync(r => r.Id == id, ct);
        if (room is null) return ServiceResult.NotFound();

        if (string.IsNullOrWhiteSpace(input.Name) || string.IsNullOrWhiteSpace(input.Floor) || input.Capacity <= 0)
            return ServiceResult.BadRequest("Informe nome, andar e capacidade (> 0).");

        room.Name = input.Name;
        room.Floor = input.Floor;
        room.Capacity = input.Capacity;
        room.Description = input.Description ?? "";
        room.Resources.Clear();

        await SyncResourcesAsync(room, input.ResourceIds, ct);
        await db.SaveChangesAsync(ct);

        return ServiceResult.Ok();
    }
    public async Task<ServiceResult<bool>> ToggleActiveAsync(string id, CancellationToken ct)
    {
        var room = await db.Rooms.SingleOrDefaultAsync(r => r.Id == id, ct);
        if (room is null) return ServiceResult<bool>.NotFound();

        room.Active = !room.Active;
        await db.SaveChangesAsync(ct);

        return ServiceResult<bool>.Ok(room.Active);
    }

    private async Task SyncResourcesAsync(
        RoomEntity room, IReadOnlyCollection<string>? resourceIds, CancellationToken ct)
    {
        if (resourceIds is null || resourceIds.Count == 0) return;

        var validIds = await db.Resources
            .Where(r => resourceIds.Contains(r.Id))
            .Select(r => r.Id)
            .ToListAsync(ct);

        foreach (var resourceId in validIds)
            room.Resources.Add(new RoomResourceEntity { RoomId = room.Id, ResourceId = resourceId });
    }
}
