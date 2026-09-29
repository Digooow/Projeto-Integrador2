using Microsoft.EntityFrameworkCore;
using Projeto_Integrador2.Models;
using Projeto_Integrador2.Persistence;

namespace Projeto_Integrador2.Services;
public sealed class ResourceService(ReservationDbContext db)
{
    public async Task<List<ResourceResponse>> GetResourcesAsync(CancellationToken ct) =>
        await db.Resources
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new ResourceResponse(r.Id, r.Name))
            .ToListAsync(ct);
    public async Task<ServiceResult<string>> CreateResourceAsync(UpsertResourceRequest input, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(input.Id) || string.IsNullOrWhiteSpace(input.Name))
            return ServiceResult<string>.BadRequest("Informe id e nome do recurso.");

        if (await db.Resources.AnyAsync(r => r.Id == input.Id, ct))
            return ServiceResult<string>.Conflict("Já existe um recurso com esse identificador.");

        db.Resources.Add(new ResourceEntity { Id = input.Id, Name = input.Name });
        await db.SaveChangesAsync(ct);

        return ServiceResult<string>.Ok(input.Id);
    }
}
