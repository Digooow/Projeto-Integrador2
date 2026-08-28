using System.Net;
using System.Text.Json;
using Xunit;

namespace Projeto_Integrador2.Tests;

public sealed class TvPanelEndToEndTests : IClassFixture<ReservationApiFactory>
{
    private readonly ReservationApiFactory factory;

    public TvPanelEndToEndTests(ReservationApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task Public_panel_can_load_the_page_and_approved_reservations_without_login()
    {
        using var client = factory.CreateClient();

        var page = await client.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);
        Assert.Contains("Ocupa", await page.Content.ReadAsStringAsync());

        var reservations = await client.GetAsync("/api/reservations?status=Approved&page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, reservations.StatusCode);
        using var payload = JsonDocument.Parse(await reservations.Content.ReadAsStringAsync());
        var approved = payload.RootElement.GetProperty("data").EnumerateArray().Single();
        Assert.Equal("Approved", approved.GetProperty("status").GetString());
        Assert.Equal("Aula exibida no painel", approved.GetProperty("title").GetString());
    }
}