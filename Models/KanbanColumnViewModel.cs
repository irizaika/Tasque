namespace Tasque.Models
{
    public class KanbanColumnViewModel
    {
        public string Title { get; }
        public string HeaderCss { get; }
        public IEnumerable<TaskItem> Tasks { get; }
        public string GroupKey { get; }
        public string Status { get; }

        public KanbanColumnViewModel(string title, string headerCss, IEnumerable<TaskItem> tasks, string groupKey, string status)
        {
            Title = title;
            HeaderCss = headerCss;
            Tasks = tasks;
            GroupKey = groupKey;
            Status = status;
        }
    }
}
