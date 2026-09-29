namespace Projeto_Integrador2.Services;
public sealed class JwtSettings
{
    public string SecretKey { get; init; } = "";
    public string Issuer { get; init; } = "projeto-integrador2";
    public string Audience { get; init; } = "projeto-integrador2-users";
}
