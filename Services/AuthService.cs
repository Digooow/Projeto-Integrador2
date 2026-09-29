using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Projeto_Integrador2.Domain;
using Projeto_Integrador2.Models;
using Projeto_Integrador2.Persistence;
using Projeto_Integrador2.Security;

namespace Projeto_Integrador2.Services;
public sealed class AuthService(ReservationDbContext db, JwtSettings jwtSettings)
{
    public async Task<LoginResponse?> LoginAsync(string email, string password, CancellationToken ct)
    {
        var user = await db.Users
            .SingleOrDefaultAsync(u => u.Email == email && u.Active, ct);

        if (user is null || user.PasswordHash is null || !PasswordHasher.Verify(password, user.PasswordHash))
            return null;

        var expiresAt = DateTime.UtcNow.AddHours(8);
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

        return new LoginResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt,
            new UserResponse(user.Id, user.Name, user.Email, user.Role.ToString(), user.Active, [.. user.Floors]));
    }
    public async Task<ServiceResult<string>> RegisterAsync(
        string name, string email, string password, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return ServiceResult<string>.BadRequest("Informe nome, e-mail e senha.");

        if (password.Length < 8)
            return ServiceResult<string>.BadRequest("A senha precisa ter pelo menos 8 caracteres.");

        var normalizedEmail = email.Trim().ToLowerInvariant();

        if (await db.Users.AnyAsync(u => u.Email == normalizedEmail, ct))
            return ServiceResult<string>.Conflict("Já existe um usuário com esse e-mail.");

        var user = new UserEntity
        {
            Id = $"user_{Guid.NewGuid():N}",
            Name = name.Trim(),
            Email = normalizedEmail,
            PasswordHash = PasswordHasher.Hash(password),
            Role = UserRole.Teacher,
            Active = true,
            Floors = []
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        return ServiceResult<string>.Ok(user.Id);
    }
}
