namespace Tasque.Models
{
    public class MultiTenantSettings
    {
        public Dictionary<string, TenantGroupConfig> TenantGroups { get; set; }
    }

    public class TenantGroupConfig
    {
        public string AdminsGroupId { get; set; }
        public string UsersGroupId { get; set; }
    }
}
