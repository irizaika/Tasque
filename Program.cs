using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Tasque.Interfaces;
using Tasque.Repositories;
using Tasque.Services;

namespace Tasque
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var config = builder.Configuration.GetSection("Authentication");

            // --- MVC ---
            builder.Services.AddControllersWithViews();

            // --- Infrastructure ---
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite("Data Source=taskque.db"));

            // --- Custom Services ---
            builder.Services.AddScoped<ITaskService, TaskService>();
            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            builder.Services.AddScoped<ITenantProvider, TenantProvider>(); // to do change it

            // --- Authentication & Authorization ---
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddOpenIdConnect(options =>
            {
                options.Authority = config["Authority"];
                options.ClientId = config["ClientId"];
                options.ClientSecret = config["ClientSecret"];
                options.ResponseType = "code";
                options.SaveTokens = true;
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");

                options.Events.OnTokenValidated = context =>
                {
                    var identity = (ClaimsIdentity)context.Principal.Identity;

                    // Log all claims (for debugging)
                    foreach (var claim in context.Principal.Claims)
                        Console.WriteLine($"{claim.Type}: {claim.Value}");

                    var adminGroupId = config["AdminsGroupId"];
                    var userGroupId = config["UsersGroupId"];
                    var userGroups = context.Principal.FindAll("groups").Select(c => c.Value);

                    if (userGroups.Contains(adminGroupId))
                        identity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
                    else if (userGroups.Contains(userGroupId))
                        identity.AddClaim(new Claim(ClaimTypes.Role, "User"));

                    var nameClaim = context.Principal.FindFirst("name")?.Value;
                    if (!string.IsNullOrEmpty(nameClaim))
                        identity.AddClaim(new Claim(ClaimTypes.Name, nameClaim));

                    var emailClaim = context.Principal.FindFirst(ClaimTypes.Email)?.Value;
                    if (!string.IsNullOrEmpty(emailClaim))
                        identity.AddClaim(new Claim(ClaimTypes.Email, emailClaim));

                    return Task.CompletedTask;
                };
            });

            var app = builder.Build();

            // --- Middleware Pipeline ---
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication(); // Required before UseAuthorization
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
