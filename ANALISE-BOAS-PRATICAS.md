# 📋 ANÁLISE DE BOAS PRÁTICAS DE PROGRAMAÇÃO

## Status vigente — 28/08/2026

Esta análise original é preservada como referência técnica. O estado de
execução atual deve ser consultado no [roadmap](./ROADMAP-ATUAL.md).

- ✅ FluentValidation, Serilog, sinks, JWT, JwtBearer e Swashbuckle adicionados ao projeto.
- ✅ Paginação e serving do frontend implementados.
- ✅ Build e 9 testes aprovados: 7 unitários e 2 E2E.
- ⏳ Interfaces/repositories, configuração completa de FluentValidation, logging,
  middleware global e refatoração de `Program.cs` são melhorias técnicas futuras.

Os percentuais e checklists posteriores refletem a fotografia da análise e não
devem ser usados para reabrir entregas já concluídas, como autenticação JWT,
integração frontend/backend, migrations ou deploy.

**Projeto:** Projeto-Integrador2 - Sistema de Reserva de Salas  
**Data:** 22/08/2026  
**Análise:** SOLID, Clean Code, DRY, e Padrões de Desenvolvimento

---

## 📊 RESUMO EXECUTIVO

```
SOLID Principles:       ████████░░ 80% Implementado
Clean Code:             ███████░░░ 70% Implementado
DRY Principle:          ██████░░░░ 60% Implementado
Design Patterns:        ████████░░ 80% Implementado
Arquitetura:            █████████░ 90% Implementado

NOTA: Projeto já está acima da média! Boas práticas bem estabelecidas.
```

---

## ✅ O QUE ESTÁ BOM (Implementado Corretamente)

### 1️⃣ **SOLID - Single Responsibility Principle (S)** ⭐⭐⭐⭐⭐

**Status:** ✅ Excelente

**Evidências:**

```csharp
// Cada classe tem UMA responsabilidade clara:

ReservationService → Lógica de negócio de reservas
├─ Submit()    → Criar reserva
├─ Approve()   → Aprovar reserva
├─ Cancel()    → Cancelar reserva
└─ History()   → Consultar histórico

ReservationDbContext → Apenas mapeamento ORM
├─ OnModelCreating() → Configurar relacionamentos
└─ DbSets → Exposição de tabelas

UserEntity → Apenas dados de usuário
RoomEntity → Apenas dados de sala
ReservationEntity → Apenas dados de reserva
```

**Por que é bom:**
- Cada classe é testável isoladamente
- Mudanças em uma entidade não afetam outra
- Fácil de entender e manter
- Reutilizável em outros contextos

**Exemplo no código:**

```csharp
public sealed class ReservationService
{
    // Uma única responsabilidade: gerenciar reservas
    public Reservation Submit(ReservationRequest request) { ... }
    public Reservation Approve(Guid reservationId, User approver) { ... }
    public Reservation Cancel(Guid reservationId, User user) { ... }
}
```

---

### 2️⃣ **SOLID - Dependency Injection (D)** ⭐⭐⭐⭐⭐

**Status:** ✅ Excelente

**Evidências:**

```csharp
// Program.cs - DI bem configurado:
builder.Services.AddDbContext<ReservationDbContext>(options =>
    options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());

// Endpoints recebem DbContext injetado automaticamente:
app.MapGet("/api/rooms", async (ReservationDbContext db, CancellationToken cancellationToken) =>
{
    // db é injetado pelo framework
    // Sem new ReservationDbContext() -> coupling reduzido
});

app.MapPost("/api/reservations", async (CreateReservationRequest input, ReservationDbContext db, CancellationToken cancellationToken) =>
{
    // Mesmo padrão aqui
});
```

**Por que é bom:**
- Fácil de testar (pode-se injetar mock)
- Não há tight coupling
- Configuração centralizada
- Ciclo de vida gerenciado automaticamente

---

### 3️⃣ **Domain-Driven Design (DDD)** ⭐⭐⭐⭐⭐

**Status:** ✅ Excelente

**Evidências:**

