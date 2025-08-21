using Microsoft.AspNetCore.Mvc;
using Tasque.Interfaces;
using Tasque.Models;
using TaskStatus = Tasque.Models.TaskStatus;

namespace Tasque.Controllers;

public class KanbanController : Controller
{
    private readonly ILogger<KanbanController> _logger;
    private readonly ITaskService _taskService;

    public KanbanController(ILogger<KanbanController> logger, ITaskService taskService)
    {
        _logger = logger;
        _taskService = taskService;
    }

    //public IActionResult KanbanBoard()
    //{
    //    return View();
    //}

    //public IActionResult KanbanBoard()
    //{
    //    var tasks = _taskService.GetAllTasks(); // returns IEnumerable<Task>

    //    var model = new KanbanViewModel
    //    {
    //        ToDo = tasks.Where(t => t.Status == TaskStatus.ToDo).ToList(),
    //        InProgress = tasks.Where(t => t.Status == TaskStatus.InProgress).ToList(),
    //        Done = tasks.Where(t => t.Status == TaskStatus.Done).ToList()
    //    };

    //    return View(model);
    //}
    public IActionResult KanbanBoard(string groupBy = "None")
    {
        var tasks = _taskService.GetAllTasks();

        // Decide grouping key
        Func<TaskItem, string> keySelector = groupBy switch
        {
            "Priority" => t => t.Priority.ToString(),
            "Group" => t => string.IsNullOrWhiteSpace(t.Group) ? "No Group" : t.Group!,
            "AssignedTo" => t =>  string.IsNullOrWhiteSpace(t.AssignedTo) ? "Unassigned" : t.AssignedTo!,
            _ => _ => "All Tasks"
        };

        // Group and map into view-friendly model
        var groupedModels = tasks
            .GroupBy(keySelector)
            .Select(g => new KanbanGroup
            {
                GroupKey = g.Key,
                ToDo = g.Where(t => t.Status == TaskStatus.ToDo).ToList(),
                InProgress = g.Where(t => t.Status == TaskStatus.InProgress).ToList(),
                Done = g.Where(t => t.Status == TaskStatus.Done).ToList()
            })
            .ToList();

        var model = new KanbanBoardViewModel
        {
            GroupBy = groupBy,
            GroupedTasks = groupedModels
        };

        return View(model);
    }

    [HttpPost]
    public IActionResult UpdateTaskFromDrag([FromBody] DragUpdateModel update)
    {
        var task = _taskService.GetTaskById(update.Id);
        if (task == null) return NotFound();

        // Always update status
        task.Status = Enum.Parse<TaskStatus>(update.Status);

        // Update based on grouping
        switch (update.GroupBy)
        {
            case "Priority":
                task.Priority = Enum.Parse<TaskPriority>(update.GroupValue);
                break;
            case "AssignedTo":
                task.AssignedTo = update.GroupValue == "Unassigned" ? null : update.GroupValue;
                break;
            case "Group":
                task.Group = update.GroupValue == "No Group" ? null : update.GroupValue;
                break;
        }

        _taskService.UpdateTask(task);
        return Ok();
    }

    public class DragUpdateModel
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public string GroupBy { get; set; }
        public string GroupValue { get; set; }
    }



    //[HttpPost]
    //public IActionResult UpdateTaskStatus(int id, TaskStatus newStatus)
    //{
    //    _taskService.UpdateStatus(id, newStatus);
    //    return Ok();
    //}

    [HttpPost]
    public IActionResult CloseTask(int id)
    {
        var task = _taskService.GetTaskById(id);
        if (task == null) return NotFound();

        task.Status = TaskStatus.Closed;
        _taskService.UpdateTask(task);

        return Ok();
    }


}
