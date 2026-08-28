# Roadmap atual — Ocupa

**Atualizado em:** 28/08/2026

Este é o único roadmap vigente do projeto. Os demais documentos devem registrar
arquitetura, decisões técnicas ou histórico, sem duplicar status e cronogramas.

## Estado entregue

- ✅ Frontend ASP.NET servido no Render.
- ✅ Projeto Supabase próprio configurado.
- ✅ Migrations `001`, `002` e `003` executadas no Supabase.
- ✅ `GET /api/rooms` e `GET /api/reservations` retornam `200`.
- ✅ Login JWT por e-mail e senha funcionando.
- ✅ Cadastro público cria usuários com papel de requisitante.
- ✅ Administração cria e edita usuários.
- ✅ GitHub Actions executa testes, publica a imagem Docker e dispara o redeploy
  no Render quando `RENDER_API_KEY` e `RENDER_SERVICE_ID` estão configurados.
- ✅ A connection string do Supabase fica somente como segredo do serviço no
  Render; não deve ser documentada em arquivos locais ou versionada.

## Operação e credenciais

- A senha demo `Troque-me-123!` é temporária e deve ser trocada após o primeiro
  acesso. Ela não deve ser usada como credencial permanente.
- O painel público pode ser consultado sem login. Operações de reserva e
  administração usam o token JWT.
- Para diagnosticar o ambiente publicado, consulte o health check, as respostas
  `200` de salas e reservas e os logs do serviço no Render. Não usar fallback
  local como evidência de disponibilidade da API.

## Próximas melhorias

Estas tarefas não bloqueiam as entregas acima e devem ser priorizadas conforme a
necessidade do produto:

1. Ampliar testes automatizados do navegador e cenários de autorização.
2. Endurecer a configuração operacional (CORS, rate limiting, observabilidade e
   tratamento explícito de fuso horário).
3. Avaliar notificações, integrações externas e relatórios avançados como
   evoluções de produto.
4. Revisar periodicamente a rotação de segredos e a substituição das credenciais
   de demonstração.

## Referências

- [README](./README.md)
- [Status visual](./STATUS-VISUAL.md)
- [Integração frontend/backend](./INTEGRACAO-FRONTEND-BACKEND.md)
- [CI/CD](./.github/CI-CD-IMPROVEMENTS.md)
