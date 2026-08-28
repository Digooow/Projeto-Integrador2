# Ocupa — Sistema de Reserva de Salas

## Estado atual — 28/08/2026

O backend ASP.NET Core serve o frontend e está publicado no Render. O projeto
Supabase próprio está configurado e as migrations `001`, `002` e `003` foram
executadas. Salas e reservas respondem `200`, e o login JWT por e-mail e senha
está funcionando.

Também estão disponíveis:

- cadastro público de requisitantes;
- criação e edição de usuários pela administração;
- reservas pontuais e recorrentes, aprovação, rejeição e cancelamento;
- cadastro de salas e recursos;
- painel público;
- GitHub Actions com testes, publicação Docker e redeploy automático do Render.

O roadmap vigente está em [ROADMAP-ATUAL.md](./ROADMAP-ATUAL.md). Não use
diagnósticos ou cronogramas antigos como status do produto.

## Arquitetura

```text
Frontend HTML/CSS/JavaScript
          │
          ▼
ASP.NET Core 8 Minimal API ── EF Core/Npgsql ── PostgreSQL (Supabase)
          │
          └── Render (container Docker)
```

Principais diretórios:

- `Domain/`: regras de negócio;
- `Persistence/`: entidades e `DbContext`;
- `frontend/`: telas do sistema;
- `supabase/migrations/`: schema e políticas do banco;
- `tests/Projeto-Integrador2.Tests/`: testes automatizados;
- `.github/workflows/`: pipeline de CI/CD.

## Autenticação e autorização

`POST /auth/login` recebe e-mail e senha e retorna um token JWT. O frontend
mantém o token na sessão e o envia como `Authorization: Bearer` nas chamadas
protegidas. O cadastro público cria um requisitante; operações administrativas
de criação/edição de usuários exigem autorização.

A senha demo `Troque-me-123!` é temporária e deve ser trocada. Nunca armazene
senhas ou tokens em documentação.

## Banco de dados e segredos

As migrations são aplicadas no projeto Supabase próprio:

```text
supabase/migrations/001_initial.sql
supabase/migrations/002_frontend_integration.sql
supabase/migrations/003_jwt_authentication.sql
```

A connection string do Supabase fica **somente** como segredo de ambiente do
serviço no Render. Não a coloque em `.env`, exemplos, código ou no repositório.

## Executar e testar

Com o SDK .NET 8 instalado:

```powershell
dotnet run
dotnet test tests/Projeto-Integrador2.Tests/Projeto-Integrador2.Tests.csproj
```

O frontend principal é `frontend/reserva-salas.html`. Em produção, ele é servido
pelas rotas `/` e `/reserva-salas.html` do serviço publicado.

## Endpoints principais

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/health` | Health check |
| `POST` | `/auth/login` | Login por e-mail e senha |
| `POST` | `/auth/register` | Cadastro público de requisitante |
| `GET` | `/api/rooms` | Lista salas |
| `GET` | `/api/reservations` | Lista reservas paginadas |
| `POST` | `/api/reservations` | Cria reserva |
| `POST` | `/api/reservations/{id}/approve` | Aprova reserva |
| `POST` | `/api/reservations/{id}/reject` | Rejeita reserva |
| `POST` | `/api/reservations/{id}/cancel` | Cancela reserva |
| `GET/POST/PUT` | `/api/users` | Consulta, cria e edita usuários |

## CI/CD e publicação

Um push na `main` executa testes, constrói e publica a imagem Docker. Em seguida,
o workflow dispara o redeploy do serviço Render usando os secrets:

- `DOCKER_USERNAME` e `DOCKER_PASSWORD`;
- `RENDER_API_KEY`;
- `RENDER_SERVICE_ID`.

Os detalhes do pipeline estão em
[.github/CI-CD-IMPROVEMENTS.md](./.github/CI-CD-IMPROVEMENTS.md).

## Documentação relacionada

- [Roadmap atual](./ROADMAP-ATUAL.md)
- [Status visual](./STATUS-VISUAL.md)
- [Integração frontend/backend](./INTEGRACAO-FRONTEND-BACKEND.md)
- [Análise técnica](./ANALISE-PROJETO.md)
- [Testes](./tests/Projeto-Integrador2.Tests/README.md)
