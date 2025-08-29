document.addEventListener("DOMContentLoaded", function () {
    let draggedItem = null;

    // -------------------- DRAG & DROP --------------------
    document.addEventListener("dragstart", e => {
        const item = e.target.closest(".kanban-item");
        if (!item) return;
        draggedItem = item;
        e.dataTransfer.setData("text/plain", item.dataset.id);
    });

    document.addEventListener("dragend", () => {
        draggedItem = null;
    });

    document.querySelectorAll(".kanban-column").forEach(col => {
        col.addEventListener("dragover", e => {
            e.preventDefault();
            col.classList.add("drop-target");
        });
        col.addEventListener("dragleave", () => col.classList.remove("drop-target"));
        col.addEventListener("drop", e => {
            e.preventDefault();
            col.classList.remove("drop-target");
            if (!draggedItem) return;

            const newStatus = col.dataset.status;
            moveTask(draggedItem, newStatus, col);
        });
    });

    // -------------------- CLICK HANDLERS (delegated) --------------------
    document.addEventListener("click", e => {
        if (e.target.closest(".close-task-btn")) handleCloseTask(e);
        if (e.target.closest(".reopen-task-btn")) handleReopenTask(e);
        if (e.target.closest(".edit-task-btn")) handleEditTask(e);
    });

    // -------------------- HELPERS --------------------
    function moveTask(taskElem, newStatus, colElem) {
        const taskId = taskElem.dataset.id;
        const groupKey = colElem.dataset.groupKey || null;
        const groupBy = colElem.closest("[data-groupby]")?.dataset.groupby || null;

        // Move in DOM
        colElem.querySelector(".card-body").appendChild(taskElem);
        taskElem.dataset.status = newStatus;

        // Update button
        renderTaskActions(taskElem, newStatus);

        // Notify backend
        fetch(`/Kanban/UpdateTaskFromDrag`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ id: taskId, status: newStatus, groupBy, groupValue: groupKey })
        }).then(res => {
            if (!res.ok) alert("Update failed");
        });
    }

    function renderTaskActions(taskElem, status) {
        const actions = taskElem.querySelector(".task-actions");
        if (!actions) return;

        const buttons = {
            "ToDo": `<button class="btn btn-sm btn-outline-secondary edit-task-btn" data-task-id="${taskElem.dataset.id}" data-bs-toggle="modal" data-bs-target="#editTaskModal"><i class="bi bi-pencil"></i></button>`,
            "InProgress": `<button class="btn btn-sm btn-outline-secondary edit-task-btn" data-task-id="${taskElem.dataset.id}" data-bs-toggle="modal" data-bs-target="#editTaskModal"><i class="bi bi-pencil"></i></button>`,
            "Done": `<button class="btn btn-sm btn-outline-success close-task-btn" data-task-id="${taskElem.dataset.id}"><i class="bi bi-check-circle"></i></button>`,
            "Closed": `<button class="btn btn-sm btn-outline-warning reopen-task-btn" data-task-id="${taskElem.dataset.id}"><i class="bi bi-arrow-counterclockwise"></i></button>`
        };

        actions.innerHTML = buttons[status] || "";
    }

    // -------------------- ACTION HANDLERS --------------------
    function handleCloseTask(e) {
        const btn = e.target.closest(".close-task-btn");
        const taskId = btn.dataset.taskId;
        if (!confirm("Mark this task as closed?")) return;

        fetch(`/Kanban/CloseTask/${taskId}`, { method: "POST" })
            .then(r => r.ok ? moveTask(btn.closest(".kanban-item"), "Closed", getColumn("Closed", btn)) : alert("Failed"));
    }

    function handleReopenTask(e) {
        const btn = e.target.closest(".reopen-task-btn");
        const taskId = btn.dataset.taskId;
        if (!confirm("Reopen this task?")) return;

        fetch(`/Kanban/ReopenTask/${taskId}`, { method: "POST" })
            .then(r => r.ok ? moveTask(btn.closest(".kanban-item"), "ToDo", getColumn("ToDo", btn)) : alert("Failed"));
    }

    function handleEditTask(e) {
        const btn = e.target.closest(".edit-task-btn");
        const item = btn.closest(".kanban-item");
        if (!item) return;

        const modal = document.getElementById("editTaskModal");
        modal.querySelector('input[name="Id"]').value = item.dataset.id;
        modal.querySelector('input[name="Title"]').value = item.dataset.title;
        modal.querySelector('textarea[name="Description"]').value = item.dataset.description;
        modal.querySelector('input[name="Group"]').value = item.dataset.group;
        modal.querySelector('input[name="DueDate"]').value = item.dataset.duedate;

        const statusMap = { "ToDo": "0", "InProgress": "1", "Done": "2", "Closed": "3" };
        modal.querySelector('select[name="Status"]').value = statusMap[item.dataset.status] || "";

        const priorityMap = { "Low": "0", "Medium": "1", "High": "2" };
        modal.querySelector('select[name="Priority"]').value = priorityMap[item.dataset.priority] || "";
    }

    function getColumn(status, elem) {
        const groupKey = elem.closest("[data-group-key]")?.dataset.groupKey;
        return document.querySelector(`.kanban-column[data-status="${status}"][data-group-key="${groupKey}"]`);
    }
});
