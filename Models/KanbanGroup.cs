namespace Tasque.Models
{
    public class KanbanGroup
    {
        public string GroupKey { get; set; }
        public List<TaskItem> ToDo { get; set; }
        public List<TaskItem> InProgress { get; set; }
        public List<TaskItem> Done { get; set; }
    }
}