```csharp
// Domain layer pura - sem dependências externas:
namespace Projeto_Integrador2.Domain;

// Value Objects imutáveis (records):
public sealed record RoomId(string Value);
public sealed record Room(
    RoomId Id,
    string Name,
    int Floor,
    int Capacity,
    IReadOnlyCollection<string> Resources);

// Aggregate Root:
public sealed class Reservation
{
    public Guid Id { get; }
    public string UserId { get; }
    public ReservationStatus Status { get; internal set; }
    public IReadOnlyList<ReservationOccurrence> Occurrences { get; }
    // Lógica agregada contida aqui
}

// Domain exceptions:
public sealed class ReservationConflictException(string message) : InvalidOperationException(message);
public sealed class CapacityExceededException(string message) : InvalidOperationException(message);

// Domain service (sem dependências externas):
public sealed class ReservationService
{
    // Usa apenas tipos do domínio
    public Reservation Submit(ReservationRequest request) { ... }
}
```

**Por que é bom:**
- Isolamento de lógica de negócio
- Testável sem banco de dados
- Facilita evolução do domínio
- Fácil de entender regras de negócio

---

### 4️⃣ **Clean Code - Naming** ⭐⭐⭐⭐⭐

**Status:** ✅ Excelente

**Evidências:**

```csharp
// ✅ Nomes descritivos e semânticos:
public Reservation Submit(ReservationRequest request)      // Claro: submete uma solicitação
public Reservation Approve(Guid reservationId, User approver) // Claro: aprova e quem aprova
public Reservation Cancel(Guid reservationId, User user)   // Claro: cancela e quem cancela
public IReadOnlyList<Reservation> PendingRequests()        // Claro: retorna pendentes
public IReadOnlyList<Reservation> ConfirmedReservations()  // Claro: retorna confirmadas

// ✅ Variáveis claras:
var existingOccurrence = ...  // Qual ocorrência? A que já existe
var newOccurrence = ...        // Qual? A nova
var hasConflict = ...          // Booleano claro

// ✅ Sem abreviaturas confusas:
// Bom:
public ReservationDbContext dbContext
// Ruim seria:
public ReservationDbContext ctx
public ReservationDbContext rc
```

---

### 5️⃣ **Clean Code - Métodos Pequenos & Focados** ⭐⭐⭐⭐

**Status:** ✅ Muito Bom

**Evidências:**

```csharp
// ✅ Métodos pequenos fazem UMA coisa:

private static bool Overlaps(ReservationOccurrence first, ReservationOccurrence second) =>
    first.Start < second.End && second.Start < first.End;
// Só verifica overlap - 1 linha, 1 responsabilidade

private static void EnsureApprover(User user)
{
    if (user.Role is not (UserRole.Coordinator or UserRole.Administrator))
        throw new UnauthorizedAccessException(...);
}
// Só valida se é aprovador

private static IReadOnlyList<ReservationOccurrence> ExpandOccurrences(ReservationRequest request)
{
    // Só expande recorrências
    if (request.Recurrence is null)
        return [new ReservationOccurrence(request.Start, request.End)];
    
    // ... lógica de expansão
}

// ✅ Métodos públicos também são concisos:
public Reservation Get(Guid reservationId) =>
    reservations.SingleOrDefault(r => r.Id == reservationId)
    ?? throw new KeyNotFoundException("Reservation was not found.");
// Uma linha (expressão), clara intenção
```

**Por que é bom:**
- Fácil de entender
- Fácil de testar
- Menos bugs (menos complexidade)
- Reutilizável

---

### 6️⃣ **Immutability & Sealed Classes** ⭐⭐⭐⭐⭐

**Status:** ✅ Excelente

**Evidências:**

```csharp
// ✅ Records são imutáveis por padrão (C# 9+):
public sealed record User(string Id, UserRole Role);
public sealed record RoomId(string Value);
public sealed record Room(
    RoomId Id,
    string Name,
    int Floor,
    int Capacity,
    IReadOnlyCollection<string> Resources);

// ✅ Sealed classes previnem herança indevida:
public sealed class Reservation { ... }
public sealed class ReservationService { ... }
public sealed class UserEntity { ... }

// ✅ IReadOnlyCollection/IReadOnlyList previnem mutação externa:
public IReadOnlyCollection<string> Resources { get; }  // Não pode adicionar de fora
public IReadOnlyList<ReservationOccurrence> Occurrences { get; }
```

**Por que é bom:**
- Thread-safe por padrão
- Comportamento previsível
- Menos bugs relacionados a estado
- Facilita análise estática

---

### 7️⃣ **Error Handling & Custom Exceptions** ⭐⭐⭐⭐

**Status:** ✅ Muito Bom

**Evidências:**

