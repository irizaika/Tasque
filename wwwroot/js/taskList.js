document.addEventListener("DOMContentLoaded", function () {
    const editButtons = document.querySelectorAll(".edit-description-btn");
    const taskIdField = document.getElementById("editTaskId");
    const descriptionField = document.getElementById("editDescriptionText");

    editButtons.forEach(btn => {
        btn.addEventListener("click", function () {
            const taskId = this.dataset.taskId;
            const taskDesc = this.dataset.taskDesc;

            taskIdField.value = taskId;
            descriptionField.value = taskDesc;

            // Show modal
            const modal = new bootstrap.Modal(document.getElementById("editDescriptionModal"));
            modal.show();
        });
    });
});

//// hide success message
//window.addEventListener('DOMContentLoaded', () => {
//        const msg = document.getElementById('floating-message');
//    if (msg) {
//        setTimeout(() => {
//            msg.classList.add('hide');    // optional if you want to completely remove
//        }, 4000); // 4 seconds
//    }
//});


// Group by script
document.addEventListener("DOMContentLoaded", function () {
    const groupBySelect = document.getElementById("groupBySelect");
    const tableBody = document.getElementById("taskTableBody");

    // Save original flat task list
    const tasks = Array.from(document.querySelectorAll(".task-row")).map(row => ({
        html: row.outerHTML,
        assignedto: row.dataset.assignedto || "Unassigned",
        priority: row.dataset.priority || "No Priority",
        group: row.dataset.group || "No Group",
        status: row.dataset.status || "No Status"
    }));

    groupBySelect.addEventListener("change", function () {
        const key = this.value;
        if (key) {
            renderGrouped(key);

        } else {
            renderFlat();
        }
        attachExpandRow();
    });

    function getColspan() {
        const headerRow = document.querySelector("table thead tr");
        return headerRow ? headerRow.children.length : 1;
    }

    function renderGrouped(key) {
        const colspan = getColspan();
        const grouped = tasks.reduce((acc, t) => {
            const groupKey = t[key];
            acc[groupKey] = acc[groupKey] || [];
            acc[groupKey].push(t.html);
            return acc;
        }, {});

        tableBody.innerHTML = "";
        for (const [groupName, rows] of Object.entries(grouped)) {
            tableBody.innerHTML += `
                <tr class="group-header table-primary">
                    <td colspan="${colspan}"><strong>${capitalize(key)}: ${groupName}</strong> (${rows.length} tasks)</td>
                </tr>
                ${rows.join("")}
            `;
        }
    }

    function renderFlat() {
        tableBody.innerHTML = tasks.map(t => t.html).join("");
    }

    function capitalize(str) {
        return str.charAt(0).toUpperCase() + str.slice(1);
    }
});

// sorl table items by clicking on header
document.addEventListener("DOMContentLoaded", function () {
    const table = document.querySelector("table");
    const headers = table.querySelectorAll("thead th");
    const tbody = table.querySelector("tbody");

    const customOrder = {
        "Priority": ["High", "Medium", "Low"],
        "Status": ["To Do", "In Progress", "Done"]
    };

    headers.forEach((header, index) => {
        header.style.cursor = "pointer";

        header.addEventListener("click", function () {
            const isAscending = header.classList.toggle("asc");
            header.classList.toggle("desc", !isAscending);

            // Remove arrows from other headers
            headers.forEach(h => {
                h.querySelector(".sort-arrow")?.remove();
            });

            // Add arrow to current header
            const arrow = document.createElement("span");
            arrow.classList.add("sort-arrow");
            arrow.innerHTML = isAscending ? " ▲" : " ▼"; // Unicode arrows
            header.appendChild(arrow);

            // Check if there are group headers
            const groupHeaders = tbody.querySelectorAll(".group-header");

            if (groupHeaders.length > 0) {
                let groupRows = [];

                Array.from(tbody.children).forEach(row => {
                    if (row.classList.contains("group-header")) {
                        if (groupRows.length) {
                            sortRows(groupRows, index, isAscending);
                            groupRows.forEach(r => tbody.insertBefore(r, row));
                        }
                        groupRows = [];
                    } else {
                        groupRows.push(row);
                    }
                });

                if (groupRows.length) {
                    sortRows(groupRows, index, isAscending);
                    groupRows.forEach(r => tbody.appendChild(r));
                }

            } else {
                const rows = Array.from(tbody.querySelectorAll("tr"))
                    .filter(r => !r.classList.contains("group-header"));

                sortRows(rows, index, isAscending);
                rows.forEach(row => tbody.appendChild(row));
            }
        });
    });

    function sortRows(rows, colIndex, isAscending) {
        rows.sort((a, b) => {
            const cellA = a.children[colIndex].textContent.trim();
            const cellB = b.children[colIndex].textContent.trim();

            const columnName = table.querySelectorAll("thead th")[colIndex].textContent.trim();

            if (customOrder[columnName]) {
                const order = customOrder[columnName];
                const posA = order.indexOf(cellA);
                const posB = order.indexOf(cellB);
                return isAscending ? posA - posB : posB - posA;
            }

            const aNum = parseFloat(cellA.replace(",", "."));
            const bNum = parseFloat(cellB.replace(",", "."));
            if (!isNaN(aNum) && !isNaN(bNum)) {
                return isAscending ? aNum - bNum : bNum - aNum;
            }

            return isAscending
                ? cellA.localeCompare(cellB)
                : cellB.localeCompare(cellA);
        });
    }
});




