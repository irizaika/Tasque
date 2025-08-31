# Taskque

Taskque is a **Kanban-style task management web application** built for demo and portfolio purposes by **Irina Zaika**.  
It provides a clean drag-and-drop interface to organize tasks and integrates **Microsoft Entra ID (Azure Active Directory)** for secure authentication and authorization.

⚠️ **Note**: Taskque is intended for **demonstration and educational use only**.  
It is **not production-ready** — data may be reset or deleted at any time.

---

## ✨ Features

### 📝 Task Management
- Create, edit, and delete tasks
- Assign tasks to users
- Set priorities (Low, Medium, High)
- Add due dates
- Track task progress via status: *To Do, In Progress, Done, Closed*

### 📊 Kanban Board
- Drag-and-drop tasks between columns
- Group tasks by:
  - Assigned User
  - Priority
  - Group
  - None (flat view)
- Dynamic UI updates when tasks are moved

### ⚡ Task Actions
- Edit task details in a modal (pre-filled with task data)
- Close tasks directly from the **Done** column
- Automatic UI updates on status change

### 🔐 Security
- **Authentication** via Microsoft Entra ID (Azure AD)
- **Authorization** based on Azure AD roles:
  - Admins: view/manage all tasks
  - Users: manage only their own tasks
- Industry-standard **OAuth 2.0 / OpenID Connect**

---

## 🛠 Technology Stack

| Layer       | Technology |
|-------------|------------|
| Frontend    | ASP.NET Core Razor Pages, Bootstrap 5, JavaScript (Drag & Drop, Fetch API) |
| Backend     | ASP.NET Core MVC, C# |
| Database    | Entity Framework Core (SQLite) |
| Auth        | Microsoft Entra ID (Azure AD) |
| Hosting     | Local or Azure App Service |

---

## 🚀 How It Works

1. **Login**  
   - Users authenticate with Microsoft Entra ID.  
   - Roles (Admin/User) are determined by Azure AD group membership.

2. **Kanban Board**  
   - Tasks are displayed in status-based columns.  
   - Optional grouping creates swimlanes (by assignee, priority, or group).

3. **Drag & Drop**  
   - Moving a task updates its status (and group if applicable).  
   - Changes are persisted via backend API calls.

4. **Task Editing**  
   - Click the ✏️ icon to edit task details in a modal.  
   - Fields auto-fill from task attributes.

5. **Closing Tasks**  
   - Click ✅ in the Done column to close a task.  
   - Task disappears from the board after confirmation.

---

## 📡 API Endpoints

### Kanban
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET    | `/Kanban/KanbanBoard?groupBy={groupBy}` | Returns grouped Kanban view (supports grouping by `Priority`, `Group`, `AssignedTo`, or `None`) |
| POST   | `/Kanban/UpdateTaskFromDrag` | Updates task when dragged/dropped between columns or groups |
| POST   | `/Kanban/CloseTask/{id}` | Marks a task as closed |
| POST   | `/Kanban/ReopenTask/{id}` | Reopens a closed task (moves it back to **To Do**) |


### Tasks
| Method | Endpoint | Description | Notes |
|--------|----------|-------------|-------|
| GET    | `/Tasks/Index` | List tasks for user (or all if admin) | Returns `Index` view with tasks. |
| GET    | `/Tasks/Create` | Task creation form | Returns empty form for new task. |
| POST   | `/Tasks/Create` | Create a task | Requires anti-forgery token. Redirects to dashboard or referring page. |
| POST   | `/Tasks/Edit` | Update a task | Requires anti-forgery token. Only allowed for Admin or assigned user. Redirects back to referring page. |
| POST   | `/Tasks/Delete/{id}` | Delete a task | Requires anti-forgery token. Only allowed for Admin or assigned user. Redirects back to referring page. |
| GET    | `/Tasks/AdminDashboard` | Admin dashboard (all tasks) | Admin role required. |
| GET    | `/Tasks/UserDashboard` | User dashboard (own tasks) | User or Admin role required. |
| POST   | `/Tasks/UpdateDescription` | Update only task description | Requires anti-forgery token. Redirects back to referring page. |
| GET    | `/Tasks/Search` | Search/filter tasks | Returns `AdminDashboard` or `UserDashboard` view depending on role. |
| GET    | `/Tasks/Reset` | Reset dashboard | Returns default `AdminDashboard` or `UserDashboard` view. |


---

## 📂 Data Models

### `DragUpdateModel`
```json
{
  "id": 123,
  "status": "Done",
  "groupBy": "Priority",
  "groupValue": "High"
}
```
### 'TaskItem'
```json
{
  "id": 123,
  "title": "Example Task",
  "description": "Details about the task",
  "status": "ToDo",
  "priority": "Medium",
  "dueDate": "2025-08-12T00:00:00",
  "assignedTo": "user@example.com",
  "group": "Development"
}
```

---

## 🔒 Security & Privacy

Only task-related data is stored (titles, status, assignee, due dates).
No sensitive personal data (passwords, payment info, etc.).
No cookies, tracking, or analytics.
See Privacy Policy

---

## 🔐 Multi-Tenancy

Tenant Resolution:
- Taskque maps Azure AD Group Object IDs (GUIDs) → Tenant names using appsettings.json.
- Each task row includes a TenantId (currently storing the tenant name) to ensure isolation.
- Example mapping in configuration:
``` json
  "TenantGroups": {
    "22222222-1111-3333-4444-555555555555": "mytasks",
    "11111111-2222-3333-4444-555555555555": "mytasks"
  }
```
This allows multiple organizations (tenants) to share one database while keeping their data separate.
⚠️ Note: The tenant model (Tenant table) is defined in code but not yet active. Currently, tenant resolution is config-based. Future versions may support dynamic tenant provisioning.

---

## ⚠️ Known Limitations

Not production-ready
No real-time collaboration (no live sync across sessions)
No file attachments or comments
Data may be deleted at any time

---

## 🛠 Future Plans
1. Multi-tenancy Support

- Current: Shared database with TenantId column (tenant name from config mapping).
- Future: Enable dynamic tenant creation (self-service onboarding).

2. Tenant-level Admin Roles

- Global Admin: manages all tenants (system owner).

3. Enhanced Collaboration

- Real-time updates using SignalR or WebSockets.
- Notifications when tasks are assigned or updated.

4. Extended Task Features

- File attachments
- Task comments
- Labels/tags for better categorization

---

## 📖 License

This project is for portfolio/demo purposes only.
Not licensed for production use.