```csharp
// ✅ Exceptions customizadas e específicas:
public sealed class ReservationConflictException(string message) : InvalidOperationException(message);
public sealed class CapacityExceededException(string message) : InvalidOperationException(message);

// ✅ Usadas apropriadamente:
public Reservation Submit(ReservationRequest request)
{
    if (!rooms.TryGetValue(request.RoomId, out var room))
        throw new KeyNotFoundException($"Room '{request.RoomId.Value}' was not found.");

    if (request.Attendees > room.Capacity)
        throw new CapacityExceededException($"Room '{room.Name}' capacity is {room.Capacity}.");
    
    // ...
}

// ✅ No endpoint, capturados apropriadamente:
catch (Exception ex) when (ex is ArgumentException or CapacityExceededException)
{
    return Results.BadRequest(new { error = ex.Message });
}
```

**Por que é bom:**
- Erros específicos facilitam debugging
- Permite tratamento diferenciado
- Mensagens claras para usuário

---

### 8️⃣ **Entity Framework Fluent API** ⭐⭐⭐⭐⭐

**Status:** ✅ Excelente

**Evidências:**

```csharp
public sealed class ReservationDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ✅ Configuração clara e expressiva:
        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(user => user.Id);
            entity.Property(user => user.Role).HasConversion<string>();
            entity.HasIndex(user => user.Email).IsUnique();
        });

        // ✅ Índices estratégicos:
        modelBuilder.Entity<ReservationEntity>(entity =>
        {
            entity.HasIndex(reservation => new { reservation.RoomId, reservation.Status });
            // Otimizado para queries de conflito
        });

        // ✅ Relacionamentos bem definidos:
        entity.HasOne(link => link.Room)
            .WithMany(room => room.Resources)
            .HasForeignKey(link => link.RoomId);
    }
}
```

**Por que é bom:**
- Validação em nível de banco
- Performance otimizada (índices)
- Integridade referencial
- Fácil de auditar

---

### 9️⃣ **Testes Unitários** ⭐⭐⭐⭐⭐

**Status:** ✅ Excelente

**Evidências:**

```csharp
// ✅ Testes sem dependência externa:
[Fact]
public void Submit_creates_a_pending_request_without_reserving_the_room()
{
    var service = NewService();
    var request = Request("teacher-1", new RoomId("204"));

    var result = service.Submit(request);

    Assert.Equal(ReservationStatus.Pending, result.Status);
    Assert.Single(service.PendingRequests());
    Assert.Empty(service.ConfirmedReservations());
}

// ✅ Testes descritivos (Arrange-Act-Assert):
[Fact]
public void Approve_rejects_an_overlapping_confirmed_reservation()
{
    // Arrange
    var service = NewService();
    var first = service.Submit(Request("teacher-1", new RoomId("204")));
    service.Approve(first.Id, new User("admin-1", UserRole.Administrator));
    var second = service.Submit(Request("teacher-2", new RoomId("204"), Start.AddHours(2)));

    // Act
    var exception = Assert.Throws<ReservationConflictException>(() =>
        service.Approve(second.Id, new User("admin-1", UserRole.Administrator)));

    // Assert
    Assert.Equal(ReservationStatus.Pending, service.Get(second.Id).Status);
    Assert.Contains("204", exception.Message);
}

// ✅ Edge cases testados:
[Fact]
public void Submit_rejects_a_request_that_exceeds_room_capacity()
{
    var service = NewService();
    var exception = Assert.Throws<CapacityExceededException>(() =>
        service.Submit(Request("teacher-1", new RoomId("204"), attendees: 31)));
    Assert.Contains("capacity", exception.Message, StringComparison.OrdinalIgnoreCase);
}
```

**Por que é bom:**
- Contrato do sistema documentado
- Regras de negócio protegidas
- Regressões identificadas rápido
- Confiança em refatoração

---

## 🟡 O QUE PODE MELHORAR (Recomendações)

### 1️⃣ **SOLID - Open/Closed Principle (O)** - 50%

**Status:** 🟡 Parcial

**Problema:**

```csharp
// Atual - sem interfaces:
public sealed class ReservationService
{
    private readonly IReadOnlyDictionary<RoomId, Room> rooms;
    // Tight coupling com dados em memória
}

// Program.cs - Logic misturada com configuração:
app.MapPost("/api/reservations", async (CreateReservationRequest input, ReservationDbContext db, ...) =>
{
    // Lógica de reserva aqui (violação de Open/Closed)
    // Se adicionar nova regra, modifica este arquivo
});
```

**Solução - Criar Service Layer:**

