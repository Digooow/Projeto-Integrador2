# Roadmap atual — Ocupa

**Atualizado em:** 28/08/2026

Este arquivo complementa os documentos históricos do projeto. Nenhum registro
anterior foi removido; esta é a referência para o estado atual e os próximos
passos.

## Evolução concluída

### Integração e persistência

- Frontend HTML/CSS/JavaScript servido pelo ASP.NET Core no Render.
- Banco PostgreSQL próprio no Supabase conectado por variável de ambiente.
- Migrations 001, 002 e 003 executadas no banco atual.
- Salas, recursos e reservas consultados com sucesso pela API publicada.

### Autenticação e usuários

- Login JWT por e-mail e senha funcionando.
- Cadastro público de requisitantes funcionando.
- Administração de usuários com criação, edição e ativação/desativação.
- Usuários de demonstração disponíveis após a migration 002/003.

### Publicação e validação

- GitHub Actions executa build e testes.
- Imagem Docker é publicada no Docker Hub.
- Redeploy do Render é acionado automaticamente pelos secrets
  `RENDER_API_KEY` e `RENDER_SERVICE_ID`.
- Endpoint `/health`, frontend e consultas de salas/reservas foram validados em
  produção.
- Corrigido o erro de sintaxe que deixava a tela de login cinza.

## Próximas etapas

1. Criar testes E2E para login, cadastro e permissões por papel.
2. Restringir CORS ao frontend publicado.
3. Tratar explicitamente fuso horário em datas e horários.
4. Adicionar logs estruturados, métricas e rate limiting.
5. Trocar a senha de demonstração antes de qualquer uso real.
6. Avaliar notificações, relatórios e integrações externas.

## Referências

- [README principal](./README.md)
- [Integração frontend/backend](./INTEGRACAO-FRONTEND-BACKEND.md)
- [Status visual](./STATUS-VISUAL.md)
- [Roadmap histórico](./ROADMAP-ACOES.md)
