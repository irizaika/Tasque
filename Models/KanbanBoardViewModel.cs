namespace Tasque.Models
{
    public class KanbanBoardViewModel
    {
        public string GroupBy { get; set; } = "None";
        public List<KanbanGroup> GroupedTasks { get; set; } = new List<KanbanGroup> ();     
    }
}