```csharp
// ✅ NOVO: Infrastructure/ReservationApplicationService.cs
public interface IReservationService
{
    Task<ReservationResponse> CreateReservationAsync(CreateReservationRequest request, CancellationToken ct);
    Task<ReservationResponse> ApproveReservationAsync(Guid id, User approver, CancellationToken ct);
    Task<ReservationResponse> CancelReservationAsync(Guid id, User user, CancellationToken ct);
}

public sealed class ReservationApplicationService(
    ReservationDbContext db,
    ILogger<ReservationApplicationService> logger) : IReservationService
{
    public async Task<ReservationResponse> CreateReservationAsync(
        CreateReservationRequest input, 
        CancellationToken cancellationToken)
    {
        try
        {
            var room = await db.Rooms.SingleOrDefaultAsync(..., cancellationToken);
            if (room is null)
                throw new RoomNotFoundException();

            // Lógica aqui
            var reservation = new ReservationEntity { ... };
            db.Reservations.Add(reservation);
            await db.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Reservation created: {ReservationId}", reservation.Id);
            return new ReservationResponse(...);
        }
        catch (ReservationConflictException ex)
        {
            logger.LogWarning(ex, "Conflict creating reservation");
            throw;
        }
    }
}

// ✅ Program.cs fica limpo:
builder.Services.AddScoped<IReservationService, ReservationApplicationService>();

app.MapPost("/api/reservations", async (
    CreateReservationRequest input,
    IReservationService service,  // ← Injetar interface
    CancellationToken cancellationToken) =>
{
    var result = await service.CreateReservationAsync(input, cancellationToken);
    return Results.Created($"/api/reservations/{result.Id}", result);
});
```

**Benefício:**
- Program.cs fica limpo (50 linhas → 200 linhas de negócio)
- Fácil adicionar nova lógica sem modificar endpoints
- Testável sem mock de endpoints

---

### 2️⃣ **SOLID - Interface Segregation (I)** - 40%

**Status:** 🔴 Falta

**Problema:**

```csharp
// Sem interfaces:
public sealed class ReservationService { ... }
public sealed class ReservationDbContext : DbContext { ... }

// Difícil testar, difícil mockar
```

**Solução - Criar interfaces:**

```csharp
// ✅ NOVO: Domain/IReservationRepository.cs
public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IEnumerable<Reservation>> GetByStatusAsync(ReservationStatus status, CancellationToken ct);
    Task<IEnumerable<Reservation>> GetConflictingAsync(
        string roomId, 
        DateTime start, 
        DateTime end, 
        CancellationToken ct);
    Task AddAsync(Reservation reservation, CancellationToken ct);
    Task UpdateAsync(Reservation reservation, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
}

// ✅ NOVO: Persistence/ReservationRepository.cs
public sealed class ReservationRepository(ReservationDbContext db) : IReservationRepository
{
    public async Task<Reservation?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await db.Reservations
            .Include(r => r.Occurrences)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    // Implementar outros métodos
}

// ✅ NOVO: Domain/IRoomRepository.cs
public interface IRoomRepository
{
    Task<Room?> GetByIdAsync(string id, CancellationToken ct);
    Task<IEnumerable<Room>> GetAllActiveAsync(CancellationToken ct);
}

// ✅ No Service:
public sealed class ReservationApplicationService(
    IReservationRepository reservationRepo,
    IRoomRepository roomRepo,
    ILogger<ReservationApplicationService> logger) : IReservationService
{
    public async Task<ReservationResponse> CreateReservationAsync(...)
    {
        var room = await roomRepo.GetByIdAsync(input.RoomId, cancellationToken);
        if (room is null)
            throw new RoomNotFoundException();
        
        // Lógica
        await reservationRepo.AddAsync(reservation, cancellationToken);
    }
}

// ✅ Em Program.cs:
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IReservationService, ReservationApplicationService>();
```

**Benefício:**
- Fácil testar com mocks
- Fácil trocar implementação (ex: trocar banco)
- Interfaces menores (segregadas)

---

### 3️⃣ **DRY - Duplicação de Validação**

**Status:** 🟡 Parcial

**Problema:**

```csharp
// Validação 1 - No domain:
private static bool Overlaps(ReservationOccurrence first, ReservationOccurrence second) =>
    first.Start < second.End && second.Start < first.End;

// Validação 2 - No endpoint (duplicada):
var hasConflict = await db.ReservationOccurrences
    .Include(o => o.Reservation)
    .AnyAsync(o =>
        o.ReservationId != id &&
        o.Reservation.RoomId == reservation.RoomId &&
        o.Reservation.Status == ReservationStatus.Approved &&
        reservation.Occurrences.Any(occ => occ.StartsAt < o.EndsAt && o.StartsAt < occ.EndsAt),
        cancellationToken);
```

