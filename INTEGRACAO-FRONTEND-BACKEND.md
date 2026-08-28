# Integração Frontend ↔ Backend

## Estado vigente — 28/08/2026

O frontend em `frontend/reserva-salas.html` é servido pelo backend ASP.NET nas
rotas `/` e `/reserva-salas.html` e usa a API publicada no Render. O Supabase
próprio está configurado e as migrations `001`, `002` e `003` foram executadas.

Salas e reservas retornam `200`. O login envia e-mail e senha para
`/auth/login`, guarda o JWT na sessão e envia `Authorization: Bearer` nas
operações protegidas.

## Funcionalidades integradas

- reservas pontuais e recorrentes;
- aprovação, rejeição e cancelamento;
- cadastro e edição administrativa de usuários;
- cadastro de salas e recursos;
- cadastro público de requisitantes;
- paginação de reservas;
- painel público sem login.

## Endpoints principais

| Método | Rota | Finalidade |
|---|---|---|
| `POST` | `/auth/login` | Login por e-mail e senha |
| `POST` | `/auth/register` | Cadastro público de requisitante |
| `GET` | `/api/users` | Listar usuários (admin) |
| `POST` | `/api/users` | Criar usuário (admin) |
| `PUT` | `/api/users/{id}` | Editar usuário (admin) |
| `GET` | `/api/rooms` | Listar salas |
| `GET` | `/api/resources` | Listar recursos |
| `POST` | `/api/reservations` | Criar reserva |
| `POST` | `/api/reservations/{id}/approve` | Aprovar reserva |
| `POST` | `/api/reservations/{id}/reject` | Rejeitar reserva |
| `POST` | `/api/reservations/{id}/cancel` | Cancelar reserva |

## Persistência e segurança

O backend usa EF Core/Npgsql para o PostgreSQL do Supabase. A connection string
fica exclusivamente como segredo do serviço no Render; não deve ser copiada
para este repositório ou para arquivos locais.

O fallback local do frontend serve apenas para demonstração quando a API não
responde e não é evidência de disponibilidade do ambiente publicado.

## Execução local

```powershell
dotnet run
dotnet test tests/Projeto-Integrador2.Tests/Projeto-Integrador2.Tests.csproj
```

Para detalhes do estado e das próximas melhorias, consulte o
[roadmap atual](./ROADMAP-ATUAL.md).
