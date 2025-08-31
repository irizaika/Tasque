using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Tasque.Interfaces;
using Tasque.Services;

namespace Tasque.Repositories
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlite("Data Source=taskque.db");
            return new AppDbContext(optionsBuilder.Options, new FakeTenantProvider());
        }
    }

    public class FakeTenantProvider : ITenantProvider
    {
        public string TenantId => "design-time";
    }
}

