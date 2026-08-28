# Ocupa — Documentação técnica atualizada

## Estado vigente — 28/08/2026

O protótipo original usava `window.storage`. Esse texto é histórico e não descreve mais a arquitetura vigente.

Atualmente, o frontend chama a API ASP.NET Core e usa dados locais de demonstração somente quando a API não responde. A API persiste no PostgreSQL/Supabase, valida capacidade e autorização, oferece login JWT ponta a ponta e pagina reservas.

O fluxo de login do frontend solicita e-mail e senha, chama `/auth/login` e envia
`Authorization: Bearer` nas chamadas protegidas.

## Estado operacional
- As migrations `001`, `002` e `003` foram executadas no Supabase.
- O frontend está publicado no Render; salas e reservas retornam `200`.
- A senha demo `Troque-me-123!` é temporária e deve ser trocada.
- Melhorias futuras (E2E de navegador, CORS, fuso horário, notificações e
  integrações externas) estão no [roadmap atual](../../ROADMAP-ATUAL.md).
