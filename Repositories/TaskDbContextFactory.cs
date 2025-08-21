using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Tasque.Repositories
{
    public class TaskDbContextFactory : IDesignTimeDbContextFactory<TaskDbContext>
    {
        public TaskDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TaskDbContext>();

            // Use the same connection string you use in your app
            optionsBuilder.UseSqlite("Data Source=taskque.db");

            return new TaskDbContext(optionsBuilder.Options);
        }
    }
}

