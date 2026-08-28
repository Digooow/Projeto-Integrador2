# Testes do backend

## Status vigente — 28/08/2026

- ✅ Projeto de testes alinhado ao `.NET 8`.
- ✅ 9 testes aprovados na última execução: 7 unitários e 2 E2E da API.
- ✅ O fluxo de autenticação JWT e a persistência publicada fazem parte da
  aplicação; novos cenários de navegador e autorização podem ser adicionados
  conforme o roadmap.

O texto abaixo permanece como a orientação original dos testes de domínio.

Os testes descrevem as regras de negócio do sistema de reserva antes da integração com o Supabase.

## Executar

```powershell
dotnet test tests/Projeto-Integrador2.Tests/Projeto-Integrador2.Tests.csproj
```

A persistência deverá ser adicionada por meio de uma implementação de repositório; estes testes não devem depender de rede ou de um banco real.
