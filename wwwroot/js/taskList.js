document.addEventListener("DOMContentLoaded", () => {
    const table = document.querySelector("table");
    const tbody = document.getElementById("taskTableBody");
    const groupBySelect = document.getElementById("groupBySelect");
    const headers = table.querySelectorAll("thead th");

    // Extract all tasks into state
    let tasks = Array.from(document.querySelectorAll(".task-row")).map(row => ({
        id: row.dataset.id,
        html: row.outerHTML,
        assignedto: row.dataset.assignedto || "Unassigned",
        priority: row.dataset.priority || "No Priority",
        group: row.dataset.group || "No Group",
        status: row.dataset.status || "No Status"
    }));

    // === Event Delegation ===
    tbody.addEventListener("click", e => {
        if (e.target.closest(".edit-description-btn")) handleEditDescriptionTask(e);
        if (e.target.closest(".edit-task-btn")) handleEditTask(e);
    });

    function handleEditDescriptionTask(e) {
        const btn = e.target.closest(".edit-description-btn");
        if (!btn) return;

        document.getElementById("editTaskId").value = btn.dataset.taskId;
        document.getElementById("editDescriptionText").value = btn.dataset.taskDesc;

        new bootstrap.Modal("#editDescriptionModal").show();
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

    // === Grouping ===
    groupBySelect.addEventListener("change", () => render());

    function render() {
        const key = groupBySelect.value;
        tbody.innerHTML = "";

        if (!key) {
            tbody.innerHTML = tasks.map(t => t.html).join("");
            return;
        }

        const groups = groupBy(tasks, key);
        const colspan = table.querySelector("thead tr").children.length;

        // Define fixed group order if needed
        const groupOrder = {
            "priority": ["High", "Medium", "Low"],
            "status": ["ToDo", "InProgress", "Done", "Closed"]
        };

        // Get group keys sorted by predefined order (if exists)
        let orderedGroups = Object.keys(groups);
        if (groupOrder[key]) {
            orderedGroups = groupOrder[key].filter(g => groups[g]) // keep only existing
                .concat(orderedGroups.filter(g => !groupOrder[key].includes(g))); // add extras
        }

        // Render groups in stable order
        orderedGroups.forEach(groupName => {
            const rows = groups[groupName];
            tbody.insertAdjacentHTML("beforeend", `
            <tr class="group-header table-primary">
                <td colspan="${colspan}">
                    <strong>${capitalize(key)}: ${groupName}</strong> (${rows.length} tasks)
                </td>
            </tr>
            ${rows.map(r => r.html).join("")}
        `);
        });
    }


    // === Sorting ===
    table.querySelectorAll("thead th").forEach((header, index) => {
        header.style.cursor = "pointer";
        header.addEventListener("click", () => {
            const isAsc = header.classList.toggle("asc");
            header.classList.toggle("desc", !isAsc);

            headers.forEach(h => {
                h.querySelector(".sort-arrow")?.remove();
            });

            // Add arrow to current header
            const arrow = document.createElement("span");
            arrow.classList.add("sort-arrow");
            arrow.innerHTML = isAsc ? "▲" : "▼"; // Unicode arrows
            header.appendChild(arrow);

            tasks = sortTasks(tasks, index, isAsc, header.textContent.trim());
            render();
        });
    });

    // === Helpers ===
    function groupBy(items, key) {
        return items.reduce((acc, t) => {
            const groupKey = t[key] || "Unknown";
            (acc[groupKey] ||= []).push(t);
            return acc;
        }, {});
    }

    function sortTasks(items, colIndex, asc, colName) {
        const customOrder = {
            "Priority": ["High", "Medium", "Low"],
            "Status": ["ToDo", "InProgress", "Done", "Closed"]
        };

        return [...items].sort((a, b) => {
            const valA = getCellText(a.html, colIndex);
            const valB = getCellText(b.html, colIndex);

            if (customOrder[colName]) {
                const order = customOrder[colName];
                return asc ? order.indexOf(valA) - order.indexOf(valB)
                    : order.indexOf(valB) - order.indexOf(valA);
            }
            return asc ? valA.localeCompare(valB) : valB.localeCompare(valA);
        });
    }

    function getCellText(html, colIndex) {
        const tmp = document.createElement("tr");
        tmp.innerHTML = html;
        return tmp.children[colIndex]?.textContent.trim() || "";
    }

    const capitalize = s => s.charAt(0).toUpperCase() + s.slice(1);

      // === Default Sort by Priority ===
    const defaultCol = "Priority";  // column name to sort
    const defaultAsc = true;        // true = High → Low (based on your customOrder array)

    // Find the index of the Priority column
    const defaultHeader = Array.from(headers).find(h => h.textContent.trim() === defaultCol);
    if (defaultHeader) {
        const colIndex = Array.from(headers).indexOf(defaultHeader);
        tasks = sortTasks(tasks, colIndex, defaultAsc, defaultCol);

        // Add arrow to show it's sorted
        const arrow = document.createElement("span");
        arrow.classList.add("sort-arrow");
        arrow.innerHTML = defaultAsc ? "▲" : "▼";
        defaultHeader.appendChild(arrow);
        defaultHeader.classList.add(defaultAsc ? "asc" : "desc");
    }

    // Initial render
    render();
});
