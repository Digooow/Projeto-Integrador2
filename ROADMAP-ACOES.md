# 🚀 ROADMAP DE AÇÕES - Sistema de Reserva de Salas

## Status vigente — 28/08/2026

O roadmap abaixo preserva o planejamento original da sprint de 22/08. Use esta atualização para não reabrir tarefas já entregues.

### Itens concluídos nesta evolução

- [x] Integração frontend/backend.
- [x] Serving do frontend em `/` e `/reserva-salas.html` no publish da API.
- [x] Paginação de reservas, com `pageSize` limitado a 100.
- [x] Migration 002 com políticas RLS complementares idempotentes.
- [x] Dependências da stack recomendada instaladas.
- [x] Target dos testes alinhado ao .NET 8.
- [x] Build da API e 7 testes unitários aprovados.

### Próximas tarefas reais

- [x] Configurar JWT, credenciais, claims e autorização nos endpoints.
- [ ] Integrar o login JWT ao frontend e validar o fluxo ponta a ponta.
- [ ] Configurar FluentValidation, Serilog/Application Insights e Swagger no código.
- [ ] Aplicar a migration 002 no Supabase e validar permissões.
- [ ] Publicar a imagem atualizada e fazer redeploy no Render.
- [ ] Criar testes E2E e revisar CORS, fuso horário e rate limiting.

**Data:** 22/08/2026  
**Objetivo:** Guia prático para equipe saber exatamente o que fazer

> **Nota de rastreabilidade — 26/08/2026:** O planejamento abaixo mantém as tarefas e checklists da sprint de 22/08. Os itens de integração, paginação, serving do frontend, dependências e testes já concluídos estão marcados no status vigente acima e não devem ser recontados como trabalho futuro.

---

## 📊 STATUS GERAL DO PROJETO

```
████████████████░░░░░░░░░░░░░░░░░░░░░░░░░░░░ 42% COMPLETO

Componentes:
✅ Backend API:          ████████████████░░░░░░░░░░░░░ 90%
✅ Banco de Dados:       █████████████████████████████░ 100%
✅ Testes:               ████████████████░░░░░░░░░░░░░░ 80%
✅ CI/CD:                █████████████████████████████░ 100%
✅ Deploy:               █████████████████████████████░ 100%
✅ Documentação:         █████████████████████████████░ 100%
🔴 Autenticação:         ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ 0%
🔴 Frontend:             ██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ 20%
🟡 Segurança (RLS):      ██████░░░░░░░░░░░░░░░░░░░░░░░░░░ 60%
```

---

## 🔥 FASE 1: CRÍTICA (SEMANA 1) - BLOQUEIA PRODUÇÃO

### Tarefa 1.1: Integrar Autenticação JWT ao Frontend

> **Atualização — 28/08/2026:** O backend já configura middleware, login por credenciais e proteção por claims. Falta o frontend armazenar o token retornado por `/auth/login` e enviá-lo nas chamadas protegidas.

**Status:** 🟡 Backend concluído; integração do frontend pendente
**Prioridade:** 🔴 P0 CRÍTICA  
**Tempo:** 6 horas  
**Responsável:** Backend Lead

#### O que fazer:

```csharp
Passo 1: Instalar dependências
  [ ] Install-Package System.IdentityModel.Tokens.Jwt
  [ ] Install-Package Microsoft.IdentityModel.Tokens
  [ ] Install-Package Microsoft.AspNetCore.Authentication.JwtBearer

Passo 2: Criar serviço de JWT em Infrastructure/
  [ ] AuthenticationService.cs
      ├─ GenerateToken(User) → string JWT
      ├─ ValidateToken(token) → ClaimsPrincipal
      └─ ExtratoUser from token → User object

Passo 3: Atualizar Program.cs
  [ ] AddAuthentication("Bearer") 
  [ ] AddJwtBearer()
  [ ] MapGet("/auth/login", LoginEndpoint) // Temporário para testes
  [ ] UseAuthentication()
  [ ] UseAuthorization()

Passo 4: Proteger Endpoints
  [ ] POST /api/reservations → Requer [Authorize]
  [ ] POST /api/reservations/{id}/approve → Requer [Authorize(Roles="Coordinator,Administrator")]
  [ ] POST /api/reservations/{id}/cancel → Requer [Authorize]

Passo 5: Testar
  [ ] POST /auth/login → Retorna JWT
  [ ] GET /api/rooms → 200 OK (público)
  [ ] POST /api/reservations sem token → 401 Unauthorized
  [ ] POST /api/reservations com token → 201 Created
  [ ] POST /api/reservations/{id}/approve como Teacher → 403 Forbidden
```

