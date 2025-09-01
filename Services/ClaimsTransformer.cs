using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Tasque.Models;

namespace Tasque.Services
{
    public static class ClaimsTransformer
    {
        public static Task Transform(TokenValidatedContext context)
        {
            var identity = (ClaimsIdentity)context.Principal.Identity;
            if (identity != null)
            {

                var nameClaim = context.Principal.FindFirst("name")?.Value;
                if (!string.IsNullOrEmpty(nameClaim))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Name, nameClaim));
                }

                var emailClaim = context.Principal.FindFirst(ClaimTypes.Email)?.Value;
                if (!string.IsNullOrEmpty(emailClaim))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Email, emailClaim));
                }

                var settings = context.HttpContext.RequestServices
                                 .GetRequiredService<IOptions<MultiTenantSettings>>()
                                 .Value;

                var userGroups = context.Principal.FindAll("groups").Select(c => c.Value);

                foreach (var kvp in settings.TenantGroups)
                {
                    var tenantName = kvp.Key;
                    var tenantConfig = kvp.Value;

                    if (userGroups.Contains(tenantConfig.AdminsGroupId))
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
                        identity.AddClaim(new Claim("tenant_id", tenantName));
                        break;
                    }
                    if (userGroups.Contains(tenantConfig.UsersGroupId))
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, "User"));
                        identity.AddClaim(new Claim("tenant_id", tenantName));
                        break;
                    }
                }
            }
            else
            {
                Console.WriteLine($"Identity is null");
            }
            return Task.CompletedTask;
        }
    }
}
