using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Tasque.Interfaces;
using Tasque.Models;
using Tasque.Repositories;

namespace Tasque.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite("Data Source=taskque.db"));
            return services;
        }

        public static IServiceCollection AddCustomServices(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<ITaskService, TaskService>();
            services.AddScoped<ITenantProvider, TenantProvider>();
            return services;
        }

        public static IServiceCollection AddAuthenticationAndAuthorization(this IServiceCollection services, IConfiguration config)
        {
            var auth = config.GetSection("Authentication");

            services.Configure<MultiTenantSettings>(config);

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddOpenIdConnect(options =>
            {
                options.Authority = auth["Authority"];
                options.ClientId = auth["ClientId"];
                options.ClientSecret = auth["ClientSecret"];
                options.ResponseType = "code";
                options.SaveTokens = true;
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");

                options.Events.OnTokenValidated = ClaimsTransformer.Transform;
            });
            return services;
        }
    }
}
