# Testes do backend

## Status vigente — 26/08/2026

- ✅ Projeto de testes alinhado ao `.NET 8`.
- ✅ 8 testes aprovados na última execução: 7 unitários e 1 E2E da API.
- ⏳ Testes de integração/E2E da API, autenticação e persistência real ainda não foram implementados.

O texto abaixo permanece como a orientação original dos testes de domínio.

Os testes descrevem as regras de negócio do sistema de reserva antes da integração com o Supabase.

## Executar

```powershell
dotnet test tests/Projeto-Integrador2.Tests/Projeto-Integrador2.Tests.csproj
```

A persistência deverá ser adicionada por meio de uma implementação de repositório; estes testes não devem depender de rede ou de um banco real.
