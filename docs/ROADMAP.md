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
- Configurar Swagger/OpenAPI (`/swagger`), logging estruturado JSON via Serilog com `X-Correlation-Id`, FluentValidation em `Validators/RequestValidators.cs`, CORS restrito (`CORS_ALLOWED_ORIGINS`, sem wildcard `*`) e rate limiting por janela fixa.
- Preparar testes E2E com suporte condicional a PostgreSQL real (`TEST_POSTGRES_CONNECTION_STRING`) no GitHub Actions (`postgres:16-alpine`), mantendo fallback `InMemory` offline.
- Validar o projeto com build e 16 testes automatizados.
- Remover comentários dos arquivos de código atuais.

## Fase 1 — Segurança e dados

- Validar CORS por ambiente com testes de deploy no domínio final de produção.
- Remover credenciais demonstrativas da produção e documentar rotação de secrets.
- Formalizar e testar a matriz completa de permissões por endpoint e papel.
- Definir política de UTC, timezone da instituição e exibição no frontend.
- Validar migrations (`supabase/migrations/*.sql`), índices, constraints e RLS em ambiente Supabase de teste.
- Garantir que operações concorrentes não criem conflito de sala sob carga.

**Critério:** matriz de permissões, cenários de timezone, RLS e concorrência cobertos
por testes automatizados.

## Fase 2 — Qualidade, contratos e observabilidade

- Padronizar respostas de erro e validação no formato `ProblemDetails` (RFC 9457).
- Expandir a documentação OpenAPI com exemplos de resposta e códigos de erro paginados.
- Adicionar exportação de métricas/tracing distribuído (OpenTelemetry) além dos logs JSON estruturados.
- Evoluir `/health` para diagnosticar dependências externas e latência do banco.
- Ampliar E2E para tokens expirados e todos os papéis/fluxos administrativos.
- Adicionar testes de RLS, migrations e falhas de conexão do banco.
- Documentar contratos de request/response e exemplos de erro para o frontend.
- Adicionar testes de arquitetura para impedir acesso direto ao `DbContext`
  pelos endpoints.
- Avaliar interfaces para portas externas somente quando houver múltiplas
  implementações ou necessidade de substituição em testes.

**Critério:** o pipeline detectar regressões e produzir diagnóstico suficiente
para investigar uma falha.

## Fase 3 — Resiliência e entrega

- Adicionar limites de payload, timeouts e tratamento global de exceções (em complemento ao rate limiting já ativo).
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

1. Política de timezone, matriz de permissões por endpoint e validação com secrets por ambiente.
2. RLS, migrations no Supabase e concorrência sob carga.
3. `ProblemDetails` e expansão de contratos OpenAPI.
4. Métricas/tracing distribuído, health check avançado e alertas.
5. Backup, rollback, tratamento global de exceções e scans.
6. Funcionalidades de produto.

## Critério de maturidade

O projeto estará profissional quando tiver deploy reproduzível e reversível,
autorização testada, dados protegidos, observabilidade sem vazamento de
segredos, testes dos fluxos críticos e procedimentos de backup e incidente.
