using Tasque.Interfaces;
using Tasque.Models;
using Tasque.Repositories;
using TaskStatus = Tasque.Models.TaskStatus;

namespace Tasque.Services
{
    public class TaskService : ITaskService
    {
        private readonly AppDbContext _db;
        public TaskService(AppDbContext db)
        {
            _db = db;
        }

        public IEnumerable<TaskItem> GetTasksForUser(string userEmail, bool isAdmin)
        {
            List<TaskItem> tasks = new List<TaskItem>();
            try
            {
                if (isAdmin)
                {
                    tasks = _db.Tasks.ToList();
                }
                else
                {
                    tasks = _db.Tasks.Where(t => t.AssignedTo == userEmail).ToList();

                }
            }
            catch (Exception ex)
            { 
                Console.Write(ex.ToString());
            }

            return tasks;
        }

        public IEnumerable<TaskItem> GetAllTasks()
        {
            return _db.Tasks.ToList();
        }

        public TaskItem GetTaskById(int id)
        {
            return _db.Tasks.Find(id);
        }

        public void CreateTask(TaskItem task, string userEmail)
        {
            task.AssignedTo = userEmail;
            task.CreatedBy = userEmail;
            task.CreatedAt = DateTime.UtcNow;
            _db.Tasks.Add(task);
            _db.SaveChanges();
        }

        public void UpdateStatus(int id, TaskStatus status)
        {
            var task = GetTaskById(id);
            task.Status = status;
            _db.SaveChanges();
        }

        public void UpdateTask(TaskItem task, TaskItem updatedTask)
        {
            task.Title = updatedTask.Title;
            task.Description = updatedTask.Description;
            task.Group = updatedTask.Group;
            task.Status = updatedTask.Status;
            task.Priority = updatedTask.Priority;
            task.DueDate = updatedTask.DueDate;
            _db.SaveChanges();
        }

        public void UpdateTask(TaskItem updatedTask)
        {
            var task = GetTaskById(updatedTask.Id);
            task.Title = updatedTask.Title;
            task.Description = updatedTask.Description;
            task.Group = updatedTask.Group;
            task.Status = updatedTask.Status;
            task.Priority = updatedTask.Priority;
            task.DueDate = updatedTask.DueDate;
            _db.SaveChanges();
        }

        public void DeleteTask(TaskItem task)
        {
            _db.Tasks.Remove(task);
            _db.SaveChanges();
        }

        public IEnumerable<TaskItem> Search(string searchTitle, TaskStatus? status, TaskPriority? priority, DateTime? dueDate, string assignedTo)
        {
            var tasks = _db.Tasks.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTitle))
                tasks = tasks.Where(t =>  t.Title.ToLower().Contains(searchTitle.ToLower()) || 
                ((t.Description != null) && t.Description.ToLower().Contains(searchTitle.ToLower()))
                );

            if (status.HasValue)
                tasks = tasks.Where(t => t.Status == status);

            if (priority.HasValue)
                tasks = tasks.Where(t => t.Priority == priority.Value);

            if (dueDate.HasValue)
                tasks = tasks.Where(t => t.DueDate.HasValue && t.DueDate.Value.Date == dueDate.Value.Date);

            if (!string.IsNullOrWhiteSpace(assignedTo))
                tasks = tasks.Where(t => t.AssignedTo.Contains(assignedTo));

            return tasks;
        }

        public IEnumerable<string> GetAllAssignees()
        {
            var emails = _db.Tasks.Select(t => t.AssignedTo).Distinct().ToList();
            return emails;
        }
    }
}
