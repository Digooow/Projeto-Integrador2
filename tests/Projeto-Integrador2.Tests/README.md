# Testes do backend

Os testes descrevem as regras de negócio do sistema de reserva antes da integração com o Supabase.

## Executar

```powershell
dotnet test tests/Projeto-Integrador2.Tests/Projeto-Integrador2.Tests.csproj
```

A persistência deverá ser adicionada por meio de uma implementação de repositório; estes testes não devem depender de rede ou de um banco real.
