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

    // ---------- Helpers ----------
    private TaskDashboardViewModel BuildDashboard(IEnumerable<TaskItem> tasks, bool includeEmails)
    {
        return new TaskDashboardViewModel
        {
            Tasks = [.. tasks],
            CurrentUserEmail = UserEmail ?? "",
            UserEmailList = includeEmails ? [.. _taskService.GetAllAssignees()] : []
        };
    }

    private IActionResult RedirectToReferrerOr(string defaultAction, object? routeValues = null)
    {
        var referer = Request.Headers["Referer"].ToString();
        return !string.IsNullOrEmpty(referer) ? Redirect(referer) : RedirectToAction(defaultAction, routeValues);
    }

    // ---------- Actions ----------
    public IActionResult Index()
    {
        var tasks = _taskService.GetTasksForUser(UserEmail ?? "", IsAdmin);
        return View(tasks);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(TaskItem task)
    {
        _taskService.CreateTask(task, UserEmail ?? "");
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
        return RedirectToReferrerOr(nameof(UserDashboard));
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
        return RedirectToReferrerOr(nameof(UserDashboard));
    }

    [Authorize(Roles = "Admin")]
    public IActionResult AdminDashboard()
    {
        var tasks = _taskService.GetTasksForUser(UserEmail ?? "", true);
        return View(BuildDashboard(tasks, includeEmails: true));
    }

    [Authorize(Roles = "User,Admin")]
    public IActionResult UserDashboard()
    {
        var tasks = _taskService.GetTasksForUser(UserEmail ?? "", false);
        return View(BuildDashboard(tasks, includeEmails: false));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateDescription(int id, string description)
    {
        var task = _taskService.GetTaskById(id);
        if (task == null) return NotFound();
        if (!IsAdmin && task.AssignedTo != UserEmail) return Forbid();

        task.Description = description;
        _taskService.UpdateTask(task, task);

        TempData["Message"] = "Task updated successfully!";
        return RedirectToReferrerOr(nameof(UserDashboard));
    }

    public IActionResult Search(string searchTitle, TaskStatus? status, TaskPriority? priority, DateTime? dueDate, string assignedTo, string isAdminBoard)
    {
        bool adminBoard = isAdminBoard == "true";
        var searchUser = adminBoard ? assignedTo : (UserEmail ?? "");

        var tasks = _taskService.Search(searchTitle, status, priority, dueDate, searchUser);
        var vm = BuildDashboard(tasks, includeEmails: adminBoard);

        return adminBoard ? View("AdminDashboard", vm) : View("UserDashboard", vm);
    }

    public IActionResult Reset(bool isAdminBoard)
    {
        var tasks = _taskService.GetTasksForUser(UserEmail ?? "", isAdminBoard);
        var vm = BuildDashboard(tasks, includeEmails: isAdminBoard);

        return isAdminBoard ? View("AdminDashboard", vm) : View("UserDashboard", vm);
    }
}
