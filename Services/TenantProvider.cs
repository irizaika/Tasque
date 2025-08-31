using Tasque.Interfaces;

namespace Tasque.Services
{
    public class TenantProvider : ITenantProvider
    {
        private readonly IHttpContextAccessor _ctx;
        private readonly IConfiguration _config;

        public TenantProvider(IHttpContextAccessor ctx, IConfiguration config)
        {
            _ctx = ctx;
            _config = config;
        }

        public string TenantId
        {
            get
            {
                var http = _ctx.HttpContext;
                if (http == null) return "";

                var groups = http.User.Claims
                 .Where(c => c.Type == "groups").ToList();

                var tenantId = groups
                    .Select(c => MapGroupToTenant(c.Value)) // Map Azure groupId → TenantId
                    .FirstOrDefault(t => !string.IsNullOrEmpty(t));

                return tenantId ?? "";
            }
        }

        private string MapGroupToTenant(string groupId)
        {
            // Tenant id mapped to group object id in the appsettings.json
            var dict = _config.GetSection("TenantGroups").Get<Dictionary<string, string>>(); 
            return dict.TryGetValue(groupId, out var tenantId) ? tenantId : "";
        }
    }
}