**Solução - Extension methods:**

```csharp
// ✅ NOVO: Domain/ReservationExtensions.cs
public static class ReservationExtensions
{
    public static bool Overlaps(this ReservationOccurrence first, ReservationOccurrence second) =>
        first.Start < second.End && second.Start < first.End;

    public static bool HasConflict(
        this IEnumerable<Reservation> reservations,
        string roomId,
        IEnumerable<ReservationOccurrence> occurrences) =>
        reservations
            .Where(r => r.RoomId.Value == roomId && r.Status == ReservationStatus.Approved)
            .SelectMany(r => r.Occurrences)
            .Any(existing => occurrences.Any(new => new.Overlaps(existing)));
}

// ✅ No endpoint, uso simples:
app.MapPost("/api/reservations/{id:guid}/approve", async (Guid id, DecideReservationRequest input, ReservationDbContext db, CancellationToken cancellationToken) =>
{
    var reservation = await db.Reservations
        .Include(r => r.Occurrences)
        .SingleOrDefaultAsync(r => r.Id == id, cancellationToken);

    if (reservation is null)
        return Results.NotFound();

    var approved = await db.Reservations
        .Where(r => r.Status == ReservationStatus.Approved)
        .ToListAsync(cancellationToken);

    // ✅ Reusa lógica:
    if (approved.HasConflict(reservation.RoomId, reservation.Occurrences.Select(...)))
        return Results.Conflict(...);

    // Resto da lógica
});
```

**Benefício:**
- Uma fonte de verdade para validação
- Menos bugs (não duplica lógica)
- Fácil manter

---

### 4️⃣ **Clean Code - Program.cs muito grande**

**Status:** 🟡 Muito Grande

**Problema:**

```csharp
// Program.cs tem 260+ linhas
// Endpoints misturados com configuração
// Difícil de ler
```

**Solução - Extension methods:**

```csharp
// ✅ NOVO: EndpointExtensions.cs
public static class EndpointExtensions
{
    public static WebApplication MapReservationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/reservations")
            .WithName("Reservations")
            .WithOpenApi();

        group.MapGet("/", GetReservations)
            .WithName("List Reservations")
            .WithDescription("Lista todas as reservas");

        group.MapPost("/", CreateReservation)
            .WithName("Create Reservation")
            .WithDescription("Cria nova reserva");

        group.MapPost("/{id:guid}/approve", ApproveReservation)
            .WithName("Approve Reservation");

        group.MapPost("/{id:guid}/cancel", CancelReservation)
            .WithName("Cancel Reservation");

        return app;
    }

    private static async Task<IResult> GetReservations(
        ReservationDbContext db,
        ReservationStatus? status,
        IReservationService service,
        CancellationToken ct)
    {
        var reservations = await service.GetReservationsAsync(status, ct);
        return Results.Ok(reservations);
    }

    private static async Task<IResult> CreateReservation(
        CreateReservationRequest input,
        IReservationService service,
        CancellationToken ct)
    {
        // Lógica aqui
    }

    // Outros endpoints
}

// ✅ HealthEndpoints.cs
public static class HealthEndpoints
{
    public static WebApplication MapHealthEndpoints(this WebApplication app)
    {
        app.MapGet("/health", Health)
            .WithName("Health Check")
            .WithOpenApi()
            .ExcludeFromDescription();

        return app;
    }

    private static IResult Health() => Results.Ok(new
    {
        status = "ok",
        database = "connected",
        timestamp = DateTime.UtcNow
    });
}

// ✅ RoomEndpoints.cs
public static class RoomEndpoints
{
    public static WebApplication MapRoomEndpoints(this WebApplication app)
    {
        app.MapGet("/api/rooms", GetRooms)
            .WithName("List Rooms")
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> GetRooms(
        ReservationDbContext db,
        IReservationService service,
        CancellationToken ct)
    {
        // Lógica aqui
    }
}

// ✅ ServiceExtensions.cs
public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ReservationDbContext>(options =>
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());

        services.AddScoped<IReservationService, ReservationApplicationService>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<IRoomRepository, RoomRepository>();

        services.AddCors(options =>
            options.AddDefaultPolicy(policy =>
                policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

        return services;
    }
}

// ✅ Program.cs fica LIMPO:
using Projeto_Integrador2.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.Sources.Clear();
builder.Configuration.AddEnvironmentVariables();

var connectionString = Environment.GetEnvironmentVariable("SUPABASE_CONNECTION_STRING")
    ?? throw new InvalidOperationException("SUPABASE_CONNECTION_STRING não configurada");

builder.Services.AddApplicationServices(connectionString);

var app = builder.Build();

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://0.0.0.0:{port}");

app.UseCors();

// ✅ Mapear todos os endpoints:
app.MapHealthEndpoints();
app.MapRoomEndpoints();
app.MapReservationEndpoints();

app.Run();
```

