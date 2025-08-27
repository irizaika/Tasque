using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tasque.Interfaces;
using Tasque.Models;
using TaskStatus = Tasque.Models.TaskStatus;

[Authorize]
public class TasksController : Controller
{
    private readonly ITaskService _taskService;
    private readonly IHttpContextAccessor _context;

    public TasksController(ITaskService taskService, IHttpContextAccessor context)
    {
        _taskService = taskService;
        _context = context;
    }

    private string? UserEmail => _context.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
    private bool IsAdmin => User?.IsInRole("Admin") ?? false;

    public IActionResult Index()
    {
        var tasks = _taskService.GetTasksForUser(UserEmail ?? "", IsAdmin);
        return View(tasks);
    }

    public IActionResult Create() => View();

    [HttpPost]
    public IActionResult Create(TaskItem task)
    {
        _taskService.CreateTask(task, UserEmail??"");
        return RedirectToReferrerOr(nameof(UserDashboard));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(TaskItem updatedTask)
    {
        var task = _taskService.GetTaskById(updatedTask.Id);
        if (task == null) return NotFound();
        if (!IsAdmin && task.AssignedTo != UserEmail) return Forbid();

        _taskService.UpdateTask(task, updatedTask);
        TempData["Message"] = "Task updated successfully!";
        return RedirectToReferrerOr("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var task = _taskService.GetTaskById(id);
        if (task == null) return NotFound();
        if (!IsAdmin && task.AssignedTo != UserEmail) return Forbid();

        _taskService.DeleteTask(task);
        TempData["Message"] = "Task deleted successfully!";
        return RedirectToReferrerOr("Index");
    }

    [Authorize(Roles = "Admin")]
    public IActionResult AdminDashboard()
    {
        var allTasks = _taskService.GetTasksForUser(UserEmail ?? "", true);
        var emails = _taskService.GetAllAssignees();

        var taskListViewModel = new TaskDashboardViewModel()
        {
            Tasks = [.. allTasks],
            CurrentUserEmail = UserEmail ?? "",
            UserEmailList = [.. emails]
        };

        return View(taskListViewModel);
    }

    [Authorize(Roles = "User,Admin")]
    public IActionResult UserDashboard()
    {
        var tasks = _taskService.GetTasksForUser(UserEmail ?? "", false);

        var taskListViewModel = new TaskDashboardViewModel()
        {
            Tasks = [.. tasks],
            CurrentUserEmail= UserEmail??"",
            UserEmailList = []
        };
        return View(taskListViewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateDescription(int id, string description)
    {
        var task = _taskService.GetTaskById(id);
        if (task == null) return NotFound();
        task.Description = description;
        _taskService.UpdateTask(task, task);
        TempData["Message"] = "Task updated successfully!";
        return RedirectToReferrerOr("Index");
    }

    private IActionResult RedirectToReferrerOr(string action)
    {
        var referer = Request.Headers["Referer"].ToString();
        return !string.IsNullOrEmpty(referer) ? Redirect(referer) : RedirectToAction(action);
    }

    public IActionResult Search(string searchTitle, TaskStatus? status, TaskPriority? priority, DateTime? dueDate, string assignedTo, string isAdminBoard)
    {
        var tasks = _taskService.Search(searchTitle, status, priority, dueDate, isAdminBoard == "true" ? assignedTo: UserEmail ?? "");
        var emails = _taskService.GetAllAssignees();//todo no neew to get it all the time

        var taskListViewModel = new TaskDashboardViewModel()
        {
            Tasks = [.. tasks],
            CurrentUserEmail = UserEmail ?? "",
            UserEmailList = [.. emails]
        };
        if (isAdminBoard == "true")
        {
            return View("AdminDashboard", taskListViewModel);
        }
        return View("UserDashboard", taskListViewModel);
    }

    public IActionResult Reset(bool isAdminBoard)
    {
        var allTasks = _taskService.GetTasksForUser(UserEmail ?? "", isAdminBoard);
        var emails = _taskService.GetAllAssignees();//todo no need to get it all the time

        var taskListViewModel = new TaskDashboardViewModel()
        {
            Tasks = [.. allTasks],
            CurrentUserEmail = UserEmail ?? "",
            UserEmailList = [.. emails]
        };

        if (isAdminBoard == true)
        {
            return View("AdminDashboard", taskListViewModel);
        }
        return View("UserDashboard", taskListViewModel);
    }
}