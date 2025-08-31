namespace Tasque.Models
{
    public class Tenant
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "";
        public string? ConnectionString { get; set; } // only if db-per-tenant
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
