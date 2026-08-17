using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Service.Api.Authentication;

public sealed class KeycloakClaimsTransformation(IOptions<KeycloakOptions> options) : IClaimsTransformation
{
    public const string RoleClaimType = "role";

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
        {
            return Task.FromResult(principal);
        }

        AddRoles(identity, principal.FindFirstValue("realm_access"));

        var resourceAccess = principal.FindFirstValue("resource_access");
        if (!string.IsNullOrWhiteSpace(resourceAccess))
        {
            using var document = JsonDocument.Parse(resourceAccess);
            if (document.RootElement.TryGetProperty(options.Value.ClientId, out var client))
            {
                AddRoles(identity, client.GetRawText());
            }
        }

        return Task.FromResult(principal);
    }

    private static void AddRoles(ClaimsIdentity identity, string? accessClaim)
    {
        if (string.IsNullOrWhiteSpace(accessClaim))
        {
            return;
        }

        using var document = JsonDocument.Parse(accessClaim);
        if (!document.RootElement.TryGetProperty("roles", out var roles))
        {
            return;
        }

        foreach (var role in roles.EnumerateArray())
        {
            var value = role.GetString();
            if (value is not null && !identity.HasClaim(RoleClaimType, value))
            {
                identity.AddClaim(new Claim(RoleClaimType, value));
            }
        }
    }
}
