using Microsoft.EntityFrameworkCore;
using Tasque.Models;

namespace Tasque.Repositories
{
    public class TaskDbContext : DbContext
    {
        public TaskDbContext(DbContextOptions<TaskDbContext> options) : base(options)
        {
        }

        public DbSet<TaskItem> Tasks { get; set; }
    }

}