#### Arquivo de exemplo: `Infrastructure/AuthenticationService.cs`

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Projeto_Integrador2.Domain;

namespace Projeto_Integrador2.Infrastructure;

public sealed class AuthenticationService(IConfiguration config)
{
    private readonly string _secretKey = config["JWT:SecretKey"] 
        ?? throw new InvalidOperationException("JWT:SecretKey não configurada");
    private readonly string _issuer = config["JWT:Issuer"] ?? "projeto-integrador2";
    private readonly string _audience = config["JWT:Audience"] ?? "projeto-integrador2-users";

    public string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Role, user.Role.ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_secretKey));
            var handler = new JwtSecurityTokenHandler();
            
            return handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
            }, out SecurityToken validatedToken);
        }
        catch
        {
            return null;
        }
    }
}
```

#### Arquivo de exemplo: `Program.cs` (adicionar)

```csharp
// Adicionar no appsettings.json:
{
  "JWT": {
    "SecretKey": "sua-chave-super-secreta-com-32-caracteres-minimo!!!!",
    "Issuer": "projeto-integrador2",
    "Audience": "projeto-integrador2-users"
  }
}

// Adicionar no Program.cs, após AddCors:
var key = Encoding.ASCII.GetBytes(builder.Configuration["JWT:SecretKey"]!);
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JWT:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddScoped<AuthenticationService>();

// Após app.UseCors():
app.UseAuthentication();
app.UseAuthorization();

// Adicionar endpoint de login (temporário):
app.MapPost("/auth/login", async (AuthLoginRequest input, ReservationDbContext db) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Id == input.UserId && u.Active);
    if (user is null)
        return Results.BadRequest(new { error = "Usuário não encontrado" });

    var authService = app.Services.GetRequiredService<AuthenticationService>();
    var domainUser = new User(user.Id, user.Role);
    var token = authService.GenerateToken(domainUser);

    return Results.Ok(new { token, user = new { user.Id, user.Name, user.Role } });
});

public sealed record AuthLoginRequest(string UserId);
```

---

### Tarefa 1.2: Completar RLS no Banco de Dados

**Status:** ❌ Não iniciado  
**Prioridade:** 🔴 P0 CRÍTICA  
**Tempo:** 4 horas  
**Responsável:** DBA/Backend

#### O que fazer:

```sql
Passo 1: Adicionar políticas de INSERT

[ ] CREATE POLICY "users_can_create_own_reservations" 
    ON reservations 
    FOR INSERT
    WITH CHECK (requester_id = auth.uid());

[ ] CREATE POLICY "only_coordinators_can_insert_users"
    ON users
    FOR INSERT
    WITH CHECK (
        EXISTS (
            SELECT 1 FROM users 
            WHERE id = auth.uid() 
            AND role IN ('Coordinator', 'Administrator')
        )
    );

Passo 2: Adicionar políticas de UPDATE

[ ] CREATE POLICY "coordinators_can_update_reservations"
    ON reservations
    FOR UPDATE
    USING (
        EXISTS (
            SELECT 1 FROM users 
            WHERE id = auth.uid() 
            AND role IN ('Coordinator', 'Administrator')
        )
    )
    WITH CHECK (
        EXISTS (
            SELECT 1 FROM users 
            WHERE id = auth.uid() 
            AND role IN ('Coordinator', 'Administrator')
        )
    );

