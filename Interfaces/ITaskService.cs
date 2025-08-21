using Tasque.Models;
using TaskStatus = Tasque.Models.TaskStatus;

namespace Tasque.Interfaces
{
    public interface ITaskService
    {
        IEnumerable<TaskItem> GetTasksForUser(string userEmail, bool isAdmin);
        IEnumerable<TaskItem> GetAllTasks();
        TaskItem GetTaskById(int id);
        void CreateTask(TaskItem task, string userEmail);
        void UpdateTask(TaskItem task, TaskItem updatedTask);
        void DeleteTask(TaskItem task);
        IEnumerable<TaskItem> Search(string searchTitle, TaskStatus? status, TaskPriority? priority, DateTime? dueDate, string assignedTo);
        void UpdateStatus(int id, TaskStatus status);
        void UpdateTask(TaskItem updatedTask);
        IEnumerable<string> GetAllAssignees();
    }
}
