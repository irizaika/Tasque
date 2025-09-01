using Tasque.Services;

namespace Tasque;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var services = builder.Services;
        var config = builder.Configuration;

        // --- MVC & Controllers ---
        services.AddControllersWithViews();

        // --- Infrastructure ---
        services.AddDatabase(config);
        services.AddCustomServices();

        // --- Authentication & Authorization ---
        services.AddAuthenticationAndAuthorization(config);

        var app = builder.Build();

        // --- Middleware Pipeline ---
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapStaticAssets();
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();

        app.Run();
    }
}
