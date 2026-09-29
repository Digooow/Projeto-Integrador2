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

O build e os testes são a barreira mínima para publicação. A imagem recebe tags
de branch, commit e `latest`. A suíte atual possui 9 testes e valida o projeto
após a separação entre endpoints e serviços.

## Gatilhos e comportamento

- `push` em `main` ou `develop`: executa build e testes.
- Pull request para `main`: executa build e testes.
- Publicação Docker e deploy Render: somente `main`, após o job de build/testes.
- Resultados TRX são enviados como artefato mesmo quando os testes falham.

## Secrets necessários

- `DOCKER_USERNAME`
- `DOCKER_PASSWORD`
- `RENDER_API_KEY`
- `RENDER_SERVICE_ID`

Usar tokens de acesso com o menor privilégio possível e revisar sua rotação.

## O que o pipeline ainda não garante

- Não executa migrations contra um Supabase de teste.
- Não valida CORS, RLS ou concorrência em banco PostgreSQL real.
- Não faz scan de vulnerabilidades atualmente.
- Não possui aprovação manual, rollback automático ou alertas de produção.
- A cobertura ainda não é coletada apesar do nome histórico de algumas etapas.

## Evolução recomendada

- Fixar o deploy por digest ou SHA, mantendo `latest` apenas como conveniência.
- Adicionar scan de dependências e da imagem Docker.
- Publicar cobertura e resultados E2E como artefatos.
- Adicionar aprovação para produção e procedimento de rollback.
- Configurar alertas para falhas de build, deploy e health check.
