document.addEventListener("DOMContentLoaded", function () {
    let draggedItem = null;

    // Bind dragstart on all current and future tasks
    function bindDragAndDrop() {
        document.querySelectorAll(".kanban-item").forEach(item => {
            item.addEventListener("dragstart", function (e) {
                draggedItem = this;
                e.dataTransfer.setData("text/plain", this.dataset.id);
            });

            item.addEventListener("dragend", function () {
                draggedItem = null;
            });
        });
    }

    // Bind click for close buttons
    function bindCloseTaskButtons() {
        document.querySelectorAll(".close-task-btn").forEach(btn => {
            btn.removeEventListener("click", closeTaskHandler); // prevent duplicates
            btn.addEventListener("click", closeTaskHandler);
        });
    }

    // Handle Close Task button click
    function closeTaskHandler(event) {
        event.preventDefault();
        const btn = event.currentTarget;
        const taskId = btn.getAttribute("data-task-id");

        if (!confirm("Mark this task as closed?")) return;

        fetch(`/Kanban/CloseTask/${taskId}`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "RequestVerificationToken": document.querySelector('input[name="__RequestVerificationToken"]').value
            }
        }).then(response => {
            if (response.ok) {
                const taskElem = btn.closest(".kanban-item");
                if (taskElem) {
                    taskElem.dataset.status = "Closed";

                    // Update button to reopen
                    updateTaskButton(taskElem, "Closed");

                    // Move into Closed column
                    const closedCol = document.querySelector(`.kanban-column[data-status="Closed"][data-group-key="${taskElem.closest('[data-group-key]').dataset.groupKey}"] .card-body`);
                    if (closedCol) {
                        closedCol.appendChild(taskElem);
                    }

                    bindReopenTaskButtons();
                }
            } else {
                alert("Failed to close task");
            }
        });
    }

    function reopenTaskHandler(event) {
        event.preventDefault();
        const btn = event.currentTarget;
        const taskId = btn.getAttribute("data-task-id");

        if (!confirm("Reopen this task?")) return;

        fetch(`/Kanban/ReopenTask/${taskId}`, {
            method: "POST",
            headers: { "Content-Type": "application/json" }
        }).then(response => {
            if (response.ok) {
                const taskElem = btn.closest(".kanban-item");
                if (taskElem) {
                    taskElem.dataset.status = "ToDo";

                    // Update button back to edit
                    updateTaskButton(taskElem, "ToDo");

                    // Move to ToDo column
                    const todoCol = document.querySelector(`.kanban-column[data-status="ToDo"][data-group-key="${taskElem.closest('[data-group-key]').dataset.groupKey}"] .card-body`);
                    if (todoCol) {
                        todoCol.appendChild(taskElem);
                    }

                    bindEditTaskButtons();
                    bindCloseTaskButtons();
                }
            } else {
                alert("Failed to reopen task");
            }
        });
    }

    function bindReopenTaskButtons() {
        document.querySelectorAll(".reopen-task-btn").forEach(btn => {
            btn.removeEventListener("click", reopenTaskHandler);
            btn.addEventListener("click", reopenTaskHandler);
        });
    }


    // Update task button based on status
    function updateTaskButton(taskElem, status) {
        const actionsSpan = taskElem.querySelector('.task-actions');
        if (!actionsSpan) return;

        if (status === 'Done') {
            actionsSpan.innerHTML = `
        <button type="button" class="btn btn-sm btn-outline-success close-task-btn" data-task-id="${taskElem.dataset.id}" title="Close Task">
          <i class="bi bi-check-circle"></i>
        </button>
      `;
        } else if (status === 'Closed') {
            actionsSpan.innerHTML = `
        <button type="button" class="btn btn-sm btn-outline-success reopen-task-btn" data-task-id="${taskElem.dataset.id}" title="Close Task">
          <i class="bi bi-arrow-counterclockwise"></i>
        </button>
      `;
        } else {
            actionsSpan.innerHTML = `
        <button type="button" class="btn btn-sm btn-outline-secondary edit-task-btn" data-bs-toggle="modal" data-bs-target="#editTaskModal" data-task-id="${taskElem.dataset.id}">
          <i class="bi bi-pencil"></i>
        </button>
      `;
        }
    }

    // Setup columns to handle dragover and drop
    function bindColumns() {
        document.querySelectorAll(".kanban-column").forEach(column => {
            column.addEventListener("dragover", function (e) {
                e.preventDefault();
                this.classList.add("drop-target");
            });

            column.addEventListener("dragleave", function () {
                this.classList.remove("drop-target");
            });

            column.addEventListener("drop", function (e) {
                e.preventDefault();
                this.classList.remove("drop-target");

                if (!draggedItem) return;

                const taskId = draggedItem.dataset.id;
                const newStatus = this.dataset.status;
                const groupKey = this.dataset.groupKey || null;
                const groupBy = this.closest("[data-groupby]")?.dataset.groupby || null;

                // Append dragged item into new column's card body
                this.querySelector(".card-body").appendChild(draggedItem);

                // Update status attribute on task element
                draggedItem.setAttribute("data-status", newStatus);

                // Send update to backend
                fetch(`/Kanban/UpdateTaskFromDrag`, {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({
                        id: taskId,
                        status: newStatus,
                        groupBy: groupBy,
                        groupValue: groupKey
                    })
                }).then(res => {
                    if (!res.ok) alert("Update failed");
                });
                showSpinner();

                // Update button inside task depending on status
                updateTaskButton(draggedItem, newStatus);

                // Re-bind close buttons for new buttons
                bindCloseTaskButtons();
                bindEditTaskButtons();
                hideSpinner();

                draggedItem = null;
            });
        });
    }
    // Initial binding on page load
    bindDragAndDrop();
    bindCloseTaskButtons();
    bindColumns();
    bindEditTaskButtons();
    bindReopenTaskButtons();
});

function bindEditTaskButtons() {
    document.querySelectorAll(".edit-task-btn").forEach(btn => {
        btn.removeEventListener("click", editTaskHandler);
        btn.addEventListener("click", editTaskHandler);
    });
}

function editTaskHandler(e) {
    const btn = e.currentTarget;
    const item = btn.closest(".edit-task-item");

    if (!item) return;

    const status = item.dataset.status;
    const priority = item.dataset.priority;
    const title = item.dataset.title;
    const description = item.dataset.description;
    const group = item.dataset.group;
    const dueDate = item.dataset.duedate;
    const id = item.dataset.id;

    const modal = document.getElementById("editTaskModal");

    modal.querySelector('input[name="Id"]').value = id;
    modal.querySelector('input[name="Title"]').value = title;
    modal.querySelector('textarea[name="Description"]').value = description;
    modal.querySelector('input[name="Group"]').value = group;
    modal.querySelector('input[name="DueDate"]').value = dueDate;

    const statusMap = { "ToDo": "0", "InProgress": "1", "Done": "2", "Closed": "3" };
    const statusSelect = modal.querySelector('select[name="Status"]');
    if (statusSelect) {
        statusSelect.value = statusMap[status] || "";
    }

    const priorityMap = { "Low": "0", "Medium": "1", "High": "2" };
    const prioritySelect = modal.querySelector('select[name="Priority"]');
    if (prioritySelect) {
        prioritySelect.value = priorityMap[priority];
    }
}



function showSpinner() {
    document.getElementById("globalSpinner").classList.remove("d-none");
}

function hideSpinner() {
    document.getElementById("globalSpinner").classList.add("d-none");
}