**Benefício:**
- Program.cs reduzido para ~20 linhas
- Endpoints organizados por recurso
- Fácil adicionar novo endpoint

---

### 5️⃣ **Input Validation - FluentValidation**

**Status:** 🔴 Falta

**Problema:**

```csharp
// Validação faltando ou duplicada:
if (request.Attendees <= 0)
    throw new ArgumentOutOfRangeException();
// Deveria estar em um validador
```

**Solução - FluentValidation:**

```bash
dotnet add package FluentValidation
```

```csharp
// ✅ NOVO: Application/Validators/CreateReservationRequestValidator.cs
public sealed class CreateReservationRequestValidator : AbstractValidator<CreateReservationRequest>
{
    public CreateReservationRequestValidator()
    {
        RuleFor(x => x.RoomId)
            .NotEmpty()
            .WithMessage("RoomId é obrigatório");

        RuleFor(x => x.RequesterId)
            .NotEmpty()
            .WithMessage("RequesterId é obrigatório");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Título é obrigatório")
            .Length(5, 200)
            .WithMessage("Título deve ter entre 5 e 200 caracteres");

        RuleFor(x => x.Attendees)
            .GreaterThan(0)
            .WithMessage("Quantidade de pessoas deve ser maior que 0")
            .LessThanOrEqualTo(500)
            .WithMessage("Quantidade de pessoas não pode exceder 500");

        RuleFor(x => x.Start)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Data de início deve ser no futuro");

        RuleFor(x => x.End)
            .GreaterThan(x => x.Start)
            .WithMessage("Data de término deve ser após data de início");

        When(x => x.Recurrence is not null, () =>
        {
            RuleFor(x => x.Recurrence!.Days)
                .NotEmpty()
                .WithMessage("Pelo menos um dia da semana deve ser selecionado");

            RuleFor(x => x.Recurrence!.Until)
                .GreaterThan(x => x.Start)
                .WithMessage("Data de término da recorrência deve ser após data de início");
        });
    }
}

// ✅ ServiceExtensions.cs
public static IServiceCollection AddApplicationServices(...)
{
    // ... resto do código
    
    // Registrar validadores:
    services.AddValidatorsFromAssemblyContaining<CreateReservationRequestValidator>();
    
    return services;
}

// ✅ Middleware de validação:
app.UseFluentValidationMiddleware();

// ✅ Ou usar endpoint filter:
app.MapPost("/api/reservations", async (
    CreateReservationRequest input,
    IValidator<CreateReservationRequest> validator,
    IReservationService service,
    CancellationToken ct) =>
{
    var validationResult = await validator.ValidateAsync(input, ct);
    if (!validationResult.IsValid)
        return Results.BadRequest(new { errors = validationResult.Errors });

    var result = await service.CreateReservationAsync(input, ct);
    return Results.Created($"/api/reservations/{result.Id}", result);
});
```

**Benefício:**
- Validação centralizada
- Reutilizável
- Teste separado
- Mensagens claras

---

### 6️⃣ **Logging Estruturado**

**Status:** 🔴 Falta

**Problema:**

```csharp
// Sem logging:
public async Task<ReservationResponse> CreateReservationAsync(...)
{
    // Sem rastreamento de operação
    // Sem informação de quem fez o quê
}
```

**Solução - Serilog:**

```bash
dotnet add package Serilog
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Enrichers.Environment
```

