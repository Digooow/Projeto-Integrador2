using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Projeto_Integrador2.Persistence;
using Projeto_Integrador2.Security;
using Xunit;

namespace Projeto_Integrador2.Tests;

public sealed class ReservationApiEndToEndTests : IClassFixture<ReservationApiFactory>
{
    private readonly ReservationApiFactory factory;

    public ReservationApiEndToEndTests(ReservationApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task Teacher_can_request_and_admin_can_approve_reservation_seen_in_query()
    {
        using var client = factory.CreateClient();

        var teacherToken = await LoginAsync(client, "teacher@senac.test", "Senha-123!");
        using var teacher = AuthorizedClient(teacherToken);
        var start = new DateTime(2026, 9, 1, 19, 0, 0, DateTimeKind.Utc);
        var reservationResponse = await teacher.PostAsJsonAsync("/api/reservations", new
        {
            requesterId = "teacher-1",
            roomId = "room-204",
            start,
            end = start.AddHours(2),
            title = "Aula E2E",
            responsavel = "Fernanda Lima",
            attendees = 20,
            recurrence = (object?)null
        });

        Assert.Equal(HttpStatusCode.Created, reservationResponse.StatusCode);
        using var created = JsonDocument.Parse(await reservationResponse.Content.ReadAsStringAsync());
        var reservationId = created.RootElement.GetProperty("ids")[0].GetGuid();

        var adminToken = await LoginAsync(client, "admin@senac.test", "Senha-123!");
        using var admin = AuthorizedClient(adminToken);
        var approvalResponse = await admin.PostAsync($"/api/reservations/{reservationId}/approve", null);

        Assert.Equal(HttpStatusCode.OK, approvalResponse.StatusCode);
        using var approval = JsonDocument.Parse(await approvalResponse.Content.ReadAsStringAsync());
        Assert.Equal("Approved", approval.RootElement.GetProperty("status").GetString());

        var queryResponse = await teacher.GetAsync("/api/reservations?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, queryResponse.StatusCode);
        using var query = JsonDocument.Parse(await queryResponse.Content.ReadAsStringAsync());
        var reservation = query.RootElement.GetProperty("data").EnumerateArray()
            .Single(item => item.GetProperty("id").GetGuid() == reservationId);
        Assert.Equal("Approved", reservation.GetProperty("status").GetString());
        Assert.Equal("teacher-1", reservation.GetProperty("requesterId").GetString());
    }

    private HttpClient AuthorizedClient(string token)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static async Task<string> LoginAsync(HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/auth/login", new { email, password });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return payload.RootElement.GetProperty("accessToken").GetString()!;
    }
}

public sealed class ReservationApiFactory : WebApplicationFactory<Program>
{
    private const string ConnectionString = "Host=localhost;Database=e2e";
    private const string JwtSecret = "e2e-secret-key-with-at-least-32-bytes";

    public ReservationApiFactory()
    {
        Environment.SetEnvironmentVariable("SUPABASE_CONNECTION_STRING", ConnectionString);
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", JwtSecret);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ReservationDbContext>>();
            services.AddDbContext<ReservationDbContext>(options =>
                options.UseInMemoryDatabase("reservation-api-e2e"));

            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ReservationDbContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            db.Users.AddRange(
                new UserEntity
                {
                    Id = "teacher-1",
                    Name = "Fernanda Lima",
                    Email = "teacher@senac.test",
                    PasswordHash = PasswordHasher.Hash("Senha-123!"),
                    Role = Projeto_Integrador2.Domain.UserRole.Teacher,
                    Active = true
                },
                new UserEntity
                {
                    Id = "admin-1",
                    Name = "Renata Alves",
                    Email = "admin@senac.test",
                    PasswordHash = PasswordHasher.Hash("Senha-123!"),
                    Role = Projeto_Integrador2.Domain.UserRole.Administrator,
                    Active = true
                });
            db.Rooms.Add(new RoomEntity
            {
                Id = "room-204",
                Name = "Sala 204",
                Floor = "2º andar",
                Capacity = 30,
                Description = "Sala de teste",
                Active = true
            });
            db.SaveChanges();
        });
    }
}
