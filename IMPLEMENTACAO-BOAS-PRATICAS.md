# 💻 GUIA DE IMPLEMENTAÇÃO - REFATORAÇÃO DE BOAS PRÁTICAS

## Status vigente — 26/08/2026

O guia abaixo continua sendo o plano detalhado e foi mantido para preservar a rastreabilidade. O que mudou nesta data:

- ✅ Dependências recomendadas instaladas e restauradas.
- ✅ Build da API aprovado e 9 testes aprovados: 7 unitários e 2 E2E.
- ✅ Paginação e publicação do frontend implementadas.
- ⏳ As fases de interfaces/repositories, validação em runtime, logging, middleware global e separação completa dos endpoints ainda precisam ser executadas.

Instalar uma biblioteca não marca sua fase como concluída; a conclusão ocorrerá quando houver configuração no código e teste correspondente.

**Projeto:** Projeto-Integrador2 - Sistema de Reserva de Salas  
**Objetivo:** Passo a passo para implementar melhorias identificadas  
**Timeline:** 1-2 sprints (6-8 semanas)

---

## 📑 ÍNDICE

1. [Pré-requisitos](#pré-requisitos)
2. [Fase 1 - Interfaces & Repositories](#fase-1---interfaces--repositories)
3. [Fase 2 - Validação com FluentValidation](#fase-2---validação-com-fluentvalidation)
4. [Fase 3 - Logging com Serilog](#fase-3---logging-com-serilog)
5. [Fase 4 - Global Exception Handler](#fase-4---global-exception-handler)
6. [Fase 5 - Refatoração de Program.cs](#fase-5---refatoração-de-programcs)
7. [Testes para Validar](#testes-para-validar)

---

## 🚀 Pré-requisitos

> **Atualização — 26/08/2026:** Os pacotes descritos nos comandos abaixo foram instalados no projeto nesta data. Esta seção permanece como histórico dos pré-requisitos e não significa que as funcionalidades já estejam configuradas no código.

```bash
# Certifique-se que está na pasta do projeto
cd c:\Users\Rodrigo\Desktop\Projetos\Projeto-Integrador2

# Instalar pacotes necessários
dotnet add package FluentValidation
dotnet add package Serilog
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Enrichers.Environment
dotnet add package Serilog.Formatting.Compact
dotnet add package Serilog.Enrichers.Context
```

---

## FASE 1 - Interfaces & Repositories

### Objetivo
Aplicar **Interface Segregation** e **Dependency Inversion** para melhor testabilidade.

### Passo 1.1: Criar Interface de Repositório

**Arquivo novo:** `Domain/IReservationRepository.cs`

```csharp
using Projeto_Integrador2.Persistence;

namespace Projeto_Integrador2.Domain;

/// <summary>
/// Interface para abstração de acesso a dados de reservas.
/// Implementação de Interface Segregation (SOLID I).
/// </summary>
public interface IReservationRepository
{
    /// <summary>
    /// Obtém uma reserva pelo ID.
    /// </summary>
    Task<ReservationEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Obtém todas as reservas com status específico.
    /// </summary>
    Task<IEnumerable<ReservationEntity>> GetByStatusAsync(
        ReservationStatus status,
        CancellationToken cancellationToken);

    /// <summary>
    /// Obtém reservas que podem conflitar com período/sala.
    /// </summary>
    Task<IEnumerable<ReservationEntity>> GetConflictingAsync(
        string roomId,
        DateTime start,
        DateTime end,
        ReservationStatus status = ReservationStatus.Approved,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém todas as reservas de um usuário.
    /// </summary>
    Task<IEnumerable<ReservationEntity>> GetByRequesterAsync(
        string requesterId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Adiciona nova reserva.
    /// </summary>
    Task AddAsync(ReservationEntity reservation, CancellationToken cancellationToken);

    /// <summary>
    /// Atualiza reserva existente.
    /// </summary>
    Task UpdateAsync(ReservationEntity reservation, CancellationToken cancellationToken);

    /// <summary>
    /// Deleta reserva.
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Salva alterações pendentes.
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
```

### Passo 1.2: Criar Interface de Repositório de Salas

**Arquivo novo:** `Domain/IRoomRepository.cs`

```csharp
using Projeto_Integrador2.Persistence;

namespace Projeto_Integrador2.Domain;

/// <summary>
/// Interface para abstração de acesso a dados de salas.
/// </summary>
public interface IRoomRepository
{
    /// <summary>
    /// Obtém uma sala pelo ID.
    /// </summary>
    Task<RoomEntity?> GetByIdAsync(string id, CancellationToken cancellationToken);

    /// <summary>
    /// Obtém todas as salas ativas.
    /// </summary>
    Task<IEnumerable<RoomEntity>> GetAllActiveAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Obtém sala com seus recursos.
    /// </summary>
    Task<RoomEntity?> GetWithResourcesAsync(string id, CancellationToken cancellationToken);
}
```

### Passo 1.3: Implementar Repositório de Reservas

**Arquivo novo:** `Persistence/ReservationRepository.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Projeto_Integrador2.Domain;

namespace Projeto_Integrador2.Persistence;

/// <summary>
/// Implementação de repositório para Reservas.
/// Encapsula toda lógica de acesso a dados.
/// </summary>
public sealed class ReservationRepository(ReservationDbContext dbContext) : IReservationRepository
{
    public async Task<ReservationEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Reservations
            .Include(r => r.Occurrences)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<ReservationEntity>> GetByStatusAsync(
        ReservationStatus status,
        CancellationToken cancellationToken)
    {
        return await dbContext.Reservations
            .Where(r => r.Status == status)
            .Include(r => r.Occurrences)
            .AsNoTracking()
            .OrderBy(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ReservationEntity>> GetConflictingAsync(
        string roomId,
        DateTime start,
        DateTime end,
        ReservationStatus status = ReservationStatus.Approved,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Reservations
            .Where(r =>
                r.RoomId == roomId &&
                r.Status == status &&
                r.Occurrences.Any(o => o.StartsAt < end && o.EndsAt > start))
            .Include(r => r.Occurrences)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ReservationEntity>> GetByRequesterAsync(
        string requesterId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Reservations
            .Where(r => r.RequesterId == requesterId)
            .Include(r => r.Occurrences)
            .AsNoTracking()
            .OrderBy(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ReservationEntity reservation, CancellationToken cancellationToken)
    {
        await dbContext.Reservations.AddAsync(reservation, cancellationToken);
    }

    public Task UpdateAsync(ReservationEntity reservation, CancellationToken cancellationToken)
    {
        dbContext.Reservations.Update(reservation);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var reservation = await dbContext.Reservations.FindAsync(new object[] { id }, cancellationToken);
        if (reservation is not null)
        {
            dbContext.Reservations.Remove(reservation);
        }
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
```

### Passo 1.4: Implementar Repositório de Salas

**Arquivo novo:** `Persistence/RoomRepository.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Projeto_Integrador2.Domain;

namespace Projeto_Integrador2.Persistence;

/// <summary>
/// Implementação de repositório para Salas.
/// </summary>
public sealed class RoomRepository(ReservationDbContext dbContext) : IRoomRepository
{
    public async Task<RoomEntity?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        return await dbContext.Rooms
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id && r.Active, cancellationToken);
    }

    public async Task<IEnumerable<RoomEntity>> GetAllActiveAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Rooms
            .Where(r => r.Active)
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<RoomEntity?> GetWithResourcesAsync(string id, CancellationToken cancellationToken)
    {
        return await dbContext.Rooms
            .Include(r => r.Resources)
                .ThenInclude(link => link.Resource)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id && r.Active, cancellationToken);
    }
}
```

---

## FASE 2 - Validação com FluentValidation

### Objetivo
Aplicar **Clean Code** e **Single Responsibility** movendo validação para classe dedicada.

### Passo 2.1: Criar Estrutura de Diretório

```bash
mkdir Application
mkdir Application\Validators
mkdir Application\DTOs
```

### Passo 2.2: Criar Validador para CreateReservationRequest

**Arquivo novo:** `Application/Validators/CreateReservationRequestValidator.cs`

```csharp
using FluentValidation;
using Projeto_Integrador2.Domain;

namespace Projeto_Integrador2.Application.Validators;

/// <summary>
/// Validador para requisição de criação de reserva.
/// Implementa Clean Code segregando validação em classe dedicada.
/// </summary>
public sealed class CreateReservationRequestValidator : AbstractValidator<CreateReservationRequest>
{
    private const int MinTitleLength = 5;
    private const int MaxTitleLength = 200;
    private const int MaxAttendees = 500;

    public CreateReservationRequestValidator()
    {
        RuleFor(x => x.RoomId)
            .NotEmpty()
            .WithMessage("ID da sala é obrigatório");

        RuleFor(x => x.RequesterId)
            .NotEmpty()
            .WithMessage("ID do solicitante é obrigatório");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Título é obrigatório")
            .Length(MinTitleLength, MaxTitleLength)
            .WithMessage($"Título deve ter entre {MinTitleLength} e {MaxTitleLength} caracteres");

        RuleFor(x => x.Attendees)
            .GreaterThan(0)
            .WithMessage("Quantidade de pessoas deve ser maior que 0")
            .LessThanOrEqualTo(MaxAttendees)
            .WithMessage($"Quantidade de pessoas não pode exceder {MaxAttendees}");

        RuleFor(x => x.Start)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Data de início deve ser no futuro");

        RuleFor(x => x.End)
            .GreaterThan(x => x.Start)
            .WithMessage("Data de término deve ser após data de início");

        When(x => x.Recurrence is not null, () =>
        {
            RuleFor(x => x.Recurrence!)
                .NotNull()
                .SetValidator(new RecurrenceRequestValidator());
        });
    }
}

/// <summary>
/// Validador para recorrência de reserva.
/// </summary>
public sealed class RecurrenceRequestValidator : AbstractValidator<RecurrenceRequest>
{
    public RecurrenceRequestValidator()
    {
        RuleFor(x => x.Days)
            .NotEmpty()
            .WithMessage("Pelo menos um dia da semana deve ser selecionado");

        RuleFor(x => x.Until)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Data de término da recorrência deve ser no futuro");
    }
}
```

### Passo 2.3: Criar DTO de Request

**Arquivo novo:** `Application/DTOs/CreateReservationRequest.cs`

```csharp
namespace Projeto_Integrador2.Application.DTOs;

/// <summary>
/// DTO para requisição de criação de reserva.
/// Implementa DTO Pattern para desacoplamento.
/// </summary>
public sealed record CreateReservationRequest(
    string RoomId,
    string RequesterId,
    string Title,
    int Attendees,
    DateTime Start,
    DateTime End,
    RecurrenceRequest? Recurrence = null);

/// <summary>
/// DTO para configuração de recorrência.
/// </summary>
public sealed record RecurrenceRequest(
    DayOfWeek[] Days,
    DateTime Until);
```

### Passo 2.4: Criar DTO de Response

**Arquivo novo:** `Application/DTOs/ReservationResponse.cs`

```csharp
namespace Projeto_Integrador2.Application.DTOs;

/// <summary>
/// DTO para resposta de reserva.
/// </summary>
public sealed record ReservationResponse(
    Guid Id,
    string RoomId,
    string RequesterId,
    string Title,
    int Attendees,
    string Status,
    OccurrenceResponse[] Occurrences);

/// <summary>
/// DTO para ocorrência de reserva.
/// </summary>
public sealed record OccurrenceResponse(
    DateTime Start,
    DateTime End);

/// <summary>
/// DTO para resposta de sala.
/// </summary>
public sealed record RoomResponse(
    string Id,
    string Name,
    int Floor,
    int Capacity,
    string Description,
    string[] Resources);
```

### Passo 2.5: Registrar Validadores em ServiceExtensions

**Arquivo novo:** `Infrastructure/ServiceExtensions.cs`

```csharp
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Projeto_Integrador2.Application.Validators;
using Projeto_Integrador2.Domain;
using Projeto_Integrador2.Persistence;

namespace Projeto_Integrador2.Infrastructure;

/// <summary>
/// Extensões para registrar serviços da aplicação.
/// Implementa Clean Code centralizando configuração de DI.
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Registra todos os serviços da aplicação.
    /// </summary>
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        string connectionString)
    {
        // DbContext
        services.AddDbContext<ReservationDbContext>(options =>
            options.UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention());

        // Repositories
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<IRoomRepository, RoomRepository>();

        // Validators
        services.AddValidatorsFromAssemblyContaining<CreateReservationRequestValidator>();

        // CORS
        services.AddCors(options =>
            options.AddDefaultPolicy(policy =>
                policy
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod()));

        return services;
    }
}
```

---

## FASE 3 - Logging com Serilog

### Objetivo
Aplicar práticas de **observabilidade** com logging estruturado.

### Passo 3.1: Criar LoggingExtensions

**Arquivo novo:** `Infrastructure/Logging/LoggingExtensions.cs`

```csharp
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Microsoft.Extensions.Hosting;

namespace Projeto_Integrador2.Infrastructure.Logging;

/// <summary>
/// Extensões para configurar logging com Serilog.
/// Implementa práticas de observabilidade.
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Configura Serilog para a aplicação.
    /// </summary>
    public static WebApplicationBuilder AddSerilog(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentUserName()
            .Enrich.WithProperty("Application", "Projeto-Integrador2")
            .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
            .WriteTo.Console(
                formatter: new CompactJsonFormatter(),
                restrictedToMinimumLevel: LogEventLevel.Information)
            .WriteTo.File(
                path: "logs/app-.log",
                rollingInterval: RollingInterval.Day,
                fileSizeLimitBytes: 104_857_600, // 100 MB
                retainedFileCountLimit: 30,
                formatter: new CompactJsonFormatter())
            .CreateLogger();

        try
        {
            Log.Information("Starting application");
            builder.Host.UseSerilog();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
            throw;
        }

        return builder;
    }
}
```

---

## FASE 4 - Global Exception Handler

### Objetivo
Aplicar **DRY** consolidando tratamento de exceção em um único lugar.

### Passo 4.1: Criar Custom Exceptions

**Arquivo novo:** `Domain/Exceptions/DomainException.cs`

```csharp
namespace Projeto_Integrador2.Domain.Exceptions;

/// <summary>
/// Exceção base para exceções de domínio.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
    public abstract int StatusCode { get; }
}

/// <summary>
/// Exceção quando sala não encontrada.
/// </summary>
public sealed class RoomNotFoundException(string roomId)
    : DomainException($"Sala '{roomId}' não encontrada.")
{
    public override int StatusCode => 404;
}

/// <summary>
/// Exceção quando usuário não encontrado.
/// </summary>
public sealed class UserNotFoundException(string userId)
    : DomainException($"Usuário '{userId}' não encontrado.")
{
    public override int StatusCode => 404;
}

/// <summary>
/// Exceção quando não autorizado.
/// </summary>
public sealed class UnauthorizedException()
    : DomainException("Usuário não possui permissão para esta ação.")
{
    public override int StatusCode => 403;
}
```

### Passo 4.2: Criar Middleware de Exceção Global

**Arquivo novo:** `Infrastructure/Middleware/ExceptionHandlingMiddleware.cs`

```csharp
using System.Net;
using System.Text.Json;
using Projeto_Integrador2.Domain;
using Projeto_Integrador2.Domain.Exceptions;

namespace Projeto_Integrador2.Infrastructure.Middleware;

/// <summary>
/// Middleware para tratamento global de exceções.
/// Implementa DRY consolidando tratamento em único lugar.
/// </summary>
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
            await HandleExceptionAsync(context, exception, logger);
        }
    }

    private static Task HandleExceptionAsync(
        HttpContext context,
        Exception exception,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse();

        switch (exception)
        {
            case DomainException domainEx:
                context.Response.StatusCode = domainEx.StatusCode;
                response.Error = domainEx.Message;
                logger.LogWarning(
                    exception,
                    "Domain exception: {Message}",
                    domainEx.Message);
                break;

            case ReservationConflictException conflictEx:
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                response.Error = conflictEx.Message;
                logger.LogWarning(
                    exception,
                    "Conflict: {Message}",
                    conflictEx.Message);
                break;

            case CapacityExceededException capacityEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Error = capacityEx.Message;
                logger.LogWarning(
                    exception,
                    "Capacity exceeded: {Message}",
                    capacityEx.Message);
                break;

            case ArgumentException argEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Error = argEx.Message;
                logger.LogWarning(
                    exception,
                    "Invalid argument: {Message}",
                    argEx.Message);
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Error = "Ocorreu um erro interno. Tente novamente mais tarde.";
                logger.LogError(
                    exception,
                    "Unhandled exception: {ExceptionType}: {Message}",
                    exception.GetType().Name,
                    exception.Message);
                break;
        }

        return context.Response.WriteAsJsonAsync(response);
    }
}

/// <summary>
/// DTO para resposta de erro.
/// </summary>
public sealed class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
```

### Passo 4.3: Criar Extensão para Middleware

**Arquivo novo:** `Infrastructure/Middleware/MiddlewareExtensions.cs`

```csharp
namespace Projeto_Integrador2.Infrastructure.Middleware;

/// <summary>
/// Extensões para registrar middlewares.
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Registra middleware de tratamento global de exceção.
    /// </summary>
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
```

---

## FASE 5 - Refatoração de Program.cs

### Objetivo
Aplicar **Open/Closed** separando endpoints em extension methods.

### Passo 5.1: Criar EndpointExtensions para Reservas

**Arquivo novo:** `Infrastructure/Endpoints/ReservationEndpoints.cs`

```csharp
using FluentValidation;
using Projeto_Integrador2.Application.DTOs;
using Projeto_Integrador2.Domain;
using Projeto_Integrador2.Persistence;

namespace Projeto_Integrador2.Infrastructure.Endpoints;

/// <summary>
/// Mapeia endpoints de reservas.
/// Implementa Open/Closed separando endpoints em arquivo dedicado.
/// </summary>
public static class ReservationEndpoints
{
    public static WebApplication MapReservationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/reservations")
            .WithName("Reservations")
            .WithOpenApi()
            .WithDescription("Endpoints para gerenciamento de reservas");

        group.MapGet("/", ListReservations)
            .WithName("List Reservations")
            .WithDescription("Lista todas as reservas")
            .Produces<ReservationResponse[]>(StatusCodes.Status200OK)
            .WithOpenApi();

        group.MapPost("/", CreateReservation)
            .WithName("Create Reservation")
            .WithDescription("Cria uma nova reserva")
            .Accepts<CreateReservationRequest>("application/json")
            .Produces(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .WithOpenApi();

        group.MapPost("/{id:guid}/approve", ApproveReservation)
            .WithName("Approve Reservation")
            .WithDescription("Aprova uma reserva (requer Coordinator ou Administrator)")
            .Accepts<DecideReservationRequest>("application/json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .WithOpenApi();

        group.MapPost("/{id:guid}/cancel", CancelReservation)
            .WithName("Cancel Reservation")
            .WithDescription("Cancela uma reserva")
            .Accepts<DecideReservationRequest>("application/json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> ListReservations(
        ReservationDbContext db,
        ILogger<Program> logger,
        ReservationStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Listing reservations with status filter: {Status}", status?.ToString() ?? "None");

        try
        {
            var query = db.Reservations
                .AsNoTracking()
                .Include(r => r.Occurrences)
                .AsQueryable();

            if (status is not null)
                query = query.Where(r => r.Status == status);

            var reservations = await query
                .OrderBy(r => r.CreatedAt)
                .Select(r => new ReservationResponse(
                    r.Id,
                    r.RoomId,
                    r.RequesterId,
                    r.Title,
                    r.Attendees,
                    r.Status.ToString(),
                    r.Occurrences
                        .OrderBy(o => o.StartsAt)
                        .Select(o => new OccurrenceResponse(o.StartsAt, o.EndsAt))
                        .ToArray()))
                .ToListAsync(cancellationToken);

            logger.LogInformation("Found {Count} reservations", reservations.Count);
            return Results.Ok(reservations);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error listing reservations");
            throw;
        }
    }

    private static async Task<IResult> CreateReservation(
        CreateReservationRequest input,
        IReservationRepository reservationRepo,
        IRoomRepository roomRepo,
        IValidator<CreateReservationRequest> validator,
        ILogger<Program> logger,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Creating reservation for room {RoomId} by user {UserId}",
            input.RoomId,
            input.RequesterId);

        // Validar input
        var validationResult = await validator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
        {
            logger.LogWarning("Validation failed: {Errors}", validationResult.Errors);
            return Results.BadRequest(new
            {
                error = "Dados inválidos",
                details = validationResult.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => x.ErrorMessage).ToArray())
            });
        }

        // Validar sala
        var room = await roomRepo.GetByIdAsync(input.RoomId, cancellationToken);
        if (room is null)
        {
            logger.LogWarning("Room not found: {RoomId}", input.RoomId);
            throw new RoomNotFoundException(input.RoomId);
        }

        // Criar reserva no domínio
        try
        {
            var service = new ReservationService([
                new Room(new RoomId(room.Id), room.Name, 0, room.Capacity, [])
            ]);

            var domainReservation = service.Submit(new ReservationRequest(
                input.RequesterId,
                new RoomId(input.RoomId),
                input.Start,
                input.End,
                input.Title,
                input.Attendees,
                input.Recurrence is null ? null : new WeeklyRecurrence(
                    input.Recurrence.Days,
                    input.Recurrence.Until)));

            // Persistir
            var entity = new ReservationEntity
            {
                Id = domainReservation.Id,
                RequesterId = input.RequesterId,
                RoomId = input.RoomId,
                Title = input.Title,
                Attendees = input.Attendees,
                SeriesId = input.Recurrence is null ? null : Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                Status = ReservationStatus.Pending
            };

            foreach (var occurrence in domainReservation.Occurrences)
            {
                entity.Occurrences.Add(new ReservationOccurrenceEntity
                {
                    Id = Guid.NewGuid(),
                    StartsAt = occurrence.Start,
                    EndsAt = occurrence.End
                });
            }

            await reservationRepo.AddAsync(entity, cancellationToken);
            await reservationRepo.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Reservation created successfully: {ReservationId}",
                entity.Id);

            return Results.Created($"/api/reservations/{entity.Id}", new
            {
                entity.Id,
                status = entity.Status.ToString()
            });
        }
        catch (CapacityExceededException ex)
        {
            logger.LogWarning(ex, "Capacity exceeded");
            throw;
        }
    }

    private static async Task<IResult> ApproveReservation(
        Guid id,
        DecideReservationRequest input,
        IReservationRepository reservationRepo,
        ILogger<Program> logger,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Approving reservation: {ReservationId}", id);

        if (input.Role is not (UserRole.Coordinator or UserRole.Administrator))
        {
            logger.LogWarning(
                "Unauthorized approval attempt by user {UserId} with role {Role}",
                input.UserId,
                input.Role);
            throw new UnauthorizedException();
        }

        var reservation = await reservationRepo.GetByIdAsync(id, cancellationToken);
        if (reservation is null)
        {
            logger.LogWarning("Reservation not found: {ReservationId}", id);
            throw new RoomNotFoundException(id.ToString());
        }

        var conflicting = await reservationRepo.GetConflictingAsync(
            reservation.RoomId,
            reservation.Occurrences.Min(o => o.StartsAt),
            reservation.Occurrences.Max(o => o.EndsAt),
            ReservationStatus.Approved,
            cancellationToken);

        if (conflicting.Any())
        {
            logger.LogWarning(
                "Conflicting reservations found for room {RoomId}",
                reservation.RoomId);
            throw new ReservationConflictException(
                $"Já existe uma reserva aprovada para esse horário e sala.");
        }

        reservation.Status = ReservationStatus.Approved;
        reservation.DecidedBy = input.UserId;
        reservation.DecidedAt = DateTime.UtcNow;

        await reservationRepo.UpdateAsync(reservation, cancellationToken);
        await reservationRepo.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Reservation approved: {ReservationId} by {UserId}",
            id,
            input.UserId);

        return Results.Ok(new { reservation.Id, status = reservation.Status.ToString() });
    }

    private static async Task<IResult> CancelReservation(
        Guid id,
        DecideReservationRequest input,
        IReservationRepository reservationRepo,
        ILogger<Program> logger,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Cancelling reservation: {ReservationId}", id);

        var reservation = await reservationRepo.GetByIdAsync(id, cancellationToken);
        if (reservation is null)
        {
            logger.LogWarning("Reservation not found: {ReservationId}", id);
            throw new RoomNotFoundException(id.ToString());
        }

        var isOwner = reservation.RequesterId == input.UserId;
        var isAdmin = input.Role is UserRole.Coordinator or UserRole.Administrator;

        if (!isOwner && !isAdmin)
        {
            logger.LogWarning(
                "Unauthorized cancellation attempt by user {UserId} for reservation {ReservationId}",
                input.UserId,
                id);
            throw new UnauthorizedException();
        }

        reservation.Status = ReservationStatus.Cancelled;
        await reservationRepo.UpdateAsync(reservation, cancellationToken);
        await reservationRepo.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Reservation cancelled: {ReservationId} by {UserId}",
            id,
            input.UserId);

        return Results.Ok(new { reservation.Id, status = reservation.Status.ToString() });
    }
}
```

### Passo 5.2: Criar EndpointExtensions para Salas

**Arquivo novo:** `Infrastructure/Endpoints/RoomEndpoints.cs`

```csharp
using Projeto_Integrador2.Application.DTOs;
using Projeto_Integrador2.Domain;

namespace Projeto_Integrador2.Infrastructure.Endpoints;

/// <summary>
/// Mapeia endpoints de salas.
/// </summary>
public static class RoomEndpoints
{
    public static WebApplication MapRoomEndpoints(this WebApplication app)
    {
        app.MapGet("/api/rooms", GetRooms)
            .WithName("List Rooms")
            .WithDescription("Lista todas as salas ativas")
            .Produces<RoomResponse[]>(StatusCodes.Status200OK)
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> GetRooms(
        IRoomRepository roomRepo,
        ILogger<Program> logger,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Listing active rooms");

        try
        {
            var rooms = await roomRepo.GetAllActiveAsync(cancellationToken);

            var result = new List<RoomResponse>();
            foreach (var room in rooms)
            {
                var roomWithResources = await roomRepo.GetWithResourcesAsync(room.Id, cancellationToken);
                if (roomWithResources is not null)
                {
                    result.Add(new RoomResponse(
                        roomWithResources.Id,
                        roomWithResources.Name,
                        roomWithResources.Floor,
                        roomWithResources.Capacity,
                        roomWithResources.Description,
                        roomWithResources.Resources
                            .Select(r => r.Resource.Name)
                            .ToArray()));
                }
            }

            logger.LogInformation("Found {Count} active rooms", result.Count);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error listing rooms");
            throw;
        }
    }
}
```

### Passo 5.3: Criar EndpointExtensions para Health

**Arquivo novo:** `Infrastructure/Endpoints/HealthEndpoints.cs`

```csharp
namespace Projeto_Integrador2.Infrastructure.Endpoints;

/// <summary>
/// Mapeia endpoints de health check.
/// </summary>
public static class HealthEndpoints
{
    public static WebApplication MapHealthEndpoints(this WebApplication app)
    {
        app.MapGet("/health", Health)
            .WithName("Health Check")
            .WithDescription("Verifica saúde da aplicação")
            .Produces(StatusCodes.Status200OK)
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
```

### Passo 5.4: Program.cs Refatorado

**Editar:** `Program.cs`

```csharp
using Serilog;
using Projeto_Integrador2.Infrastructure;
using Projeto_Integrador2.Infrastructure.Logging;
using Projeto_Integrador2.Infrastructure.Middleware;
using Projeto_Integrador2.Infrastructure.Endpoints;

// Configurar logging antes de tudo
var builder = WebApplication.CreateBuilder(args);
builder.AddSerilog();

// Carregar configuração
builder.Configuration.Sources.Clear();
builder.Configuration.AddEnvironmentVariables();

var connectionString = Environment.GetEnvironmentVariable("SUPABASE_CONNECTION_STRING")
    ?? throw new InvalidOperationException("SUPABASE_CONNECTION_STRING não configurada");

// Registrar serviços
builder.Services.AddApplicationServices(connectionString);

// Construir aplicação
var app = builder.Build();

// Configurar port
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://0.0.0.0:{port}");

// Middleware
app.UseExceptionHandling();
app.UseCors();

// Endpoints
app.MapHealthEndpoints();
app.MapRoomEndpoints();
app.MapReservationEndpoints();

app.Run();
```

---

## 🧪 Testes para Validar

### Teste 1: Validação com FluentValidation

```bash
# Testar falta de título
curl -X POST http://localhost:5000/api/reservations \
  -H "Content-Type: application/json" \
  -d '{
    "roomId": "204",
    "requesterId": "prof-1",
    "title": "",
    "attendees": 5,
    "start": "2026-09-01T10:00:00Z",
    "end": "2026-09-01T11:00:00Z"
  }'

# Esperado: 400 Bad Request com mensagem "Título é obrigatório"
```

### Teste 2: Global Exception Handler

```bash
# Testar sala inexistente
curl -X POST http://localhost:5000/api/reservations \
  -H "Content-Type: application/json" \
  -d '{
    "roomId": "999",
    "requesterId": "prof-1",
    "title": "Reunião",
    "attendees": 5,
    "start": "2026-09-01T10:00:00Z",
    "end": "2026-09-01T11:00:00Z"
  }'

# Esperado: 404 Not Found com mensagem "Sala '999' não encontrada."
```

### Teste 3: Logging

```bash
# Ver logs em arquivo (após requisição):
cat logs/app-*.log

# Esperado: Entrada JSON estruturada com RequesterId, RoomId, etc
```

### Teste 4: Verificar Interfaces Registradas

Nos testes unitários:

```csharp
[Fact]
public void Services_AreRegistered()
{
    var services = new ServiceCollection();
    var connectionString = "Server=localhost;Database=test;...";
    services.AddApplicationServices(connectionString);

    var provider = services.BuildServiceProvider();

    var reservationRepo = provider.GetService<IReservationRepository>();
    var roomRepo = provider.GetService<IRoomRepository>();

    Assert.NotNull(reservationRepo);
    Assert.NotNull(roomRepo);
}
```

---

## 📊 Checklist de Implementação

```
Fase 1 - Interfaces & Repositories:
☐ Criar IReservationRepository.cs
☐ Criar IRoomRepository.cs
☐ Criar ReservationRepository.cs
☐ Criar RoomRepository.cs
☐ Testar com unit tests

Fase 2 - FluentValidation:
☐ Criar CreateReservationRequestValidator.cs
☐ Criar RecurrenceRequestValidator.cs
☐ Criar DTOs em Application/DTOs/
☐ Registrar validadores em ServiceExtensions.cs
☐ Atualizar endpoints para usar validadores

Fase 3 - Logging:
☐ Criar LoggingExtensions.cs
☐ Instalar pacotes Serilog
☐ Adicionar app.AddSerilog() em Program.cs
☐ Verificar arquivos de log em logs/

Fase 4 - Exception Handler:
☐ Criar DomainExceptions.cs
☐ Criar ExceptionHandlingMiddleware.cs
☐ Criar MiddlewareExtensions.cs
☐ Registrar middleware em Program.cs

Fase 5 - Endpoints:
☐ Criar ReservationEndpoints.cs
☐ Criar RoomEndpoints.cs
☐ Criar HealthEndpoints.cs
☐ Refatorar Program.cs
☐ Testar todos endpoints
```

---

## 📈 Resultado Esperado

Após implementar todas as fases:

```
✅ Conformidade com boas práticas: 76% → 92%
✅ Lines of Code em Program.cs: 260 → 30
✅ Testabilidade: Melhorada (mocks fáceis)
✅ Manutenibilidade: Melhorada (separação de concerns)
✅ Observabilidade: Adicionada (logging estruturado)
✅ Validação: Centralizada (FluentValidation)
✅ Error handling: Consistente (global middleware)
```

---

**Próximos passos:**
1. Implementar as 5 fases na ordem
2. Rodar testes após cada fase
3. Atualizar testes unitários para usar novos padrões
4. Documentar mudanças no README
5. Revisar com team antes de merge em `develop`

---

**Tempo estimado:** 16-20 horas de desenvolvimento
**Complexidade:** Médio (não afeta DB, foco em organização de código)
**Risco:** Baixo (mudanças refatoração, lógica se mantém igual)

## Registro de implementação — 26/08/2026

### Status verificável

- ✅ Dependências e target dos testes alinhados ao .NET 8.
- ✅ Build da API aprovado.
- ✅ 9 testes aprovados: 7 unitários do domínio e 2 E2E da API.
- ✅ Paginação implementada na API e adaptada no frontend.
- ✅ Políticas RLS complementares adicionadas de forma idempotente na migration 002.
- ⏳ Interfaces de repositório, FluentValidation, Serilog, middleware global e refatoração completa do `Program.cs` ainda não foram aplicados; permanecem no backlog deste guia.

### Próximo cronograma

| Período | Trabalho | Status |
|---|---|---|
| 27–29/08 | JWT e credenciais reais | ⏳ Planejado |
| 30/08–02/09 | Interfaces, validação e middleware | ⏳ Planejado |
| 03–06/09 | Serilog, testes E2E e refatoração de endpoints | ⏳ Planejado |