[ ] CREATE POLICY "users_can_update_own_info"
    ON users
    FOR UPDATE
    USING (id = auth.uid())
    WITH CHECK (id = auth.uid());

Passo 3: Adicionar políticas de DELETE

[ ] CREATE POLICY "only_admins_can_delete"
    ON users
    FOR DELETE
    USING (
        EXISTS (
            SELECT 1 FROM users 
            WHERE id = auth.uid() 
            AND role = 'Administrator'
        )
    );

Passo 4: Testar no Render/Supabase SQL Editor
  [ ] Fazer login como Teacher
  [ ] Tentar criar reserva → Deve funcionar
  [ ] Tentar aprovar reserva → Deve falhar (403)
  [ ] Fazer login como Coordinator
  [ ] Tentar aprovar reserva → Deve funcionar
```

#### Arquivo SQL para executar:

```sql
-- Executar no Supabase SQL Editor

-- 1. Políticas de INSERT
CREATE POLICY "users_can_create_own_reservations" 
    ON reservations 
    FOR INSERT
    WITH CHECK (requester_id = auth.uid());

-- 2. Políticas de UPDATE - Coordinators
CREATE POLICY "coordinators_can_update_reservations_status"
    ON reservations
    FOR UPDATE
    USING (
        EXISTS (
            SELECT 1 FROM users 
            WHERE id = auth.uid() 
            AND role IN ('Coordinator', 'Administrator')
        )
    )
    WITH CHECK (
        EXISTS (
            SELECT 1 FROM users 
            WHERE id = auth.uid() 
            AND role IN ('Coordinator', 'Administrator')
        )
    );

-- 3. Usuários veem suas próprias reservas (Pending)
CREATE POLICY "users_can_view_own_pending_reservations"
    ON reservations
    FOR SELECT
    USING (
        requester_id = auth.uid() 
        OR 
        EXISTS (
            SELECT 1 FROM users 
            WHERE id = auth.uid() 
            AND role IN ('Coordinator', 'Administrator')
        )
    );

-- Executar estes comandos para testar
GRANT USAGE ON SCHEMA public TO authenticated;
GRANT SELECT ON ALL TABLES IN SCHEMA public TO authenticated;
GRANT INSERT ON reservations TO authenticated;
GRANT UPDATE ON reservations TO authenticated;
```

---

### Checklist de Conclusão - Fase 1

```
✅ Semana 1 - SEGURANÇA

[ ] JWT Implementado
    ├─ POST /auth/login funciona
    ├─ Token gerado com user id e role
    └─ Token válido por 24 horas

[ ] Endpoints Protegidos
    ├─ POST /api/reservations requer [@Authorize]
    ├─ POST /api/reservations/{id}/approve requer [@Authorize(Roles="Coordinator,Administrator")]
    ├─ POST /api/reservations/{id}/cancel requer [@Authorize]
    └─ GET /api/rooms PERMANECE público

[ ] RLS Completo no Banco
    ├─ Users só veem suas próprias reservas (pendentes)
    ├─ Coordinators veem tudo
    ├─ INSERT/UPDATE/DELETE protegido
    └─ Sem brechas de segurança

[ ] Testes
    ├─ Teste login sem credenciais → 400 Bad Request
    ├─ Teste login com usuário inválido → 400 Bad Request
    ├─ Teste POST sem token → 401 Unauthorized
    ├─ Teste POST com token Teacher → 201 Created
    ├─ Teste approve como Teacher → 403 Forbidden
    └─ Teste approve como Coordinator → 200 OK

[ ] Documentação
    ├─ README atualizado com como fazer login
    ├─ Exemplo de curl com Authorization header
    └─ Notas sobre JWT em environment variables
