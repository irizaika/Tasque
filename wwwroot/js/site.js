// hide success message
window.addEventListener('DOMContentLoaded', () => {
    const msg = document.getElementById('floating-message');
    if (msg) {
        setTimeout(() => {
            msg.classList.add('hide');    // optional if you want to completely remove
        }, 4000); // 4 seconds
    }
});


// edit tak modal window
document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll(".edit-task-btn").forEach(btn => {
        btn.addEventListener("click", function () {
            const item = this.closest(".edit-task-item");

            // Get data from div attributes
            var status = item.getAttribute('data-status');
            var priority = item.getAttribute('data-priority');
            var title = item.getAttribute('data-title');
            var description = item.getAttribute('data-description');
            var group = item.getAttribute('data-group');
            var dueDate = item.getAttribute('data-duedate');
            var id = item.getAttribute('data-id');

            // Fill form fields
            const modal = document.getElementById("editTaskModal");

            modal.querySelector('input[name="Id"]').value = id;
            modal.querySelector('input[name="Title"]').value = title;
            modal.querySelector('textarea[name="Description"]').value = description;
            modal.querySelector('input[name="Group"]').value = group;
            modal.querySelector('input[name="DueDate"]').value = dueDate;

            // Set Status select value
            // If data-status is name, but select expects number:
            const statusMap = { "ToDo": "0", "InProgress": "1", "Done": "2", "Closed":"3" };
            var statusSelect = modal.querySelector('select[name="Status"]');
            if (statusSelect) {
                statusSelect.value = statusMap[status] || ""
            }

            // Set Priority select value
            const priorityMap = { "Low": "0", "Medium": "1", "High": "2" };
            var prioritySelect = modal.querySelector('select[name="Priority"]');
            if (prioritySelect) {
                prioritySelect.value = priorityMap[priority];
            }
        });
    });
});
