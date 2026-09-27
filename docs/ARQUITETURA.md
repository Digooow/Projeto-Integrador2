# Arquitetura

## Contexto

O sistema atual é uma aplicação ASP.NET Core 8 que serve simultaneamente a API
e o arquivo `frontend/reserva-salas.html`. Não existe um frontend React
separado nem um backend Node.js: essas tecnologias aparecem em materiais
acadêmicos antigos, não na implementação versionada.

## Visão de componentes

```text
Navegador
   │
   ├── Frontend HTML/CSS/JavaScript
   │       └── /auth/login e /api/*
   │
   └── ASP.NET Core Minimal API
           ├── autenticação JWT e autorização por papel
           ├── regras de reserva e recorrência
           └── Entity Framework Core / Npgsql
                   └── PostgreSQL no Supabase
```

O frontend é servido pelo backend nas rotas `/` e `/reserva-salas.html`.
Endpoints protegidos exigem `Authorization: Bearer <token>`. A consulta de
reservas aprovadas alimenta o painel público.

## Camadas e responsabilidades

### API e composição

`Program.cs` registra o `ReservationDbContext`, autenticação JWT, autorização,
CORS, rotas HTTP e o middleware que serve o frontend. A API usa Minimal APIs,
recebe DTOs via JSON e retorna respostas HTTP com objetos anônimos ou records.

### Domínio

`Domain/ReservationDomain.cs` contém `ReservationService`, as entidades de
domínio, estados de reserva, papéis, expansão de recorrência e regras de
capacidade, aprovação, conflito e cancelamento. Essa camada não depende de
PostgreSQL ou de HTTP.

### Persistência

`Persistence/Entities.cs` mapeia usuários, salas, recursos, reservas e
ocorrências. Uma série recorrente é identificada por `SeriesId`, mas cada
ocorrência possui sua própria reserva e horário. `ReservationDbContext` define
relacionamentos e convenções de nomes.

### Segurança

O login verifica o hash da senha e emite JWT com identidade, e-mail e papel.
Endpoints administrativos usam autorização por papel. A connection string e a
chave de assinatura são obrigatórias por variável de ambiente.

## Organização do código

- `Program.cs`: composição da aplicação e endpoints HTTP.
- `Domain/`: regras e modelos do domínio de reservas.
- `Persistence/`: entidades e `ReservationDbContext`.
- `Security/`: hash e verificação de senhas.
- `frontend/`: interface web.
- `supabase/migrations/`: schema, seeds e políticas RLS.
- `tests/`: testes unitários e E2E da API.

## Fluxos principais

1. O usuário faz login e recebe um JWT.
2. O frontend envia o token nas operações protegidas.
3. A API valida identidade, papel, sala, capacidade e horários.
4. Recorrências são expandidas em ocorrências individuais ligadas por uma
   série.
5. A reserva é persistida e pode ser aprovada, rejeitada ou cancelada.

## Modelo funcional

### Usuários

- Requisitantes criam e acompanham suas reservas.
- Coordenadores aprovam dentro do escopo de andares configurado.
- Administradores gerenciam usuários, salas, recursos e aprovações globais.

### Reservas

Uma reserva possui sala, solicitante, título, responsável, participantes,
status, decisão e uma ou mais ocorrências. O endpoint de listagem retorna
`data` e `pagination`, com página, tamanho, total e total de páginas.

### Frontend

O frontend oferece login/cadastro, solicitação pontual ou recorrente, minhas
solicitações, aprovações, calendário, salas/recursos, usuários e painel TV.
O token fica em `sessionStorage`; o fallback local existente é uma estratégia
de demonstração quando a API está indisponível, não uma persistência de
produção.

## Conformidade com os slides e documentos legados

| Afirmação encontrada | Situação real |
|---|---|
| React e Node.js | Incorreto para o código atual; usa HTML/CSS/JavaScript e ASP.NET Core 8 |
| PostgreSQL | Correto; usado via Npgsql/Supabase |
| API REST e JSON | Correto; Minimal APIs com endpoints JSON |
| Login e permissões | Correto; JWT e autorização por papel estão implementados |
| Notificações por e-mail/SMS | Futuro; não implementado |
| Relatórios avançados | Futuro; não implementado |
| Backup e recuperação | Recomendação futura; não comprovado pelo código |
| Proteção contra SQL injection | Parcialmente favorecida por EF Core, mas não é uma certificação de segurança |
| Aplicação sem backend e `window.storage` | Documento técnico legado; não descreve mais o sistema atual |

Os slides podem continuar como registro acadêmico da concepção, mas não devem
ser usados como fonte de status técnico atual. O README e este documento devem
ser a referência para a implementação.

## Limites conhecidos

CORS ainda permite configuração ampla, o tratamento de timezone precisa ser
formalizado e a observabilidade avançada ainda está no roadmap. Não considerar
o sistema pronto para uso real antes de concluir os itens de segurança e
operação de [docs/ROADMAP.md](./ROADMAP.md).
