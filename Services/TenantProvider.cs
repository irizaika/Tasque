using Tasque.Interfaces;

namespace Tasque.Services
{
    public class TenantProvider : ITenantProvider
    {
        private readonly IHttpContextAccessor _ctx;

        public TenantProvider(IHttpContextAccessor ctx)
        {
            _ctx = ctx;
        }

        public string TenantId
        {
            get               
            {
                return  _ctx.HttpContext?.User.FindFirst("tenant_id")?.Value ?? "";
            }
        }
    }
}
