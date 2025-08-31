using Microsoft.EntityFrameworkCore;
using Tasque.Interfaces;
using Tasque.Models;

namespace Tasque.Repositories
{
    public class AppDbContext : DbContext
    {
        private readonly ITenantProvider _tenantProvider;
        public AppDbContext(DbContextOptions<AppDbContext> options, ITenantProvider tenantProvider)
            : base(options)
        {
            _tenantProvider = tenantProvider;
        }

        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Global filter for TaskItem
            modelBuilder.Entity<TaskItem>().HasQueryFilter(t => t.TenantId == _tenantProvider.TenantId);
        }

        public override int SaveChanges()
        {
            ApplyTenantToNewEntities();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyTenantToNewEntities();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void ApplyTenantToNewEntities()
        {
            var tenantId = _tenantProvider.TenantId;
            foreach (var entry in ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added))
            {
                if (entry.Properties.Any(p => p.Metadata.Name == "TenantId"))
                {
                    entry.Property("TenantId").CurrentValue = tenantId;
                }
            }
        }
    }

}
