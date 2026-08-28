# Análise técnica do projeto

## Estado atual — 28/08/2026

O Ocupa é uma aplicação ASP.NET Core 8 com frontend servido pelo próprio
backend, persistência PostgreSQL em um projeto Supabase próprio e publicação em
container no Render.

### Entregas confirmadas

- Frontend disponível nas rotas `/` e `/reserva-salas.html`.
- Migrations `001`, `002` e `003` executadas no Supabase.
- Salas e reservas acessíveis com respostas `200`.
- Reservas pontuais e recorrentes, validação de capacidade, detecção de
  conflitos, aprovação, rejeição e cancelamento.
- Login JWT por e-mail e senha, com autorização por token.
- Cadastro público de requisitante.
- Administração com criação e edição de usuários.
- Paginação de reservas.
- Testes, publicação Docker e redeploy Render automatizados pelo GitHub Actions.

## Arquitetura

```text
frontend/                  HTML, CSS e JavaScript
Program.cs                 Minimal API, autenticação e autorização
Domain/                    regras de reserva
Persistence/               entidades e EF Core/Npgsql
supabase/migrations/       schema, RLS e autenticação
tests/                     testes automatizados
```

O painel público é anônimo. Criação/alteração de reservas e administração são
operações protegidas por JWT. O cadastro público cria apenas um requisitante;
alteração de papéis e usuários permanece sob administração.

## Banco e configuração

As migrations versionadas são:

1. `001_initial.sql`;
2. `002_frontend_integration.sql`;
3. `003_jwt_authentication.sql`.

A connection string do Supabase é um segredo configurado exclusivamente no
serviço Render. Ela não deve aparecer em documentação, arquivos de exemplo,
código ou commits.

## CI/CD

O workflow em `.github/workflows/dotnet.yml` executa testes, constrói/publica a
imagem Docker e dispara o redeploy do Render. O deploy remoto usa os secrets
`RENDER_API_KEY` e `RENDER_SERVICE_ID`; credenciais do Docker Hub são usadas
para publicar a imagem.

## Operação e credenciais

A senha `Troque-me-123!` existe somente como credencial demo temporária e deve
ser trocada. Não tratar essa senha como configuração permanente.

## Melhorias recomendadas

As entregas acima não devem ser reabertas. Melhorias futuras podem incluir mais
testes E2E de navegador, CORS restrito, rate limiting, observabilidade,
tratamento explícito de fuso horário, notificações e relatórios. Elas estão
organizadas no [ROADMAP-ATUAL.md](./ROADMAP-ATUAL.md).
