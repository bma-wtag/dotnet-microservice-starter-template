using System.ComponentModel.DataAnnotations;

namespace Service.Api.Authentication;

public sealed class KeycloakOptions
{
    public const string SectionName = "Keycloak";

    [Required, Url]
    public string Authority { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;

    [Required]
    public string ClientId { get; init; } = string.Empty;

    [Required]
    public string RequiredRole { get; init; } = string.Empty;

    public bool RequireHttpsMetadata { get; init; } = true;
}
