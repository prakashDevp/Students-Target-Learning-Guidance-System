// Base URL: backend + frontend are same host on Render
const API_BASE = "/api/students";

let currentStudentId = null;
let chartInstance = null;

// Helper: show message box
function showMessage(text, type = "success") {
    const box = document.getElementById("messageBox");
    box.classList.remove("hidden", "success", "error");
    box.classList.add(type === "error" ? "error" : "success");
    box.textContent = text;

    setTimeout(() => {
        box.classList.add("hidden");
    }, 3000);
}

// Helper: update status pill + emoji
function updateStatusVisuals(status) {
    const pill = document.getElementById("statusPill");
    const emoji = document.getElementById("statusEmoji");

    pill.classList.remove("status-neutral", "status-good", "status-warn", "status-bad");

    switch (status) {
        case "OnTrack":
            pill.textContent = "On Track";
            pill.classList.add("status-good");
            emoji.textContent = "🚀";
            break;
        case "NeedsImprovement":
            pill.textContent = "Needs Improvement";
            pill.classList.add("status-warn");
            emoji.textContent = "📈";
            break;
        case "VeryHard":
            pill.textContent = "Very Hard / Almost Impossible";
            pill.classList.add("status-bad");
            emoji.textContent = "😓";
            break;
        default:
            pill.textContent = "No Data";
            pill.classList.add("status-neutral");
            emoji.textContent = "🙂";
            break;
    }
}

// Load dashboard info from API
async function loadDashboard() {
    if (!currentStudentId) return;

    try {
        const res = await fetch(`${API_BASE}/${currentStudentId}/dashboard`);
        if (!res.ok) {
            console.warn("Dashboard not available yet");
            return;
        }

        const data = await res.json();

        document.getElementById("cardTarget").textContent = data.targetCgpa.toFixed(2);
        document.getElementById("cardCurrent").textContent = data.currentCgpa.toFixed(2);
        document.getElementById("cardCompleted").textContent = data.completedSemesters;
        document.getElementById("cardRemaining").textContent = data.remainingSemesters;
        document.getElementById("cardRequired").textContent = data.requiredAverage.toFixed(2);

        document.getElementById("statusText").textContent = data.status || "";
        document.getElementById("suggestionText").textContent = data.suggestion || "";

        updateStatusVisuals(data.status);
    } catch (err) {
        console.error("Error loading dashboard:", err);
    }
}

// Load semester results and update chart + history table
async function loadSemesterResults() {
    if (!currentStudentId) return;

    try {
        const res = await fetch(`${API_BASE}/${currentStudentId}/semester-results`);
        if (!res.ok) {
            console.warn("No semester results yet");
            return;
        }

        const semData = await res.json();
        const labels = semData.map(r => `Sem ${r.semesterNumber}`);
        const gpas = semData.map(r => r.semesterGpa);

        // History table
        const body = document.getElementById("historyBody");
        body.innerHTML = "";
        semData.forEach(r => {
            const tr = document.createElement("tr");
            const tdSem = document.createElement("td");
            const tdGpa = document.createElement("td");
            tdSem.textContent = r.semesterNumber;
            tdGpa.textContent = r.semesterGpa.toFixed(2);
            tr.appendChild(tdSem);
            tr.appendChild(tdGpa);
            body.appendChild(tr);
        });

        // Chart.js
        const ctx = document.getElementById("gpaTrendChart").getContext("2d");

        if (chartInstance) {
            chartInstance.destroy();
        }

        if (labels.length === 0) {
            // no data yet
            chartInstance = null;
            return;
        }

        chartInstance = new Chart(ctx, {
            type: "line",
            data: {
                labels,
                datasets: [{
                    label: "Semester GPA",
                    data: gpas,
                    tension: 0.3,
                    fill: false
                }]
            },
            options: {
                plugins: {
                    legend: {
                        labels: {
                            color: "#e5e7eb",
                            font: { size: 11 }
                        }
                    }
                },
                scales: {
                    x: {
                        ticks: { color: "#9ca3af", font: { size: 10 } },
                        grid: { display: false }
                    },
                    y: {
                        ticks: { color: "#9ca3af", font: { size: 10 } },
                        min: 0,
                        max: 10,
                        grid: { color: "rgba(148,163,184,0.25)" }
                    }
                }
            }
        });
    } catch (err) {
        console.error("Error loading semester results:", err);
    }
}

// FORM: Create student
document.getElementById("createStudentForm").addEventListener("submit", async (e) => {
    e.preventDefault();

    const name = document.getElementById("name").value.trim();
    const department = document.getElementById("department").value.trim();
    const email = document.getElementById("email").value.trim();
    const targetCgpa = parseFloat(document.getElementById("targetCgpa").value);
    const totalSemesters = parseInt(document.getElementById("totalSem").value, 10);

    if (!name || isNaN(targetCgpa)) {
        showMessage("Please enter name and target CGPA.", "error");
        return;
    }

    try {
        const res = await fetch(API_BASE, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                name,
                department: department || null,
                email: email || null,
                targetCgpa,
                totalSemesters: totalSemesters || 8
            })
        });

        if (!res.ok) {
            showMessage("Failed to create student. Check API logs.", "error");
            return;
        }

        const data = await res.json();
        currentStudentId = data.id;
        document.getElementById("studentId").textContent = currentStudentId;

        showMessage("Student created successfully! Now add semester GPAs.", "success");

        await loadDashboard();
        await loadSemesterResults();
    } catch (err) {
        console.error("Error creating student:", err);
        showMessage("Error creating student.", "error");
    }
});

// FORM: Add / update semester result
document.getElementById("semesterForm").addEventListener("submit", async (e) => {
    e.preventDefault();

    if (!currentStudentId) {
        showMessage("Create a student first.", "error");
        return;
    }

    const semNumber = parseInt(document.getElementById("semNumber").value, 10);
    const semGpa = parseFloat(document.getElementById("semGpa").value);

    if (isNaN(semNumber) || isNaN(semGpa)) {
        showMessage("Please enter valid semester number and GPA.", "error");
        return;
    }

    try {
        const res = await fetch(`${API_BASE}/${currentStudentId}/semester-results`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                semesterNumber: semNumber,
                semesterGpa: semGpa
            })
        });

        if (!res.ok) {
            showMessage("Failed to save semester result.", "error");
            return;
        }

        showMessage(`Semester ${semNumber} GPA saved.`, "success");

        await loadDashboard();
        await loadSemesterResults();
    } catch (err) {
        console.error("Error saving semester result:", err);
        showMessage("Error saving semester result.", "error");
    }
});

// On page load: nothing to load yet, just neutral status
updateStatusVisuals(null);
