using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Xunit;

namespace Projeto_Integrador2.Tests;

public sealed class ApiSecurityAndObservabilityTests(ReservationApiFactory factory)
    : IClassFixture<ReservationApiFactory>
{
    [Fact]
    public async Task Swagger_document_exposes_openapi_endpoints_and_bearer_security()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = document.RootElement;

        var paths = root.GetProperty("paths");
        Assert.True(paths.TryGetProperty("/auth/login", out _));
        Assert.True(paths.TryGetProperty("/api/reservations", out _));

        var schemes = root.GetProperty("components").GetProperty("securitySchemes");
        Assert.True(schemes.TryGetProperty("Bearer", out _));
    }

    [Fact]
    public async Task FluentValidation_rejects_invalid_payloads_and_allows_valid_puts_without_create_fields()
    {
        using var client = factory.CreateClient();

        var badRegister = await client.PostAsJsonAsync("/auth/register", new
        {
            name = "Novo Usuário",
            email = "email-invalido",
            password = "123"
        });
        Assert.Equal(HttpStatusCode.BadRequest, badRegister.StatusCode);

        using (var registerBody = JsonDocument.Parse(await badRegister.Content.ReadAsStringAsync()))
        {
            Assert.False(string.IsNullOrWhiteSpace(registerBody.RootElement.GetProperty("error").GetString()));
            Assert.True(registerBody.RootElement.GetProperty("errors").GetArrayLength() >= 2);
        }

        var teacherToken = await LoginAndGetTokenAsync(client, "teacher@senac.test", "Senha-123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", teacherToken);

        var badReservation = await client.PostAsJsonAsync("/api/reservations", new
        {
            requesterId = "teacher-1",
            roomId = "room-204",
            title = "Reserva Inválida",
            attendees = 0,
            start = "2026-10-15T19:00:00Z",
            end = "2026-10-15T18:00:00Z"
        });
        Assert.Equal(HttpStatusCode.BadRequest, badReservation.StatusCode);

        using (var reservationBody = JsonDocument.Parse(await badReservation.Content.ReadAsStringAsync()))
        {
            Assert.False(string.IsNullOrWhiteSpace(reservationBody.RootElement.GetProperty("error").GetString()));
            Assert.True(reservationBody.RootElement.GetProperty("errors").GetArrayLength() >= 2);
        }

        var adminToken = await LoginAndGetTokenAsync(client, "admin@senac.test", "Senha-123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var putRoom = await client.PutAsJsonAsync("/api/rooms/room-204", new
        {
            name = "Sala 204",
            floor = "2º andar",
            capacity = 35,
            description = "Sala atualizada sem id no payload",
            resourceIds = Array.Empty<string>()
        });
        Assert.Equal(HttpStatusCode.OK, putRoom.StatusCode);

        var putUser = await client.PutAsJsonAsync("/api/users/teacher-1", new
        {
            name = "Fernanda Lima",
            email = "teacher@senac.test",
            role = "Teacher"
        });
        Assert.Equal(HttpStatusCode.OK, putUser.StatusCode);
    }

    [Fact]
    public async Task Strict_cors_allows_configured_origin_and_blocks_untrusted_origin()
    {
        using var client = factory.CreateClient();

        using var allowedRequest = new HttpRequestMessage(HttpMethod.Get, "/health");
        allowedRequest.Headers.Add("Origin", "http://localhost:5173");
        var allowedResponse = await client.SendAsync(allowedRequest);
        Assert.Equal(HttpStatusCode.OK, allowedResponse.StatusCode);
        Assert.True(allowedResponse.Headers.TryGetValues("Access-Control-Allow-Origin", out var allowedHeaders));
        Assert.Contains("http://localhost:5173", allowedHeaders);

        using var blockedRequest = new HttpRequestMessage(HttpMethod.Get, "/health");
        blockedRequest.Headers.Add("Origin", "https://untrusted.example");
        var blockedResponse = await client.SendAsync(blockedRequest);
        Assert.Equal(HttpStatusCode.OK, blockedResponse.StatusCode);
        Assert.False(blockedResponse.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task Auth_endpoint_enforces_rate_limit_when_configured()
    {
        using var limitedFactory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("RATE_LIMIT_AUTH_PERMITS", "2"));
        using var client = limitedFactory.CreateClient();

        for (var attempt = 0; attempt < 2; attempt++)
        {
            var unauthorized = await client.PostAsJsonAsync("/auth/login", new
            {
                email = "teacher@senac.test",
                password = "wrong-password"
            });
            Assert.Equal(HttpStatusCode.Unauthorized, unauthorized.StatusCode);
        }

        var rateLimited = await client.PostAsJsonAsync("/auth/login", new
        {
            email = "teacher@senac.test",
            password = "wrong-password"
        });
        Assert.Equal(HttpStatusCode.TooManyRequests, rateLimited.StatusCode);

        using var body = JsonDocument.Parse(await rateLimited.Content.ReadAsStringAsync());
        Assert.False(string.IsNullOrWhiteSpace(body.RootElement.GetProperty("error").GetString()));
    }

    [Fact]
    public async Task Structured_logging_echoes_and_generates_correlation_id_header()
    {
        using var client = factory.CreateClient();

        var generatedResponse = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, generatedResponse.StatusCode);
        Assert.True(generatedResponse.Headers.TryGetValues("X-Correlation-Id", out var generatedValues));
        Assert.False(string.IsNullOrWhiteSpace(generatedValues.Single()));

        using var correlatedRequest = new HttpRequestMessage(HttpMethod.Get, "/health");
        correlatedRequest.Headers.Add("X-Correlation-Id", "trace-e2e-12345");
        var echoedResponse = await client.SendAsync(correlatedRequest);
        Assert.Equal(HttpStatusCode.OK, echoedResponse.StatusCode);
        Assert.True(echoedResponse.Headers.TryGetValues("X-Correlation-Id", out var echoedValues));
        Assert.Equal("trace-e2e-12345", echoedValues.Single());
    }

    private static async Task<string> LoginAndGetTokenAsync(HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/auth/login", new { email, password });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return body.RootElement.GetProperty("accessToken").GetString()!;
    }
}
