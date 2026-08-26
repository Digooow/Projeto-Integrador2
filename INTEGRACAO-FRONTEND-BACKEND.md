# Integração Frontend ↔ Backend

## Status vigente — 26/08/2026

- ✅ Frontend integrado ao backend e servido pela própria API em `/` e `/reserva-salas.html`.
- ✅ Cliente adaptado à paginação de reservas.
- ✅ Build e 7 testes unitários aprovados.
- ⏳ A URL pública do Render ainda precisa receber uma nova imagem e a migration 002 precisa ser aplicada no Supabase.
- ⏳ Autenticação JWT real permanece pendente; o login atual por seleção de usuário é demonstração.

O restante deste documento registra a integração entregue e suas limitações na ordem em que foram documentadas.

Este documento descreve o trabalho feito para conectar `frontend/reserva-salas.html`
ao backend ASP.NET Core (`Program.cs` + `Domain/` + `Persistence/`), que antes
existiam lado a lado no repositório sem nenhuma chamada de rede entre eles.

## O que havia antes

- **Backend**: expunha só `/health`, `GET /api/rooms` e `GET/POST /api/reservations`
  (+ approve/cancel), já ligado a um schema Postgres no Supabase
  (`supabase/migrations/001_initial.sql`).
- **Frontend**: um app completo (login, pedido de sala, aprovações, calendário,
  cadastro de salas/recursos/usuários, painel de TV) — mas toda a persistência
  usava `window.storage`, uma API que só existe dentro do ambiente de artifacts
  do Claude. Fora dali, o arquivo simplesmente não teria onde salvar nada.

Ou seja: nenhum dos dois nunca conversou com o outro.

## O que foi mudado

### Backend (`Program.cs`, `Persistence/Entities.cs`, `Persistence/ReservationDbContext.cs`)

Endpoints novos, para cobrir o que o frontend já fazia na tela:

| Método | Rota | Para quê |
|---|---|---|
| `GET`  | `/api/users` | lista de usuários (tela de login e admin) |
| `POST` | `/api/users` | cadastrar usuário |
| `PUT`  | `/api/users/{id}` | editar usuário (nome, e-mail, papel, andares) |
| `POST` | `/api/users/{id}/toggle-active` | ativar/desativar acesso |
| `GET`  | `/api/resources` | lista de recursos (projetor, ar-condicionado...) |
| `POST` | `/api/resources` | cadastrar novo tipo de recurso |
| `GET`  | `/api/rooms?includeInactive=true` | agora retorna `resourceIds`, `active`; parâmetro opcional para telas de admin verem salas desativadas |
| `POST` | `/api/rooms` | cadastrar sala |
| `PUT`  | `/api/rooms/{id}` | editar sala |
| `POST` | `/api/rooms/{id}/toggle-active` | ativar/desativar sala |
| `POST` | `/api/reservations/{id}/reject` | rejeitar pedido pendente |
| `POST` | `/api/reservations/{id}/approve?force=true` | aprovar mesmo com conflito de horário |

Mudança de modelo importante: **antes**, um pedido recorrente virava *uma* linha
em `reservations` com várias linhas em `reservation_occurrences`. O frontend,
porém, foi desenhado para aprovar/rejeitar **cada aula de uma série
individualmente** (inclusive "aprovar mesmo com conflito" só para aquele dia).
Por isso, `POST /api/reservations` agora cria **uma linha por ocorrência**,
todas compartilhando o mesmo `SeriesId` — isso é o que permite ao painel de
aprovações tratar cada data separadamente.

Também foi adicionado:
- `Responsavel` (texto livre) em `ReservationEntity` — o nome de quem
  efetivamente vai usar a sala, que pode ser diferente de quem fez o pedido.
- `Floors` (`text[]`) em `UserEntity` — os andares que um coordenador tem
  permissão de aprovar (usado só quando `Role == Coordinator`).

Ver `supabase/migrations/002_frontend_integration.sql` para as alterações de
schema (`alter table ... add column`) e o seed de 4 usuários de demonstração
(os mesmos que já apareciam na tela de login do frontend).

**Por que os campos de decisão (`Role`) viraram `string` em vez do enum
`UserRole`:** o `System.Text.Json` do ASP.NET Core, por padrão, espera número
ao desserializar enums vindos do corpo da requisição — não o nome ("Administrator").
Configurar um conversor global mexeria em todos os enums da API (inclusive o
`DayOfWeek` da recorrência, que o frontend já envia como número 0-6, igual
`Date.getDay()` do JavaScript). Para não quebrar nada e manter o código legível
nos dois lados, esses dois casos recebem `string`/`int` simples e fazem o
parse manualmente dentro do endpoint.

### Frontend (`frontend/reserva-salas.html`)

