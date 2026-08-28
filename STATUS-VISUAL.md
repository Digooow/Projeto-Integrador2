# Status visual do projeto

## Estado vigente — 28/08/2026

| Componente | Situação |
|---|---|
| Backend ASP.NET | ✅ Publicado e funcional |
| Frontend | ✅ Servido pelo ASP.NET no Render |
| Supabase | ✅ Projeto próprio configurado |
| Migrations | ✅ `001`, `002` e `003` executadas |
| Salas e reservas | ✅ Endpoints retornando `200` |
| Login | ✅ JWT por e-mail e senha |
| Cadastro público | ✅ Cria requisitante |
| Administração | ✅ Cria e edita usuários |
| CI/CD | ✅ Testa, publica Docker e dispara redeploy Render |

### Verificações de acesso

- Frontend: `/` e `/reserva-salas.html`.
- Health check: `/health`.
- API: `/api/rooms` e `/api/reservations` retornam `200`.
- O painel público não exige login; reservas e administração usam JWT.

### Segurança operacional

- `RENDER_API_KEY` e `RENDER_SERVICE_ID` são secrets do GitHub Actions.
- A connection string do Supabase está somente no ambiente do Render.
- `Troque-me-123!` é uma senha demo temporária e deve ser substituída.

## Próximas melhorias

Consulte o [roadmap único](./ROADMAP-ATUAL.md) para testes adicionais,
endurecimento operacional e evoluções de produto. Este arquivo não mantém
cronogramas ou diagnósticos históricos duplicados.

## Links

- [README](./README.md)
- [Integração frontend/backend](./INTEGRACAO-FRONTEND-BACKEND.md)
- [CI/CD](./.github/CI-CD-IMPROVEMENTS.md)
