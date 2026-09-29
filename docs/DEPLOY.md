# Deploy e operação

## Acesso aos serviços publicados

- **Aplicação em produção:** [projeto-integrador2-latest.onrender.com](https://projeto-integrador2-latest.onrender.com/)
- **Imagem publicada no Docker Hub:** [digooow/projeto-integrador2](https://hub.docker.com/layers/digooow/projeto-integrador2/latest)
- **Projeto no Supabase:** [supabase.co](https://lrduzdtfknsnuzkhrsgk.supabase.co)

## Estado do deploy

O repositório possui um `Dockerfile` multi-stage. O workflow constrói a imagem
na branch `main`, publica no Docker Hub e solicita um deploy no Render quando os
secrets de integração estão configurados. Isso comprova automação de entrega,
não disponibilidade contínua nem prontidão operacional.

## Ambientes

O desenvolvimento local usa `dotnet run`; a produção usa uma imagem Docker
publicada no Docker Hub e executada no Render. O PostgreSQL de produção é
fornecido pelo Supabase.

## Variáveis obrigatórias

| Variável | Uso |
|---|---|
| `SUPABASE_CONNECTION_STRING` | conexão com o PostgreSQL |
| `JWT_SECRET_KEY` | assinatura dos tokens; mínimo de 32 bytes |
| `PORT` | porta informada pelo ambiente de execução |

Secrets do GitHub Actions, Docker Hub e Render devem existir somente na
configuração dos serviços. Nunca colocar valores reais no repositório.

## Banco

Aplicar as migrations de `supabase/migrations` em ordem:

1. `001_initial.sql`: schema inicial e dados de base.
2. `002_frontend_integration.sql`: campos e políticas para a integração.
3. `003_jwt_authentication.sql`: dados e estruturas relacionadas à autenticação.

Executar primeiro em um ambiente de validação. Confirmar schema, índices,
seeds, constraints e políticas RLS. Manter backup e testar restauração antes de
qualquer mudança destrutiva. Backup automatizado ainda não está implementado.

## Verificação pós-deploy

1. Consultar `GET /health`.
2. Abrir `/` e `/reserva-salas.html`.
3. Fazer login com uma conta não demonstrativa.
4. Validar criação, aprovação e consulta de uma reserva de teste.
5. Conferir logs sem expor senha, token ou connection string.
6. Confirmar que o build e os 9 testes automatizados passaram antes da
   publicação.

## Operação segura

- Usar uma chave JWT aleatória diferente por ambiente.
- Trocar as senhas de demonstração antes de uso real.
- Não executar migrations destrutivas sem backup e plano de reversão.
- Validar o status do banco após cada deploy.
- Registrar o commit e a tag da imagem implantada.
- Ter acesso de emergência ao Render, Docker Hub e Supabase documentado fora do
  repositório.

Rollback, backup automatizado, rotação de secrets, alertas e monitoramento de
produção ainda precisam ser formalizados conforme o roadmap.
