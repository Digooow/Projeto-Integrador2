using Microsoft.EntityFrameworkCore;
using Projeto_Integrador2.Domain;
using Projeto_Integrador2.Models;
using Projeto_Integrador2.Persistence;
using Projeto_Integrador2.Security;

namespace Projeto_Integrador2.Services;
public sealed class UserService(ReservationDbContext db)
{
    public async Task<List<UserResponse>> GetUsersAsync(CancellationToken ct) =>
        await db.Users
            .AsNoTracking()
            .OrderBy(u => u.Name)
            .Select(u => new UserResponse(u.Id, u.Name, u.Email, u.Role.ToString(), u.Active, u.Floors.ToArray()))
            .ToListAsync(ct);
    public async Task<ServiceResult<string>> CreateUserAsync(UpsertUserRequest input, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(input.Id) || string.IsNullOrWhiteSpace(input.Name)
            || string.IsNullOrWhiteSpace(input.Email))
            return ServiceResult<string>.BadRequest("Informe id, nome e e-mail.");

        if (!Enum.TryParse<UserRole>(input.Role, ignoreCase: true, out var role))
            return ServiceResult<string>.BadRequest($"Papel inválido: {input.Role}.");

        if (await db.Users.AnyAsync(u => u.Id == input.Id || u.Email == input.Email, ct))
            return ServiceResult<string>.Conflict("Já existe um usuário com esse identificador ou e-mail.");

        if (string.IsNullOrWhiteSpace(input.Password))
            return ServiceResult<string>.BadRequest("Informe uma senha.");

        db.Users.Add(new UserEntity
        {
            Id = input.Id,
            Name = input.Name,
            Email = input.Email,
            PasswordHash = PasswordHasher.Hash(input.Password),
            Role = role,
            Active = true,
            Floors = role == UserRole.Coordinator ? (input.Floors ?? []).ToList() : []
        });
        await db.SaveChangesAsync(ct);

        return ServiceResult<string>.Ok(input.Id);
    }
    public async Task<ServiceResult> UpdateUserAsync(string id, UpsertUserRequest input, CancellationToken ct)
    {
        var user = await db.Users.SingleOrDefaultAsync(u => u.Id == id, ct);
        if (user is null) return ServiceResult.NotFound();

        if (string.IsNullOrWhiteSpace(input.Name) || string.IsNullOrWhiteSpace(input.Email))
            return ServiceResult.BadRequest("Informe nome e e-mail.");

        if (!Enum.TryParse<UserRole>(input.Role, ignoreCase: true, out var role))
            return ServiceResult.BadRequest($"Papel inválido: {input.Role}.");

        user.Name = input.Name;
        user.Email = input.Email;
        user.Role = role;
        user.Floors = role == UserRole.Coordinator ? (input.Floors ?? []).ToList() : [];

        await db.SaveChangesAsync(ct);

        return ServiceResult.Ok();
    }
    public async Task<ServiceResult<bool>> ToggleActiveAsync(string id, CancellationToken ct)
    {
        var user = await db.Users.SingleOrDefaultAsync(u => u.Id == id, ct);
        if (user is null) return ServiceResult<bool>.NotFound();

        user.Active = !user.Active;
        await db.SaveChangesAsync(ct);

        return ServiceResult<bool>.Ok(user.Active);
    }
}