```

---

## 🟡 FASE 2: IMPORTANTE (SEMANA 2-3)

### Tarefa 2.1: Integrar Frontend com API

> **Atualização — 26/08/2026:** Esta tarefa foi concluída em 26/08, incluindo cliente HTTP, CRUD, reservas, decisões, fallback e serving do HTML. O checklist abaixo é o plano original e permanece como histórico; não representa tarefas ainda não executadas.

**Status:** 🟡 Iniciado  
**Prioridade:** P1  
**Tempo:** 25 horas  
**Responsável:** Frontend Lead

#### Checklist:

```
Frontend (frontend/reserva-salas.html)

[ ] Página de Login
    ├─ Form com userId + password
    ├─ POST /auth/login
    ├─ Store token no localStorage
    ├─ Redirect para dashboard se login OK
    └─ Exibir erro se login falhar

[ ] Dashboard de Professores
    ├─ Listar minhas reservas (GET /api/reservations)
    ├─ Filtrar por status (Pending, Approved, Cancelled)
    ├─ Botão "Nova Reserva"
    ├─ Botão "Cancelar" em cada reserva
    ├─ Botão "Ver Detalhes" em cada reserva
    └─ Atualizar lista a cada 30 segundos

[ ] Formulário de Criação de Reserva
    ├─ Select sala (GET /api/rooms)
    ├─ Input data início
    ├─ Input horário início
    ├─ Input data fim
    ├─ Input horário fim
    ├─ Checkbox "Recorrência semanal"
    ├─ Multi-select dias da semana (Terça, Quinta, etc)
    ├─ Input data até (para recorrência)
    ├─ Input quantidade de pessoas
    ├─ POST /api/reservations
    ├─ Validação: fim > início
    ├─ Validação: pessoas > 0
    └─ Mensagem de sucesso/erro

[ ] Painel de Detalhes da Reserva
    ├─ Mostrar: Sala, Data, Hora, Pessoas, Status
    ├─ Se Pending: Botão Cancelar
    ├─ Se Approved: Mostrar "Aprovada por [Coordinator]"
    ├─ Se Rejected: Mostrar motivo
    └─ Se Cancelled: Mostrar "Cancelada"

[ ] Dashboard de Coordinators
    ├─ Listar TODAS reservas pendentes
    ├─ Botão "Aprovar" em cada uma
    ├─ Botão "Rejeitar" em cada uma
    ├─ Mostrar detalhes: Sala, Professor, Data, Pessoas
    ├─ Validação visual: Mostrar se tem conflito de horário
    ├─ POST /api/reservations/{id}/approve
    ├─ POST /api/reservations/{id}/reject (quando implementado)
    └─ Atualizar lista a cada 10 segundos

[ ] Listagem de Salas
    ├─ GET /api/rooms
    ├─ Mostrar: Nome, Andar, Capacidade, Descrição
    ├─ Mostrar: Lista de recursos (Projetor, Ar, Quadro, etc)
    ├─ Filtrar por capacidade mínima
    ├─ Buscar por nome/andar
    └─ Link: "Reservar essa sala"

[ ] Responsividade
    ├─ Mobile (< 768px)
    ├─ Tablet (768px - 1024px)
    ├─ Desktop (> 1024px)
    └─ Testar em Chrome, Firefox, Safari

[ ] Styling/UX
    ├─ Usar cores do design existente (--accent, --danger, etc)
    ├─ Buttons com hover/active states
    ├─ Loading spinners durante requisições
    ├─ Notificações de erro em toast/alert
    ├─ Dark mode (já tem CSS preparado)
    └─ Accessibility (aria-labels, keyboard navigation)
```

#### Template JavaScript inicial (adicionar ao HTML):

```javascript
// Adicionar no <script> do HTML

const API_URL = "https://projeto-integrador2-latest.onrender.com";
let token = localStorage.getItem("token");

