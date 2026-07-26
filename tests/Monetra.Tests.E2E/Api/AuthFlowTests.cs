using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Monetra.Tests.E2E.Api;

public class AuthFlowTests
{
    private readonly HttpClient _client;

    public AuthFlowTests()
    {
        _client = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };
    }

    [Fact(Skip = "E2E tests require a running API instance")]
    public async Task Register_WithValidData_ShouldReturn201()
    {
        var request = new
        {
            name = "Novo Usuário",
            email = $"teste{DateTime.UtcNow.Ticks}@email.com",
            password = "Senha@123"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact(Skip = "E2E tests require a running API instance")]
    public async Task Register_WithDuplicateEmail_ShouldReturnConflict()
    {
        var request = new
        {
            name = "Usuário Duplicado",
            email = "existente@email.com",
            password = "Senha@123"
        };

        await _client.PostAsJsonAsync("/api/v1/auth/register", request);
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact(Skip = "E2E tests require a running API instance")]
    public async Task Login_WithValidCredentials_ShouldReturnTokens()
    {
        var request = new
        {
            email = "usuario@email.com",
            password = "Senha@123"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<AuthResponse>();
        content.Should().NotBeNull();
        content!.AccessToken.Should().NotBeNullOrEmpty();
        content.RefreshToken.Should().NotBeNullOrEmpty();
    }

    private class AuthResponse
    {
        public string AccessToken { get; set; } = "";
        public string RefreshToken { get; set; } = "";
    }
}