```csharp
// ✅ NOVO: Infrastructure/Logging/LoggingExtensions.cs
public static class LoggingExtensions
{
    public static WebApplicationBuilder AddLogging(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentUserName()
            .Enrich.WithMachineName()
            .Enrich.WithProperty("Application", "Projeto-Integrador2")
            .WriteTo.Console(
                new CompactJsonFormatter(),
                theme: AnsiConsoleTheme.Code)
            .CreateLogger();

        builder.Host.UseSerilog();
        return builder;
    }
}

// ✅ Program.cs:
builder.AddLogging();

// ✅ No Service:
public sealed class ReservationApplicationService(
    IReservationRepository reservationRepo,
    ILogger<ReservationApplicationService> logger) : IReservationService
{
    public async Task<ReservationResponse> CreateReservationAsync(
        CreateReservationRequest input,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Creating reservation for room {RoomId} by user {UserId}",
            input.RoomId,
            input.RequesterId);

        try
        {
            var room = await roomRepo.GetByIdAsync(input.RoomId, cancellationToken);
            if (room is null)
            {
                logger.LogWarning("Room not found: {RoomId}", input.RoomId);
                throw new RoomNotFoundException($"Room {input.RoomId} not found");
            }

            var reservation = new ReservationEntity { ... };
            await reservationRepo.AddAsync(reservation, cancellationToken);

            logger.LogInformation(
                "Reservation created successfully: {ReservationId} for room {RoomId}",
                reservation.Id,
                input.RoomId);

            return new ReservationResponse(...);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error creating reservation for room {RoomId} by user {UserId}",
                input.RoomId,
                input.RequesterId);
            throw;
        }
    }
}
```

**Benefício:**
- Rastreabilidade completa
- Debugging mais fácil
- Auditoria
- Análise de performance

---

### 7️⃣ **DTO Pattern - Separação Input/Output**

**Status:** 🟡 Parcial

**Problema:**

```csharp
// Atual - mistura response types:
return Results.Ok(new
{
    entity.Id,
    status = entity.Status.ToString()
});

// Deveria ser DTO formal
```

**Solução - DTOs explícitos:**

```csharp
// ✅ NOVO: Application/DTOs/CreateReservationRequest.cs
public sealed record CreateReservationRequest(
    string RoomId,
    string RequesterId,
    string Title,
    int Attendees,
    DateTime Start,
    DateTime End,
    RecurrenceRequest? Recurrence = null);

public sealed record RecurrenceRequest(
    DayOfWeek[] Days,
    DateTime Until);

// ✅ NOVO: Application/DTOs/ReservationResponse.cs
public sealed record ReservationResponse(
    Guid Id,
    string RoomId,
    string RequesterId,
    string Title,
    int Attendees,
    string Status,
    OccurrenceResponse[] Occurrences);

public sealed record OccurrenceResponse(
    DateTime Start,
    DateTime End);

// ✅ Mapping (via AutoMapper ou manual):
public sealed class ReservationApplicationService(...)
{
    public async Task<ReservationResponse> CreateReservationAsync(...)
    {
        // Lógica aqui
        
        var response = new ReservationResponse(
            entity.Id,
            entity.RoomId,
            entity.RequesterId,
            entity.Title,
            entity.Attendees,
            entity.Status.ToString(),
            entity.Occurrences
                .OrderBy(o => o.StartsAt)
                .Select(o => new OccurrenceResponse(o.StartsAt, o.EndsAt))
                .ToArray());

        return response;
    }
}
```

**Benefício:**
- Contrato de API claro
- Versionamento facilitado
- Desacoplamento Domain ↔ API

---

### 8️⃣ **Global Exception Handling**

**Status:** 🟡 Parcial

**Problema:**

```csharp
// Exceções tratadas localmente em cada endpoint:
catch (Exception ex) when (ex is ArgumentException or CapacityExceededException)
{
    return Results.BadRequest(...);
}
// Repetido em vários places
```

**Solução - Middleware global:**

```csharp
// ✅ NOVO: Infrastructure/Middleware/ExceptionHandlingMiddleware.cs
public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception");
            await HandleExceptionAsync(context, exception);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            RoomNotFoundException => new { error = exception.Message, statusCode = 404 },
            ReservationConflictException => new { error = exception.Message, statusCode = 409 },
            CapacityExceededException => new { error = exception.Message, statusCode = 400 },
            UnauthorizedAccessException => new { error = "Unauthorized", statusCode = 403 },
            ArgumentException => new { error = exception.Message, statusCode = 400 },
            _ => new { error = "Internal server error", statusCode = 500 }
        };

        context.Response.StatusCode = response.statusCode;
        return context.Response.WriteAsJsonAsync(response);
    }
}

// ✅ ServiceExtensions.cs
public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
{
    return app.UseMiddleware<ExceptionHandlingMiddleware>();
}

// ✅ Program.cs
app.UseExceptionHandling();
app.UseCors();
// Endpoints
```

**Benefício:**
- Uma fonte de verdade para tratamento de erro
- Consistência
- Menos código duplicado

---

## 📋 PADRÕES DE DESIGN - IMPLEMENTAÇÃO

### ✅ Padrões Implementados

