# Relatório de consistência dos materiais

## Escopo

Este documento registra correções de consistência dos materiais acadêmicos em
`Arquivos/`. O status técnico vigente está no
[ROADMAP-ATUAL.md](./ROADMAP-ATUAL.md); este relatório não é um segundo
roadmap.

## Texto técnico vigente para os materiais

O Ocupa possui backend ASP.NET Core 8, persistência PostgreSQL em Supabase
próprio e frontend integrado, servido pelo backend no Render. O produto oferece
reservas pontuais e recorrentes, aprovação, rejeição, cancelamento, validação de
capacidade, detecção de conflitos, paginação, cadastro de salas/recursos,
cadastro público de requisitantes e administração de usuários.

O login por e-mail e senha funciona com JWT no frontend e no backend. O painel
público não exige login; operações de reserva e administração exigem
autorização. As migrations `001`, `002` e `003` foram executadas e os endpoints
de salas e reservas retornam `200`.

## Itens que não devem ser declarados como entregues

Notificações por WhatsApp/e-mail/push, matrícula de alunos, consulta por curso,
troca de sala pelo professor, check-in por QR Code, relatórios avançados,
integrações de calendário e sugestão automática de sala são requisitos ou
evoluções futuras, não funcionalidades comprovadas do incremento atual.

## Correções por material

### Aulas 01 e 02

Manter o nome **Ocupa — Sistema de Reserva de Salas** e descrever como escopo
as reservas, recorrência, decisões, cadastro, calendário, histórico e painel
público. Registrar os demais itens como fora do escopo atual.

### Aula 03

O quadro de 21/08/2026 é histórico. As entregas posteriores incluem integração
frontend/backend, persistência, CRUD, recorrência, decisões, paginação e
autenticação JWT ponta a ponta.

### Aulas 04 e 05

Declarar apenas entregas sustentadas por código, testes ou registros reais.
Cerimônias, validações externas e responsáveis devem ser preenchidos somente
com evidências do grupo.

### Aula 06

O protótipo baseado em `window.storage` é histórico. A documentação vigente
deve mencionar API ASP.NET Core, Supabase, Render, JWT, migrations executadas e
fallback local apenas para demonstração quando a API não responde.

## Regras de segurança documental

- A connection string do Supabase fica exclusivamente no ambiente do Render.
- Não incluir tokens, senhas ou secrets em materiais versionados.
- `Troque-me-123!` é somente uma senha demo temporária e deve ser trocada.
- Não reutilizar diagnósticos que indiquem Render quebrado, autenticação
  pendente ou migrations/deploy não realizados.
