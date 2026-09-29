# Arquitetura

## Contexto

O sistema atual é uma aplicação ASP.NET Core 8 que serve simultaneamente a API
e o arquivo `frontend/reserva-salas.html`. Não existe um frontend React
separado nem um backend Node.js: essas tecnologias aparecem em materiais
acadêmicos antigos, não na implementação vigente.

## Visão de componentes

```text
Navegador
   │
   ├── Frontend HTML/CSS/JavaScript
   │       └── /auth/login e /api/*
   │
   └── ASP.NET Core Minimal API
           ├── Endpoints/ApplicationEndpoints.cs
           ├── Services/
           ├── autenticação JWT e autorização por papel
           └── Entity Framework Core / Npgsql
                   └── PostgreSQL no Supabase
```

O frontend é servido pelo backend nas rotas `/` e `/reserva-salas.html`.
Endpoints protegidos exigem `Authorization: Bearer <token>`. A consulta de
reservas aprovadas alimenta o painel público.

## Camadas e responsabilidades

### Composição e infraestrutura

`Program.cs` registra o `ReservationDbContext`, autenticação JWT, autorização,
CORS, serviços e middleware. O arquivo não contém regras de negócio nem
definições de rotas.

### Endpoints

`Endpoints/ApplicationEndpoints.cs` concentra o mapeamento das Minimal APIs,
organizado por autenticação, salas, recursos, usuários e reservas. Essa camada
recebe requests, chama os serviços e converte `ServiceResult` em respostas HTTP.

### Serviços de aplicação

`Services/` contém os casos de uso e regras de aplicação:

- `AuthService`: login, emissão de JWT e cadastro de requisitantes.
- `RoomService`: consulta, criação, atualização e ativação de salas.
- `ResourceService`: consulta e criação de recursos.
- `UserService`: consulta, criação, atualização e ativação de usuários.
- `ReservationAppService`: consulta, criação, recorrência, aprovação,
  rejeição e cancelamento de reservas.

Os serviços não dependem de `HttpRequest`, `IResult` ou detalhes do transporte.
`ServiceResult` padroniza estados como `Ok`, `BadRequest`, `Conflict`,
`NotFound` e `Forbidden`.

### Domínio

`Domain/ReservationDomain.cs` contém entidades, estados de reserva e papéis.
As regras de aplicação são orquestradas pelos serviços, enquanto o domínio
mantém conceitos independentes de HTTP e de PostgreSQL.

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

- `Program.cs`: composição da aplicação, infraestrutura e middleware.
- `Endpoints/ApplicationEndpoints.cs`: rotas e conversão de resultados para HTTP.
- `Domain/`: entidades, estados e conceitos do domínio.
- `Models/`: DTOs de entrada e saída da API.
- `Services/`: casos de uso e regras de aplicação.
- `Persistence/`: entidades persistentes e `ReservationDbContext`.
- `Security/`: hash e verificação de senhas.
- `frontend/`: interface web.
- `supabase/migrations/`: schema, seeds e políticas RLS.
- `tests/`: testes unitários e E2E da API.

## Fluxos principais

1. O usuário faz login e recebe um JWT.
2. O frontend envia o token nas operações protegidas.
3. O endpoint encaminha o request ao serviço correspondente.
4. O serviço valida identidade, papel, sala, capacidade e horários.
5. Recorrências são expandidas em ocorrências individuais ligadas por uma série.
6. A reserva é persistida e pode ser aprovada, rejeitada ou cancelada.

## Princípios de design e SOLID

- **Responsabilidade única:** composição, endpoints, serviços, persistência,
  segurança e DTOs estão separados por responsabilidade.
- **Inversão de dependência:** serviços recebem suas dependências por injeção
  de dependência, sem criar o contexto ou acessar infraestrutura global.
- **Baixo acoplamento:** serviços não produzem respostas HTTP; endpoints não
  implementam regras de negócio.
- **Interfaces somente quando justificadas:** o projeto usa serviços concretos
  porque ainda não existem múltiplas implementações ou uma porta externa que
  justifique abstração. Interfaces devem ser introduzidas quando reduzirem
  acoplamento real, não apenas para cumprir formalmente o SOLID.
- **Extensibilidade:** novos grupos de endpoints e casos de uso devem ser
  adicionados em arquivos próprios, mantendo `Program.cs` estável.

## Limites conhecidos

CORS ainda permite configuração ampla, o tratamento de timezone precisa ser
formalizado, os pacotes de validação e observabilidade ainda não estão
configurados e o tratamento global de exceções ainda está no roadmap. O projeto
não deve ser considerado pronto para produção antes dos itens de segurança e
operação de [ROADMAP.md](./ROADMAP.md).
