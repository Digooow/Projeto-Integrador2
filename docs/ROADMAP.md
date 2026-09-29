# Roadmap de evolução profissional

**Atualizado em:** 28/09/2026

Este documento contém apenas trabalho futuro. Itens já concluídos foram
removidos da lista. O estado atual está no [README.md](../README.md) e a
evolução passada em [HISTORICO.md](./HISTORICO.md).

## Concluído nesta etapa

- Resolver o conflito de merge no `Program.cs`.
- Extrair as regras de negócio dos endpoints para serviços dedicados.
- Extrair o mapeamento das rotas para `Endpoints/ApplicationEndpoints.cs`.
- Aplicar autorização administrativa aos endpoints de gestão de salas,
  recursos e usuários.
- Validar o projeto com build e 9 testes automatizados.
- Remover comentários dos arquivos de código atuais.

## Fase 1 — Segurança e dados

- Restringir CORS aos domínios oficiais por ambiente.
- Separar configurações de desenvolvimento, teste e produção.
- Remover credenciais demonstrativas da produção e documentar rotação de secrets.
- Formalizar e testar a matriz de permissões por endpoint e papel.
- Definir política de UTC, timezone da instituição e exibição no frontend.
- Validar migrations, índices, constraints e RLS em ambiente de teste.
- Garantir que operações concorrentes não criem conflito de sala.

**Critério:** matriz de permissões, cenários de timezone e concorrência cobertos
por testes automatizados.

## Fase 2 — Qualidade, contratos e observabilidade

- Ativar validação centralizada e respostas `ProblemDetails`.
- Publicar OpenAPI/Swagger com autenticação, paginação e erros documentados.
- Configurar Serilog, correlação de requisições e métricas de latência, erros,
  autenticação e conflitos.
- Evoluir `/health` para diagnosticar dependências.
- Ampliar E2E para login, cadastro, tokens inválidos/expirados e papéis.
- Adicionar testes de RLS, migrations, paginação e falhas do banco.
- Documentar contratos de request/response e exemplos de erro para o frontend.
- Adicionar testes de arquitetura para impedir acesso direto ao `DbContext`
  pelos endpoints.
- Avaliar interfaces para portas externas somente quando houver múltiplas
  implementações ou necessidade de substituição em testes.

**Critério:** o pipeline detectar regressões e produzir diagnóstico suficiente
para investigar uma falha.

## Fase 3 — Resiliência e entrega

- Adicionar rate limiting, limites de payload, timeouts e tratamento global de
  exceções.
- Automatizar backup, restauração e plano de recuperação do Supabase.
- Fixar imagens por commit/digest e escanear dependências e containers.
- Documentar rollback do Render e resposta a incidentes.
- Definir SLOs básicos para disponibilidade, latência e taxa de erro.

**Critério:** uma versão com falha poder ser detectada, revertida e recuperada
por procedimento testado.

## Fase 4 — Produto

- Notificações de aprovação, rejeição, cancelamento e conflito.
- Calendário, filtros e relatórios de ocupação.
- Auditoria de alterações administrativas.
- Acessibilidade, responsividade e mensagens de erro mais claras.

Esses itens devem começar depois das fases de segurança, dados e observabilidade.

## Sequência recomendada

1. CORS, secrets, autorização e timezone.
2. RLS, migrations e concorrência.
3. Validação, `ProblemDetails` e OpenAPI.
4. E2E, logs, métricas, health check e alertas.
5. Backup, rollback, rate limiting e scans.
6. Funcionalidades de produto.

## Critério de maturidade

O projeto estará profissional quando tiver deploy reproduzível e reversível,
autorização testada, dados protegidos, observabilidade sem vazamento de
segredos, testes dos fluxos críticos e procedimentos de backup e incidente.
