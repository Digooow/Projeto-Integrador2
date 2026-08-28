# Ocupa — Documentação técnica atualizada

## Estado vigente — 28/08/2026

O protótipo original usava `window.storage`. Esse texto é histórico e não descreve mais a arquitetura vigente.

Atualmente, o frontend chama a API ASP.NET Core e usa dados locais de demonstração somente quando a API não responde. A API persiste no PostgreSQL/Supabase, valida capacidade e autorização, oferece login JWT no backend e pagina reservas.

O fluxo visual de login do frontend ainda seleciona um usuário da lista e não envia `Authorization: Bearer`; portanto, a autenticação ainda não está integrada de ponta a ponta.

## Pendências
- Integrar o login JWT ao frontend.
- Aplicar e validar a migration 002 no Supabase.
- Publicar a imagem atualizada e fazer redeploy no Render.
- Criar testes E2E, restringir CORS e tratar fuso horário explicitamente.
- Implementar notificações e integrações externas, se priorizadas.
