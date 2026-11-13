using System.Security.Claims;
using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Projeto_Integrador2.Domain;
using Projeto_Integrador2.Models;
using Projeto_Integrador2.Persistence;
using Projeto_Integrador2.Services;

namespace Projeto_Integrador2.Endpoints;

public static class ApplicationEndpoints
{
    public static WebApplication MapApplicationEndpoints(this WebApplication app)
    {
        var frontendPath = Path.Combine(app.Environment.ContentRootPath, "frontend", "reserva-salas.html");

        app.MapGet("/", () => Results.File(frontendPath, "text/html; charset=utf-8"))
            .ExcludeFromDescription();
        app.MapGet("/reserva-salas.html", () => Results.File(frontendPath, "text/html; charset=utf-8"))
            .ExcludeFromDescription();
        app.MapGet("/health", CheckHealthAsync)
            .WithTags("Health")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status503ServiceUnavailable);

        MapAuthentication(app);
        MapRooms(app);
        MapResources(app);
        MapUsers(app);
        MapReservations(app);

        return app;
    }

    private static void MapAuthentication(WebApplication app)
    {
        var group = app.MapGroup("/auth")
            .WithTags("Auth")
            .RequireRateLimiting("auth");

        group.MapPost("/login", async (
            HttpRequest request,
            IValidator<LoginRequest> validator,
            AuthService service,
            CancellationToken ct) =>
        {
            LoginRequest? input;
            try
            {
                input = await request.ReadFromJsonAsync<LoginRequest>(ct);
            }
            catch (JsonException)
            {
                return Results.BadRequest(new { error = "Payload JSON inválido." });
            }

            if (input is null)
                return Results.BadRequest(new { error = "Informe e-mail e senha." });

            if (await ValidateAsync(validator, input, ct) is { } validationError)
                return validationError;

            var response = await service.LoginAsync(input.Email, input.Password, ct);
            return response is null ? Results.Unauthorized() : Results.Ok(response);
        })
        .Produces<LoginResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status429TooManyRequests);

        group.MapPost("/register", async (
            RegisterRequest input,
            IValidator<RegisterRequest> validator,
            AuthService service,
            CancellationToken ct) =>
        {
            if (await ValidateAsync(validator, input, ct) is { } validationError)
                return validationError;

            var result = await service.RegisterAsync(input.Name, input.Email, input.Password, ct);
            return result.Status switch
            {
                ResultStatus.Conflict => Results.Conflict(new { error = result.Error }),
                ResultStatus.Ok => Results.Created("/auth/register", new { Id = result.Value }),
                _ => Results.BadRequest(new { error = result.Error })
            };
        })
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status429TooManyRequests);
    }

    private static void MapRooms(WebApplication app)
    {
        var group = app.MapGroup("/api/rooms")
            .WithTags("Rooms")
            .RequireRateLimiting("api");

        group.MapGet("/", async (
            RoomService service, ClaimsPrincipal principal, bool? includeInactive, CancellationToken ct) =>
        {
            if (includeInactive == true && !principal.IsInRole(UserRole.Administrator.ToString()))
                return Results.Forbid();

            return Results.Ok(await service.GetRoomsAsync(includeInactive == true, ct));
        });

        group.MapPost("/", async (
            UpsertRoomRequest input,
            IValidator<UpsertRoomRequest> validator,
            RoomService service,
            CancellationToken ct) =>
        {
            if (await ValidateAsync(validator, input, ct, ruleSets: ["default", "Create"]) is { } validationError)
                return validationError;

            return ToWriteResult(
                await service.CreateRoomAsync(input, ct),
                id => Results.Created($"/api/rooms/{id}", new { Id = id }));
        }).RequireAuthorization(AdminPolicy());

        group.MapPut("/{id}", async (
            string id,
            UpsertRoomRequest input,
            IValidator<UpsertRoomRequest> validator,
            RoomService service,
            CancellationToken ct) =>
        {
            if (await ValidateAsync(validator, input, ct) is { } validationError)
                return validationError;

            return ToWriteResult(await service.UpdateRoomAsync(id, input, ct), () => Results.Ok(new { Id = id }));
        }).RequireAuthorization(AdminPolicy());

        group.MapPost("/{id}/toggle-active", async (string id, RoomService service, CancellationToken ct) =>
            ToWriteResult(await service.ToggleActiveAsync(id, ct), active => Results.Ok(new { Id = id, Active = active })))
            .RequireAuthorization(AdminPolicy());
    }

    private static void MapResources(WebApplication app)
    {
        var group = app.MapGroup("/api/resources")
            .WithTags("Resources")
            .RequireRateLimiting("api");

        group.MapGet("/", async (ResourceService service, CancellationToken ct) =>
            Results.Ok(await service.GetResourcesAsync(ct)));

        group.MapPost("/", async (
            UpsertResourceRequest input,
            IValidator<UpsertResourceRequest> validator,
            ResourceService service,
            CancellationToken ct) =>
        {
            if (await ValidateAsync(validator, input, ct) is { } validationError)
                return validationError;

            return ToWriteResult(
                await service.CreateResourceAsync(input, ct),
                id => Results.Created($"/api/resources/{id}", new { Id = id }));
        }).RequireAuthorization(AdminPolicy());
    }

    private static void MapUsers(WebApplication app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .RequireRateLimiting("api");

        group.MapGet("/", async (UserService service, CancellationToken ct) =>
            Results.Ok(await service.GetUsersAsync(ct))).RequireAuthorization();

        group.MapPost("/", async (
            UpsertUserRequest input,
            IValidator<UpsertUserRequest> validator,
            UserService service,
            CancellationToken ct) =>
        {
            if (await ValidateAsync(validator, input, ct, ruleSets: ["default", "Create"]) is { } validationError)
                return validationError;

            return ToWriteResult(
                await service.CreateUserAsync(input, ct),
                id => Results.Created($"/api/users/{id}", new { Id = id }));
        }).RequireAuthorization(AdminPolicy());

        group.MapPut("/{id}", async (
            string id,
            UpsertUserRequest input,
            IValidator<UpsertUserRequest> validator,
            UserService service,
            CancellationToken ct) =>
        {
            if (await ValidateAsync(validator, input, ct) is { } validationError)
                return validationError;

            return ToWriteResult(await service.UpdateUserAsync(id, input, ct), () => Results.Ok(new { Id = id }));
        }).RequireAuthorization(AdminPolicy());

        group.MapPost("/{id}/toggle-active", async (
            string id, UserService service, CancellationToken ct) =>
            ToWriteResult(
                await service.ToggleActiveAsync(id, ct),
                active => Results.Ok(new { Id = id, Active = active })))
            .RequireAuthorization(AdminPolicy());
    }

    private static void MapReservations(WebApplication app)
    {
        var group = app.MapGroup("/api/reservations")
            .WithTags("Reservations")
            .RequireRateLimiting("api");

        group.MapGet("/", async (
            ReservationAppService service,
            ClaimsPrincipal principal,
            ReservationStatus? status,
            int? page,
            int? pageSize,
            CancellationToken ct) =>
        {
            if (status != ReservationStatus.Approved && principal.Identity?.IsAuthenticated != true)
                return Results.Unauthorized();

            return Results.Ok(await service.GetReservationsAsync(status, page ?? 1, pageSize ?? 20, ct));
        });

        group.MapPost("/", async (
            CreateReservationRequest input,
            IValidator<CreateReservationRequest> validator,
            ClaimsPrincipal principal,
            ReservationAppService service,
            CancellationToken ct) =>
        {
            if (await ValidateAsync(validator, input, ct) is { } validationError)
                return validationError;

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await service.CreateAsync(input, userId, ct);

            if (result.Status == ResultStatus.Forbidden)
                return Results.Forbid();
            if (result.Status == ResultStatus.NotFound)
                return Results.NotFound(new { error = "Sala não encontrada." });
            if (!result.IsOk)
                return Results.BadRequest(new { error = result.Error });

            var (ids, seriesId) = result.Value;
            return Results.Created($"/api/reservations/{ids[0]}", new { seriesId, count = ids.Count, ids });
        }).RequireAuthorization();

        group.MapPost("/{id:guid}/approve", async (
            Guid id,
            bool? force,
            ClaimsPrincipal principal,
            ReservationAppService service,
            CancellationToken ct) =>
        {
            var deciderId = principal.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await service.ApproveAsync(id, deciderId, force == true, ct);

            if (result.Status == ResultStatus.NotFound)
                return Results.NotFound();
            if (result.Status == ResultStatus.Conflict)
                return Results.Conflict(new { error = result.Error, conflicts = result.Value.Conflicts });
            if (!result.IsOk)
                return Results.BadRequest(new { error = result.Error });

            return Results.Ok(new { Id = id, Status = "Approved" });
        }).RequireAuthorization(CoordinatorPolicy());

        group.MapPost("/{id:guid}/reject", async (
            Guid id,
            ClaimsPrincipal principal,
            ReservationAppService service,
            CancellationToken ct) =>
        {
            var deciderId = principal.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await service.RejectAsync(id, deciderId, ct);

            if (result.Status == ResultStatus.NotFound)
                return Results.NotFound();
            if (!result.IsOk)
                return Results.BadRequest(new { error = result.Error });

            return Results.Ok(new { Id = id, Status = "Rejected" });
        }).RequireAuthorization(CoordinatorPolicy());

        group.MapPost("/{id:guid}/cancel", async (
            Guid id,
            ClaimsPrincipal principal,
            ReservationAppService service,
            CancellationToken ct) =>
        {
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var canOverride = principal.IsInRole(UserRole.Coordinator.ToString())
                || principal.IsInRole(UserRole.Administrator.ToString());
            var result = await service.CancelAsync(id, userId, canOverride, ct);

            if (result.Status == ResultStatus.NotFound)
                return Results.NotFound();
            if (result.Status == ResultStatus.Forbidden)
                return Results.Forbid();

            return Results.Ok(new { Id = id, Status = "Cancelled" });
        }).RequireAuthorization();
    }

    private static async Task<IResult?> ValidateAsync<T>(
        IValidator<T> validator,
        T instance,
        CancellationToken ct,
        string[]? ruleSets = null)
    {
        ValidationResult validation = ruleSets is null
            ? await validator.ValidateAsync(instance, ct)
            : await validator.ValidateAsync(instance, opts => opts.IncludeRuleSets(ruleSets), ct);

        if (validation.IsValid)
            return null;

        var errors = validation.Errors
            .Select(e => new { field = e.PropertyName, message = e.ErrorMessage })
            .ToArray();

        return Results.BadRequest(new
        {
            error = validation.Errors[0].ErrorMessage,
            errors
        });
    }

    private static async Task<IResult> CheckHealthAsync(ReservationDbContext db, CancellationToken ct)
    {
        var connected = await db.Database.CanConnectAsync(ct);
        return connected
            ? Results.Ok(new { status = "ok", database = "connected", timestamp = DateTime.UtcNow })
            : Results.Json(
                new { status = "degraded", database = "disconnected", timestamp = DateTime.UtcNow },
                statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    private static AuthorizeAttribute AdminPolicy() =>
        new() { Roles = UserRole.Administrator.ToString() };

    private static AuthorizeAttribute CoordinatorPolicy() =>
        new() { Roles = $"{UserRole.Coordinator},{UserRole.Administrator}" };

    private static IResult ToWriteResult<T>(
        ServiceResult<T> result, Func<T, IResult> success)
    {
        return result.Status switch
        {
            ResultStatus.Conflict => Results.Conflict(new { error = result.Error }),
            ResultStatus.NotFound => Results.NotFound(),
            ResultStatus.Ok when result.Value is not null => success(result.Value),
            _ => Results.BadRequest(new { error = result.Error })
        };
    }

    private static IResult ToWriteResult(ServiceResult result, Func<IResult> success)
    {
        return result.Status switch
        {
            ResultStatus.Conflict => Results.Conflict(new { error = result.Error }),
            ResultStatus.NotFound => Results.NotFound(),
            ResultStatus.Ok => success(),
            _ => Results.BadRequest(new { error = result.Error })
        };
    }
}
