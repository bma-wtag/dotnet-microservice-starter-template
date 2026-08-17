using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Testcontainers.Keycloak;
using Testcontainers.PostgreSql;

namespace Service.IntegrationTests;

public sealed class ApiFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18-alpine")
        .WithDatabase("service_tests")
        .WithUsername("service")
        .WithPassword("service")
        .Build();

    private readonly KeycloakContainer _keycloak = new KeycloakBuilder("quay.io/keycloak/keycloak:26.3.3")
        .WithUsername("admin")
        .WithPassword("admin")
        .WithRealm(Path.Combine(AppContext.BaseDirectory, "service-realm.json"))
        .Build();

    private WebApplicationFactory<Program> _factory = null!;

    public HttpClient Client { get; private set; } = null!;

    public string KeycloakBaseAddress => _keycloak.GetBaseAddress();

    public HttpClient CreateClient() => _factory.CreateClient();

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_postgres.StartAsync(), _keycloak.StartAsync());

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Database"] = _postgres.GetConnectionString(),
                    ["Keycloak:Authority"] = $"{_keycloak.GetBaseAddress().TrimEnd('/')}/realms/service",
                    ["Keycloak:RequireHttpsMetadata"] = "false",
                });
            });
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<DbContextOptions<Service.Infrastructure.Persistence.ApplicationDbContext>>();
                services.RemoveAll<Service.Infrastructure.Persistence.ApplicationDbContext>();
                services.AddDbContext<Service.Infrastructure.Persistence.ApplicationDbContext>(options =>
                    options.UseNpgsql(_postgres.GetConnectionString()));
                services.PostConfigure<JwtBearerOptions>(
                    JwtBearerDefaults.AuthenticationScheme,
                    options =>
                    {
                        var authority = $"{_keycloak.GetBaseAddress().TrimEnd('/')}/realms/service";
                        options.Authority = authority;
                        options.MetadataAddress = $"{authority}/.well-known/openid-configuration";
                        options.ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                            options.MetadataAddress,
                            new OpenIdConnectConfigurationRetriever(),
                            new HttpDocumentRetriever { RequireHttps = false });
                        options.Audience = "service-api";
                        options.RequireHttpsMetadata = false;
                        options.TokenValidationParameters.ValidIssuer = authority;
                        options.TokenValidationParameters.ValidAudience = "service-api";
                    });
            });
        });

        Client = _factory.CreateClient();
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<Service.Infrastructure.Persistence.ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await _factory.DisposeAsync();
        await _keycloak.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}
