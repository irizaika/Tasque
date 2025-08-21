namespace Tasque.Models
{
    public class TaskDashboardViewModel
    {
        public List<TaskItem> Tasks { get; set; } = new List<TaskItem>();
        public string CurrentUserEmail { get; set; } = "";
        public List<string> UserEmailList { get; set; } = new List<string>();
    }

}
