# CI/CD

## Objetivo

O pipeline reduz erros manuais entre alteração de código, teste, publicação da
imagem e redeploy. Ele não substitui revisão de código, validação de segurança,
testes contra Supabase ou aprovação de produção.

## Pipeline atual

O workflow em `.github/workflows/dotnet.yml`:

1. restaura dependências;
2. compila em Release;
3. executa os testes e publica resultados;
4. na branch `main`, constrói e publica a imagem Docker;
5. aciona o deploy do Render quando os secrets necessários existem.

O pipeline executa build em Release e os 16 testes automatizados (unitários +
E2E da API contra PostgreSQL 16 em container de serviço). A imagem recebe tags
de branch, commit e `latest`.

## Gatilhos e comportamento

- `push` em `main` ou `develop`: sobe serviço `postgres:16-alpine` e executa build e testes (`TEST_POSTGRES_CONNECTION_STRING`).
- Pull request para `main`: sobe serviço `postgres:16-alpine` e executa build e testes (`TEST_POSTGRES_CONNECTION_STRING`).
- Publicação Docker e deploy Render: somente `main`, após o job de build/testes.
- Resultados TRX são enviados como artefato mesmo quando os testes falham.

## Secrets necessários

- `DOCKER_USERNAME`
- `DOCKER_PASSWORD`
- `RENDER_API_KEY`
- `RENDER_SERVICE_ID`

Usar tokens de acesso com o menor privilégio possível e revisar sua rotação.

## O que o pipeline ainda não garante

- Não executa os scripts SQL de `supabase/migrations/` nem valida políticas RLS em instância Supabase de teste (os testes E2E provisionam o schema PostgreSQL via EF Core `EnsureCreated()`).
- Não faz scan de vulnerabilidades de dependências ou da imagem Docker atualmente.
- Não possui aprovação manual, rollback automático ou alertas de produção.
- A cobertura ainda não é coletada apesar do nome histórico de algumas etapas.

## Evolução recomendada

- Fixar o deploy por digest ou SHA, mantendo `latest` apenas como conveniência.
- Adicionar scan de dependências e da imagem Docker.
- Publicar cobertura e resultados E2E como artefatos.
- Adicionar aprovação para produção e procedimento de rollback.
- Configurar alertas para falhas de build, deploy e health check.
