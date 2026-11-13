# Projeto-Integrador2 — Sistema de Reserva de Salas

## Acesso ao projeto online

- **Aplicação em produção:** [projeto-integrador2-latest.onrender.com](https://projeto-integrador2-latest.onrender.com/)
- **Imagem no Docker Hub:** [digooow/projeto-integrador2](https://hub.docker.com/layers/digooow/projeto-integrador2/latest)
- **Projeto no Supabase:** [supabase.co](https://lrduzdtfknsnuzkhrsgk.supabase.co)

## Visão geral

Aplicação web para solicitar e administrar salas em instituições de ensino.
Professores e colaboradores criam reservas pontuais ou recorrentes;
coordenadores e administradores aprovam, rejeitam e cancelam solicitações.

O backend e o frontend estão integrados, com persistência em PostgreSQL/Supabase,
autenticação JWT e pipeline de entrega com Docker. O projeto está funcional
como entrega acadêmica e ainda possui pendências operacionais antes de ser
tratado como produto de produção.

## Funcionalidades principais

- Reservas pontuais e recorrentes com validação de capacidade e conflitos.
- Aprovação, rejeição e cancelamento por usuários autorizados.
- Login, cadastro de requisitantes e autorização com JWT.
- Gestão administrativa de usuários, salas e recursos.
- Paginação de reservas e painel público de reservas aprovadas.
- Persistência PostgreSQL/Supabase com migrations e RLS.
- Health check em `GET /health` e documentação OpenAPI/Swagger em `/swagger` (`/swagger/v1/swagger.json`).

### Estado atual e limites

Implementado:

- 16 testes automatizados (unitários de domínio/aplicação e E2E da API com `WebApplicationFactory`).
- Swagger/OpenAPI ativo (`/swagger` e `/swagger/v1/swagger.json`) com esquema JWT `Bearer`.
- Logs estruturados em JSON com Serilog e correlação de requisições via `X-Correlation-Id`.
- Validação de entrada com FluentValidation (`Validators/RequestValidators.cs`) nos endpoints de autenticação, reservas, salas, recursos e usuários.
- CORS restrito a origens explícitas (`CORS_ALLOWED_ORIGINS`, rejeitando wildcard `*`) e rate limiting de janela fixa nos grupos `/auth` e `/api`.
- Testes E2E compatíveis com PostgreSQL real via `TEST_POSTGRES_CONNECTION_STRING` (executados no CI contra `postgres:16-alpine`), mantendo fallback `EF Core InMemory` para execução offline.
- CRUD de salas, recursos e usuários administrativos.
- Papéis de requisitante, coordenador e administrador; o domínio também mantém
  o papel de colaborador.
- Autorização administrativa aplicada aos endpoints de gestão de salas,
  recursos e usuários.
- Coordenadores e administradores podem aprovar ou rejeitar reservas; o
  proprietário, coordenador ou administrador pode cancelar uma reserva.
- Migrations `001_initial.sql`, `002_frontend_integration.sql` e
  `003_jwt_authentication.sql`.
- Regras de negócio extraídas para serviços dedicados em `Services/`.
- Rotas extraídas para `Endpoints/ApplicationEndpoints.cs`; `Program.cs`
  permanece responsável pela composição e infraestrutura.
- Conflito de merge do `Program.cs` removido e build validado.

Ainda não tratar como concluído:

- Formalização de política de timezone e padronização de erros em `ProblemDetails`.
- Execução das migrations `supabase/migrations/*.sql` e validação de RLS contra instância Supabase real em ambiente de teste.
- Testes de concorrência sob carga e matriz completa de permissões por papel.
- Backup/restore automatizado e procedimento de rollback.

## Stack

| Camada | Tecnologia |
|---|---|
| Backend | ASP.NET Core 8 / C# / Minimal APIs |
| Domínio | Regras de negócio organizadas com DDD |
| Dados | Entity Framework Core 8 / Npgsql / PostgreSQL |
| Segurança | JWT / FluentValidation / CORS restrito / Rate Limiting |
| Contratos e Logs | Swagger (OpenAPI) / Serilog JSON + `X-Correlation-Id` |
| Frontend | HTML / CSS / JavaScript |
| Qualidade | xUnit / 16 testes automatizados |
| Entrega | Docker / GitHub Actions / Docker Hub / Render |

## Arquitetura resumida

O backend ASP.NET Core serve a API e o frontend. Os serviços de aplicação
concentram as regras de negócio e o Entity Framework Core persiste entidades no
PostgreSQL. O frontend autentica no endpoint `/auth/login`, mantém o token na
sessão e envia `Bearer` nas chamadas protegidas.

Veja detalhes em [docs/ARQUITETURA.md](./docs/ARQUITETURA.md).

## Endpoints principais

| Método e rota | Finalidade | Acesso |
|---|---|---|
| `GET /health` | Verifica conexão com o banco | Público |
| `POST /auth/login` | Emite JWT por e-mail e senha | Público |
| `POST /auth/register` | Cadastra requisitante | Público |
| `GET /api/rooms` | Lista salas ativas | Público; inativas exigem administrador |
| `POST /api/rooms` | Cria sala | Administrador |
| `PUT /api/rooms/{id}` | Atualiza sala | Administrador |
| `GET /api/resources` | Lista recursos | Público |
| `POST /api/resources` | Cria recurso | Administrador |
| `GET /api/users` | Lista usuários | Autenticado |
| `POST /api/users` | Cria usuário | Administrador |
| `PUT /api/users/{id}` | Atualiza usuário | Administrador |
| `GET /api/reservations` | Lista reservas paginadas | Público apenas para aprovadas |
| `POST /api/reservations` | Cria reserva ou série recorrente | Requisitante autenticado |
| `POST /api/reservations/{id}/approve` | Aprova reserva | Coordenador/admin |
| `POST /api/reservations/{id}/reject` | Rejeita reserva | Coordenador/admin |
| `POST /api/reservations/{id}/cancel` | Cancela reserva | Proprietário/coordenador/admin |

## Como executar

Pré-requisitos: .NET SDK 8, PostgreSQL/Supabase e estas variáveis:

```powershell
$env:SUPABASE_CONNECTION_STRING = "sua-connection-string"
$env:JWT_SECRET_KEY = "uma-chave-com-pelo-menos-32-bytes"
# Opcionais:
# $env:CORS_ALLOWED_ORIGINS = "https://seu-dominio.com"
# $env:RATE_LIMIT_AUTH_PERMITS = "20"
# $env:RATE_LIMIT_API_PERMITS = "120"
# $env:RATE_LIMIT_WINDOW_SECONDS = "60"
dotnet run
```

Acesse `http://localhost:5000/`, `http://localhost:5000/reserva-salas.html` ou a
documentação interativa OpenAPI em `http://localhost:5000/swagger`.

Nunca versionar connection strings, tokens ou credenciais reais. As migrations
estão em [supabase/migrations](./supabase/migrations).

## Testes

```powershell
# Execução local offline (fallback EF Core InMemory):
dotnet test tests/Projeto-Integrador2.Tests/Projeto-Integrador2.Tests.csproj -c Release

# Execução E2E contra PostgreSQL real (usado no CI com postgres:16-alpine):
$env:TEST_POSTGRES_CONNECTION_STRING = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgres"
dotnet test tests/Projeto-Integrador2.Tests/Projeto-Integrador2.Tests.csproj -c Release
```

A suíte executa 16 testes automatizados (regras de domínio/aplicação e testes
E2E de autenticação, fluxo de reservas, painel público, Swagger, CORS restrito,
FluentValidation, rate limiting e `X-Correlation-Id`). Quando
`TEST_POSTGRES_CONNECTION_STRING` está definida, os testes E2E provisionam bancos
efêmeros isolados no PostgreSQL; sem ela, utilizam `EF Core InMemory` como
fallback. Migrations SQL e políticas RLS no Supabase continuam fora do escopo da
suíte automática.

## Deploy

O pipeline executa restore, build em Release e os 16 testes automatizados com
container de serviço `postgres:16-alpine`. Na branch `main`, publica uma
imagem Docker e pode acionar o redeploy no Render. Consulte
[docs/DEPLOY.md](./docs/DEPLOY.md) e [docs/CI-CD.md](./docs/CI-CD.md).

O deploy atual não deve ser interpretado como certificação de produção. A
padronização em `ProblemDetails`, política formal de timezone, validação de RLS
em Supabase de teste, backups, alertas e rollback seguem os critérios de
[docs/ROADMAP.md](./docs/ROADMAP.md).

## Documentação

- [Arquitetura](./docs/ARQUITETURA.md) — componentes, fluxos, responsabilidades e SOLID.
- [Deploy](./docs/DEPLOY.md) — variáveis, migrations e operação.
- [CI/CD](./docs/CI-CD.md) — pipeline, imagens e critérios de qualidade.
- [Roadmap](./docs/ROADMAP.md) — evolução para um produto profissional.
- [Histórico](./docs/HISTORICO.md) — resumo da evolução do projeto.
- [Metodologia ágil](./docs/METODOLOGIA_AGIL.md) — processo de desenvolvimento.
- [Slides](./docs/NOVA_APRESENTACAO_SLIDES.md) — material de apresentação.