- Todo o bloco `STORAGE` (`window.storage`, `loadKey`, `saveUsers`,
  `saveCatalog`, `saveBookings`) foi substituído por um cliente HTTP simples
  (`api(path, options)`) que fala com o backend.
- `API_BASE` é configurável sem editar o arquivo:
  - `http://localhost:5000` por padrão quando aberto em `localhost`;
  - `https://projeto-integrador2-latest.onrender.com` em qualquer outro host
    (ajuste esse valor se o seu serviço no Render tiver outro nome);
  - pode ser sobrescrito com `?api=https://sua-api.exemplo.com` na URL, ou
    definindo `window.OCUPA_API_BASE` antes de carregar o arquivo.
- Duas tabelas fazem a tradução de vocabulário entre os dois lados:
  `ROLE_FRONT_TO_BACK`/`ROLE_BACK_TO_FRONT` (papéis) e `STATUS_BACK_TO_FRONT`
  (status de reserva).
- Todas as ações que antes só mexiam em `St.*` e chamavam `saveX()` agora
  chamam a API e, ao terminar, recarregam os dados do servidor
  (`fetchUsers`/`fetchCatalog`/`fetchBookings`) antes de re-renderizar — assim
  a tela sempre reflete o que está realmente salvo no banco.
- Se a API não responder (endereço errado, CORS, servidor fora do ar), o
  `boot()` cai para os dados de demonstração locais (os mesmos seeds que já
  existiam) e mostra um aviso, em vez de deixar a tela em branco.

## Como rodar localmente

1. **Backend**
   ```bash
   export SUPABASE_CONNECTION_STRING="postgresql://usuario:senha@host:5432/postgres"
   dotnet run
   ```
   Isso sobe a API em `http://localhost:5000` (ou na porta da variável `PORT`).

2. **Aplicar a nova migration no Supabase** (SQL editor do projeto, ou `psql`):
   ```bash
   psql "$SUPABASE_CONNECTION_STRING" -f supabase/migrations/002_frontend_integration.sql
   ```

3. **Frontend**: abra `frontend/reserva-salas.html` diretamente no navegador.
   Como o backend já roda em `localhost:5000` por padrão no ambiente local, não
   precisa de nenhum parâmetro extra. Para apontar para outra API (ex.: a
   instância no Render), abra assim:
   ```
   frontend/reserva-salas.html?api=https://projeto-integrador2-latest.onrender.com
   ```

## O que ficou de fora (limitações conhecidas)

> **Atualização — 26/08/2026:** A referência a “sem paginação” abaixo era válida antes da sprint de 26/08. A paginação foi implementada e está descrita na atualização de execução ao final deste documento.

Estas já eram lacunas documentadas em `ANALISE-PROJETO.md` antes desta
integração, e continuam valendo:

- **Sem autenticação real.** A tela de login é "escolha seu nome na lista" —
  não há senha nem token. Qualquer pessoa com a URL da API pode chamar
  qualquer endpoint diretamente. Antes de qualquer uso com dados reais, vale
  implementar algo como JWT e proteger os endpoints de escrita.
- **Fuso horário não tratado explicitamente.** Datas/horas viajam como texto
  local (`2026-08-25T14:00:00`) sem indicação de fuso; o Postgres grava em
  colunas `timestamptz`. Funciona para um único fuso (Brasil), mas não é
  robusto para múltiplos fusos.
- **Sem paginação** em `/api/reservations` — aceitável no volume atual, mas
  vale revisitar se o histórico crescer muito.

## Atualização de execução — 26/08/2026

A limitação de paginação acima foi resolvida: `GET /api/reservations` agora aceita `page` e `pageSize` (máximo 100) e retorna `data` com `pagination`. O frontend solicita a primeira página de até 100 registros e mantém compatibilidade com a resposta antiga para fallback.

Também foram concluídos o build limpo da API e os 7 testes unitários do domínio. Permanecem pendentes a autenticação JWT real, o tratamento explícito de fuso horário e a validação E2E contra uma instância real do Supabase/Render.

## Diagnóstico de acesso remoto — 26/08/2026

O serviço atualmente publicado no Render responde `200` em `/health`, mas retorna `404` em `/` e `/api/users`, além de `500` em `/api/rooms` e `/api/reservations`. Isso indica que a imagem em produção não corresponde ao estado atual do repositório e/ou não recebeu a migration 002.

Foi corrigido no código o problema de publicação do frontend: `frontend/reserva-salas.html` agora é incluído no publish e fica disponível em `/` e `/reserva-salas.html`. Para refletir essa correção no link público, é necessário publicar uma nova imagem no Docker Hub e fazer o redeploy no Render. Depois, execute `supabase/migrations/002_frontend_integration.sql` no SQL Editor e teste `/health`, `/api/users` e `/api/rooms` novamente.
