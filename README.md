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
- Health check em `GET /health`.

### Estado atual e limites

Implementado:

- 9 testes automatizados, incluindo testes unitários de domínio e testes E2E da API.
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

- CORS restrito, timezone formal, rate limiting e observabilidade avançada.
- Swagger/OpenAPI, Serilog e FluentValidation efetivamente configurados; os
  pacotes estão referenciados, mas os recursos ainda não estão ativos.
- Cobertura E2E específica para login, cadastro e todas as permissões.
- Backup/restore automatizado e procedimento de rollback.

## Stack

| Camada | Tecnologia |
|---|---|
| Backend | ASP.NET Core 8 / C# / Minimal APIs |
| Domínio | Regras de negócio organizadas com DDD |
| Dados | Entity Framework Core 8 / Npgsql / PostgreSQL |
| Segurança | JWT / hash de senha / RLS |
| Frontend | HTML / CSS / JavaScript |
| Qualidade | xUnit / 9 testes automatizados |
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
dotnet run
```

Acesse `http://localhost:5000/` ou
`http://localhost:5000/reserva-salas.html`.

Nunca versionar connection strings, tokens ou credenciais reais. As migrations
estão em [supabase/migrations](./supabase/migrations).

## Testes

```powershell
dotnet test tests/Projeto-Integrador2.Tests/Projeto-Integrador2.Tests.csproj
```

Os testes E2E substituem o banco por EF Core InMemory e exercitam login, criação,
aprovação e consulta de reservas. Eles não comprovam o comportamento de um
Supabase real; por isso migrations, RLS e concorrência continuam no roadmap.

## Deploy

O pipeline executa restore, build e testes. Na branch `main`, publica uma
imagem Docker e pode acionar o redeploy no Render. Consulte
[docs/DEPLOY.md](./docs/DEPLOY.md) e [docs/CI-CD.md](./docs/CI-CD.md).

O deploy atual não deve ser interpretado como certificação de produção. A
validação de domínio, secrets, CORS, backups, alertas e rollback ainda precisa
seguir os critérios de [docs/ROADMAP.md](./docs/ROADMAP.md).

## Documentação

- [Arquitetura](./docs/ARQUITETURA.md) — componentes, fluxos, responsabilidades e SOLID.
- [Deploy](./docs/DEPLOY.md) — variáveis, migrations e operação.
- [CI/CD](./docs/CI-CD.md) — pipeline, imagens e critérios de qualidade.
- [Roadmap](./docs/ROADMAP.md) — evolução para um produto profissional.
- [Histórico](./docs/HISTORICO.md) — resumo da evolução do projeto.
- [Metodologia ágil](./docs/METODOLOGIA_AGIL.md) — processo de desenvolvimento.
- [Slides](./docs/NOVA_APRESENTACAO_SLIDES.md) — material de apresentação.
