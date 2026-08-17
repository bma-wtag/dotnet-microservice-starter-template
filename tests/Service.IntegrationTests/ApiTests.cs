using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Service.Application.Examples;

namespace Service.IntegrationTests;

[Collection(ApiTestGroup.Name)]
public sealed class ApiTests(ApiFixture fixture)
{
    [Theory]
    [InlineData("/health")]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task HealthEndpoints_ReturnHealthy(string path)
    {
        var response = await fixture.Client.GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ExampleEndpoint_RejectsAnonymousRequest()
    {
        var response = await fixture.Client.GetAsync(
            $"/api/examples/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateAndGetExample_WithKeycloakToken_PersistsItem()
    {
        using var client = await CreateAuthenticatedClientAsync();

        var createResponse = await client.PostAsJsonAsync(
            "/api/examples",
            new CreateExampleRequest("integration example"));

        Assert.True(
            createResponse.StatusCode == HttpStatusCode.Created,
            $"Expected 201 but received {(int)createResponse.StatusCode}: {string.Join(", ", createResponse.Headers.WwwAuthenticate)}");
        var created = await createResponse.Content.ReadFromJsonAsync<ExampleItemDto>();
        Assert.NotNull(created);

        var persisted = await client.GetFromJsonAsync<ExampleItemDto>(
            $"/api/examples/{created.Id}");
        Assert.Equal(created, persisted);
    }

    [Fact]
    public async Task CreateExample_WithInvalidInput_ReturnsProblemDetails()
    {
        using var client = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync(
            "/api/examples",
            new CreateExampleRequest(string.Empty));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        using var identityClient = new HttpClient { BaseAddress = new Uri(fixture.KeycloakBaseAddress) };
        using var tokenResponse = await identityClient.PostAsync(
            "/realms/service/protocol/openid-connect/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = "service-api",
                ["username"] = "service-user",
                ["password"] = "service",
            }));
        tokenResponse.EnsureSuccessStatusCode();
        var token = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();

        var client = fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!.AccessToken);
        return client;
    }

    private sealed record TokenResponse([property: JsonPropertyName("access_token")] string AccessToken);
}
