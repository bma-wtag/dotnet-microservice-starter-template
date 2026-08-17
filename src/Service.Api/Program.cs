using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Service.Api.Authentication;
using Service.Api.Endpoints;
using Service.Api.Errors;
using Service.Application;
using Service.Infrastructure;
using Service.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options => options.IncludeScopes = true);

var connectionString = builder.Configuration.GetConnectionString("Database")
    ?? throw new InvalidOperationException("Connection string 'Database' is required.");
var keycloak = builder.Configuration.GetRequiredSection(KeycloakOptions.SectionName).Get<KeycloakOptions>()
    ?? throw new InvalidOperationException("Keycloak configuration is required.");

builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi();
builder.Services.AddOptions<KeycloakOptions>()
    .BindConfiguration(KeycloakOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = keycloak.Authority;
        options.Audience = keycloak.Audience;
        options.RequireHttpsMetadata = keycloak.RequireHttpsMetadata;
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "preferred_username",
            RoleClaimType = KeycloakClaimsTransformation.RoleClaimType,
        };
    });
builder.Services.AddTransient<Microsoft.AspNetCore.Authentication.IClaimsTransformation, KeycloakClaimsTransformation>();
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AuthorizationPolicies.ExampleRead, policy => policy.RequireRole(keycloak.RequiredRole));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("postgresql", tags: ["ready"]);

var app = builder.Build();

if (builder.Configuration.GetValue<bool>("Database:ApplyMigrations"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => Results.Ok(new { service = "Service.Api", status = "running" }))
    .WithName("ServiceInfo")
    .WithSummary("Returns basic service information.");

app.MapHealthChecks("/health", new HealthCheckOptions());
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("ready"),
});

app.MapExampleEndpoints();

app.Run();

public partial class Program;