| Padrão | Implementação | Status |
|--------|---------------|--------|
| **Aggregate Root** | Reservation (with Occurrences) | ✅ |
| **Value Object** | RoomId, Room (records) | ✅ |
| **Repository** | DbContext (implícito) | ✅ |
| **Service Layer** | ReservationService | ✅ |
| **Data Transfer Object** | ReservationResponse, etc | ✅ |
| **Immutability** | Records, sealed classes | ✅ |
| **Factory** | Reservation constructor (interno) | ✅ |
| **Dependency Injection** | IServiceCollection | ✅ |
| **Specification** | Implicit (queries) | 🟡 |
| **Unit of Work** | DbContext.SaveChangesAsync | ✅ |

### 🟡 Padrões Recomendados

```csharp
// 1. CQRS (Command Query Responsibility Segregation)
// Separar lógica de escrita vs leitura

// 2. Event Sourcing
// Registrar eventos de domínio (ReservationCreated, ReservationApproved, etc)

// 3. Mediator Pattern
// Com MediatR para desacoplar handlers

// 4. Result Pattern
// Result<T> ao invés de exceptions para flows esperados

// 5. Specification Pattern
// Para queries complexas
```

---

## 📊 RESUMO DE CONFORMIDADE

```
SOLID Principles:

S - Single Responsibility:        ✅ 95%
O - Open/Closed:                 🟡 50% (falta Service Layer)
L - Liskov Substitution:          ✅ 90%
I - Interface Segregation:        🔴 40% (falta Interfaces)
D - Dependency Inversion:         ✅ 95%

Clean Code:

Naming:                           ✅ 95%
Métodos pequenos:                 ✅ 90%
Sem comentários desnecessários:   ✅ 95%
Tratamento de erro:               ✅ 85%
Validação de input:               🟡 60%
Logging:                          🔴  0%

DRY Principle:

Sem duplicação óbvia:             ✅ 90%
Sem duplicação de validação:      🟡 50%
Reutilização de código:           ✅ 85%

Padrões de Design:

Domain-Driven Design:             ✅ 95%
Repository Pattern:               ✅ 90%
Service Layer:                    🟡 60%
DTO Pattern:                      ✅ 90%
Exception Handling:               🟡 70%

MÉDIA GERAL:                      ✅ 76%
```

---

## 🎯 PLANO DE MELHORIA (Priorizado)

### Fase 1: CRÍTICA (1-2 semanas)

```
[ ] Criar interfaces (IReservationRepository, IRoomRepository)
    Tempo: 3 horas
    Ganho: Testabilidade, DI
    
[ ] Aplicar FluentValidation
    Tempo: 4 horas
    Ganho: Validação centralizada, Clean Code
    
[ ] Global Exception Handling Middleware
    Tempo: 2 horas
    Ganho: Código limpo, consistência
```

### Fase 2: IMPORTANTE (2-3 semanas)

```
[ ] Extrair Service Layer (IReservationService)
    Tempo: 4 horas
    Ganho: Separation of Concerns, Open/Closed
    
[ ] Adicionar Serilog (logging estruturado)
    Tempo: 3 horas
    Ganho: Observabilidade, debugging
    
[ ] Refatorar Program.cs em Extension Methods
    Tempo: 3 horas
    Ganho: Legibilidade, manutenibilidade
```

### Fase 3: NICE-TO-HAVE (4+ semanas)

```
[ ] Implementar Specification Pattern
    Tempo: 5 horas
    
[ ] CQRS (Command/Query)
    Tempo: 8 horas
    
[ ] Event Sourcing
    Tempo: 12 horas
    
[ ] AutoMapper para DTOs
    Tempo: 3 horas
    
[ ] Mediator Pattern (MediatR)
    Tempo: 6 horas
```

---

## 💡 CONCLUSÃO

**Status Geral:** ✅ **Excelente (76%)**

O projeto já segue **a maioria das boas práticas**. O código está bem estruturado com DDD implementado corretamente, testes sólidos, e arquitetura limpa.

**Recomendações imediatas:**
1. Adicionar interfaces (IReservationRepository, IRoomRepository)
2. Implementar FluentValidation
3. Adicionar logging com Serilog
4. Criar Global Exception Handler

Essas 3 ações elevarão o nível para **~90% de conformidade** com boas práticas.

---

**Documento gerado em:** 22/08/2026  
**Próxima revisão:** conforme necessidade; o status vigente está no
[ROADMAP-ATUAL.md](./ROADMAP-ATUAL.md).