// Login
async function login(userId) {
    const res = await fetch(`${API_URL}/auth/login`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ userId })
    });
    
    if (!res.ok) {
        alert("Erro ao fazer login");
        return;
    }
    
    const data = await res.json();
    token = data.token;
    localStorage.setItem("token", token);
    location.href = "#dashboard";
}

// Listar Salas
async function loadRooms() {
    const res = await fetch(`${API_URL}/api/rooms`);
    const rooms = await res.json();
    
    const select = document.getElementById("roomSelect");
    rooms.forEach(room => {
        const option = document.createElement("option");
        option.value = room.id;
        option.text = `${room.name} (${room.capacity} pessoas)`;
        select.appendChild(option);
    });
}

// Criar Reserva
async function createReservation() {
    const roomId = document.getElementById("roomSelect").value;
    const startDate = new Date(document.getElementById("startDate").value);
    const endDate = new Date(document.getElementById("endDate").value);
    const attendees = parseInt(document.getElementById("attendees").value);
    
    const body = {
        roomId,
        start: startDate.toISOString(),
        end: endDate.toISOString(),
        title: document.getElementById("title").value,
        attendees,
        recurrence: null // TODO: implementar recorrência
    };
    
    const res = await fetch(`${API_URL}/api/reservations`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${token}`
        },
        body: JSON.stringify(body)
    });
    
    if (res.ok) {
        alert("Reserva criada com sucesso!");
        location.href = "#dashboard";
    } else {
        const error = await res.json();
        alert(`Erro: ${error.error}`);
    }
}

// Listar Minhas Reservas
async function loadMyReservations() {
    const res = await fetch(`${API_URL}/api/reservations`, {
        headers: { "Authorization": `Bearer ${token}` }
    });
    const reservations = await res.json();
    
    const list = document.getElementById("reservationsList");
    reservations.forEach(res => {
        const item = document.createElement("div");
        item.className = "reservation-item";
        item.innerHTML = `
            <h3>${res.title}</h3>
            <p>Sala: ${res.roomId}</p>
            <p>Status: ${res.status}</p>
            <button onclick="cancelReservation('${res.id}')">Cancelar</button>
        `;
        list.appendChild(item);
    });
}

// Cancelar Reserva
async function cancelReservation(id) {
    const res = await fetch(`${API_URL}/api/reservations/${id}/cancel`, {
        method: "POST",
        headers: { "Authorization": `Bearer ${token}` },
        body: JSON.stringify({ userId: localStorage.getItem("userId") })
    });
    
    if (res.ok) {
        alert("Reserva cancelada!");
        location.reload();
    }
}

// Aprovar Reserva (apenas Coordinators)
async function approveReservation(id) {
    const res = await fetch(`${API_URL}/api/reservations/${id}/approve`, {
        method: "POST",
        headers: { "Authorization": `Bearer ${token}` },
        body: JSON.stringify({ userId: localStorage.getItem("userId"), role: "Coordinator" })
    });
    
    if (res.ok) {
        alert("Reserva aprovada!");
        location.reload();
    }
}

// Executar ao carregar página
document.addEventListener("DOMContentLoaded", () => {
    if (!token) {
        location.href = "#login";
        return;
    }
    
    loadRooms();
    loadMyReservations();
});
```

---

### Tarefa 2.2: Adicionar Paginação

**Status:** ❌ Não iniciado  
**Prioridade:** P1  
**Tempo:** 2 horas  
**Responsável:** Backend

#### O que fazer:

```csharp
Passo 1: Adicionar query parameters

[ ] GET /api/reservations?page=1&pageSize=20
    └─ Adicionar parâmetros: int page = 1, int pageSize = 20

Passo 2: Implementar paginação no Program.cs

app.MapGet("/api/reservations", async (
    ReservationDbContext db, 
    ReservationStatus? status, 
    int page = 1, 
    int pageSize = 20,
    CancellationToken cancellationToken) =>
{
    const int maxPageSize = 100;
    pageSize = Math.Min(pageSize, maxPageSize);
    
    var query = db.Reservations
        .AsNoTracking()
        .Include(r => r.Occurrences);
        
    if (status is not null)
        query = query.Where(r => r.Status == status);

    var total = await query.CountAsync(cancellationToken);
    
    var reservations = await query
        .OrderBy(r => r.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(r => new ReservationResponse(...))
        .ToListAsync(cancellationToken);

    return Results.Ok(new 
    { 
        data = reservations,
        pagination = new 
        { 
            page,
            pageSize,
            total,
            totalPages = (int)Math.Ceiling(total / (double)pageSize)
        }
    });
});

Passo 3: Testar
[ ] GET /api/reservations?page=1&pageSize=10 → retorna 10 itens
[ ] GET /api/reservations?page=2&pageSize=10 → retorna próximos 10
[ ] Response tem { data[], pagination { page, pageSize, total, totalPages } }
```

---

### Checklist de Conclusão - Fase 2

```
✅ Semana 2-3 - FUNCIONALIDADE

Frontend
[ ] Login funcional (POST /auth/login)
[ ] Dashboard de Professores
[ ] Dashboard de Coordinators
[ ] Formulário de Nova Reserva
[ ] Listar Salas com Recursos
[ ] Responsivo em mobile/tablet/desktop
[ ] Validações de input implementadas
[ ] Mensagens de erro/sucesso visíveis

Backend
[ ] Paginação implementada
[ ] Validações de input (FluentValidation)
[ ] Global Exception Handler
[ ] Logging estruturado (Serilog)
[ ] Todas as regras de negócio validadas

Testes
[ ] Frontend testado em 3 navegadores
[ ] API testado com Postman (cenários reais)
[ ] Testes E2E de login → criar → aprovar → ver
[ ] Performance: 1000 requisições simultâneas
```

---

## 🟠 FASE 3: NICE-TO-HAVE (SEMANA 4+)

### Tarefas (ordem de prioridade):

```
[ ] Endpoint POST /api/reservations/{id}/reject (1h)
[ ] Endpoint PUT /api/reservations/{id} para modificar (3h)
[ ] SignalR para notificações em tempo real (8h)
[ ] Painel TV com stream de reservas aprovadas (5h)
[ ] Relatórios PDF/Excel (6h)
[ ] Integração com SendGrid para emails (4h)
[ ] Dashboard de Analytics (uso de salas) (6h)
[ ] Mobile app (React Native / Flutter) (40h+)
```

---

## 📞 PESSOAS E RESPONSABILIDADES

```
Backend Lead:
- JWT + RLS (Tarefa 1.1, 1.2)
- Paginação (Tarefa 2.2)
- Validações (Tarefa 2.2 parcial)
- Logging (Tarefa 2.2)

Frontend Lead:
- Frontend integration (Tarefa 2.1)
- HTML/CSS/JS
- Validações de input
- Responsividade

QA Lead:
- Testes E2E
- Testes de segurança (JWT, RLS)
- Testes de performance
- Testes de compatibilidade (browsers)

DevOps (Opcional):
- Monitoramento (Application Insights)
- Alertas (Slack/Email)
- Backup (Supabase)
- Load testing
```

---

## 🎯 MÉTRICAS DE SUCESSO

```
Semana 1 (Fase 1 - Segurança):
├─ ✅ API segura com JWT
├─ ✅ RLS implementado
├─ ✅ 0 vulnerabilidades de segurança
└─ ✅ Documentação de segurança

Semana 2-3 (Fase 2 - Funcionalidade):
├─ ✅ Frontend 90% funcional
├─ ✅ Usuários conseguem reservar salas
├─ ✅ Coordinators conseguem aprovar
├─ ✅ 80% cobertura de testes
└─ ✅ Deploy funcionando sem erros

Semana 4+ (Fase 3 - Polish):
├─ ✅ Features avançadas implementadas
├─ ✅ 95% cobertura de testes
├─ ✅ Performance > 100 req/sec
└─ ✅ Pronto para produção real

Geral:
├─ 📊 0 vulnerabilidades críticas
├─ 📊 99% uptime
├─ 📊 < 500ms latência P95
├─ 📊 Documentação 100%
└─ 📊 Equipe treinada
```

---

## 🚨 BLOQUEADORES E RISCOS

```
Bloqueadores Atuais:
🔴 Autenticação faltando (CRÍTICO)
   └─ Impede: Produção, Testes E2E, Segurança
   
🔴 Frontend não integrado (CRÍTICO)
   └─ Impede: Validação com usuários

Riscos:
🟡 Performance com muitas reservas
   └─ Mitigation: Adicionar paginação + índices

🟡 Dados sensíveis em logs
   └─ Mitigation: Sanitizar logs, não logar JWTs

🟡 Banco de dados crash
   └─ Mitigation: Backup automático Supabase, read replicas
```

---

## 📅 TIMELINE REALISTA

```
INÍCIO: 22/08/2026

Semana 1 (22-28 ago):
├─ Segunda: JWT + Login endpoint
├─ Terça: RLS completo
├─ Quarta: Testes de segurança
├─ Quinta: Documentação
├─ Sexta: Demo + revisão
└─ Status: Seguro para produção ✅

Semana 2 (29 ago - 04 set):
├─ Segunda: Frontend login + dashboard
├─ Terça: Formulário de nova reserva
├─ Quarta: Dashboard de coordinators
├─ Quinta: Testes E2E
├─ Sexta: Bug fixes + demo
└─ Status: MVP funcional ✅

Semana 3 (05-11 set):
├─ Segunda: Paginação + validações
├─ Terça: Logging centralizado
├─ Quarta: Testes de performance
├─ Quinta: Responsividade mobile
├─ Sexta: Polish + documentação
└─ Status: Pronto para beta ✅

Semana 4+ (12 set +):
├─ Features avançadas
├─ Notificações
├─ Relatórios
└─ Status: Produção full ✅

ENTREGA ESPERADA: 15-20 de Setembro de 2026
```

---

## 📝 TEMPLATE DE DAILY STANDUP

```
O que fez ontem:
[ ] Implementei X
[ ] Testei Y
[ ] Documentei Z

O que vai fazer hoje:
[ ] Vou fazer A
[ ] Vou fazer B
[ ] Vou fazer C

Bloqueadores:
[ ] Nenhum
OU
[ ] Preciso de decisão sobre X
[ ] Dependendo de Y do time Z

Métricas:
[ ] Commits: 5
[ ] Testes escritos: 3
[ ] Bugs corrigidos: 1
[ ] Documentação: 2 arquivos
```

---

**Próxima revisão:** Segunda-feira, 25/08/2026

**Atualizado:** 22/08/2026 às 23:45

## Atualização de execução — 26/08/2026

### Entregas concluídas

- [x] Integração frontend/backend, incluindo CRUD de usuários, salas e recursos.
- [x] Aprovação, rejeição, cancelamento e recorrência por ocorrência.
- [x] Paginação de reservas com limite máximo de 100 itens.
- [x] Migration RLS complementar idempotente para operações autenticadas.
- [x] Correção do projeto de testes para `net8.0`.
- [x] Validação local: build da API aprovado e 7 testes aprovados.

### Cronograma atualizado

| Data | Frente | Status |
|---|---|---|
| 26/08 | Estabilização, paginação, RLS e testes | ✅ Completo |
| 27–29/08 | JWT com credenciais reais e claims | ⏳ Em planejamento |
| 30/08–02/09 | E2E, CORS, fuso horário e segurança de produção | ⏳ Planejado |
| 03–06/09 | Logging, rate limiting e observabilidade | ⏳ Planejado |

O login por seleção de usuário continua sendo uma demonstração; a Fase 1 de segurança só poderá ser marcada como completa após a implementação e os testes de JWT.
