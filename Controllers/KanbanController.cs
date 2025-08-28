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

    public IActionResult KanbanBoard(string groupBy = "None")
    {
        var tasks = _taskService.GetAllTasks();

        // Select grouping strategy
        Func<TaskItem, string> keySelector = groupBy switch
        {
            "Priority" => t => t.Priority.ToString(),
            "Group" => t => string.IsNullOrWhiteSpace(t.Group) ? "No Group" : t.Group!,
            "AssignedTo" => t => string.IsNullOrWhiteSpace(t.AssignedTo) ? "Unassigned" : t.AssignedTo!,
            _ => _ => "All Tasks"
        };

        var groupedModels = tasks
            .GroupBy(keySelector)
            .Select(g => new KanbanGroup
            {
                GroupKey = g.Key,
                ToDo = g.Where(t => t.Status == TaskStatus.ToDo).ToList(),
                InProgress = g.Where(t => t.Status == TaskStatus.InProgress).ToList(),
                Done = g.Where(t => t.Status == TaskStatus.Done).ToList(),
                Closed = g.Where(t => t.Status == TaskStatus.Closed).ToList()
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
        if (update is null) return BadRequest();

        var task = _taskService.GetTaskById(update.Id);
        if (task == null) return NotFound();

        // Always update status
        if (Enum.TryParse<TaskStatus>(update.Status, out var newStatus))
        {
            task.Status = newStatus;
        }

        // Update depending on current grouping
        switch (update.GroupBy)
        {
            case "Priority":
                if (Enum.TryParse<TaskPriority>(update.GroupValue, out var priority))
                    task.Priority = priority;
                break;

            case "AssignedTo":
                task.AssignedTo = update.GroupValue == "Unassigned" ? null : update.GroupValue;
                break;

            case "Group":
                task.Group = update.GroupValue == "No Group" ? null : update.GroupValue;
                break;
        }

        _taskService.UpdateTask(task);
        return Ok(new { Message = "Task updated successfully!" });
    }

    [HttpPost]
    public IActionResult CloseTask(int id)
    {
        var task = _taskService.GetTaskById(id);
        if (task == null) return NotFound();

        task.Status = TaskStatus.Closed;
        _taskService.UpdateTask(task);

        return Ok(new { Message = "Task closed successfully!" });
    }

    [HttpPost]
    public IActionResult ReopenTask(int id)
    {
        var task = _taskService.GetTaskById(id);
        if (task == null) return NotFound();

        // Business rule: when reopening, default back to ToDo
        task.Status = TaskStatus.ToDo;
        _taskService.UpdateTask(task);

        return Ok(new { Message = "Task reopened successfully!" });
    }
}
