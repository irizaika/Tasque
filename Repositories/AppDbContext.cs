using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Tasque.Models;

namespace Tasque.Repositories
{
    public class AppDbContext : DbContext
    {
        public DbSet<TaskItem> Tasks { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<TaskItem>().ToTable("Tasks");
        }
    }

}
