
//max 3 alert message at a time
const MAX_ALERTS = 3;

function showAlert(message, type = "info", duration = 3800, containerId = "alert-container") {
    const container = document.getElementById(containerId);
    if (!container) return;

    const normalizedType = normalizeAlertType(type);

    if (container.children.length >= MAX_ALERTS) {
        container.firstChild.remove();
    }

    const alert = document.createElement("div");
    alert.className = `alert alert-${normalizedType}`;

    alert.innerHTML = `
    <span class="alert-message">${message}</span>
    <button type="button" class="alert-close-btn" aria-label="Close alert"></button>
`;

    const closeBtn = alert.querySelector(".alert-close-btn");
    if (closeBtn) {
        closeBtn.addEventListener("click", function () {
            removeAlert(alert);
        });
    }

    container.appendChild(alert);

    setTimeout(() => removeAlert(alert), duration);
}

function normalizeAlertType(type) {
    const input = (type || "").toString().trim().toLowerCase();

    if (input === "success" || input === "ok") {
        return "success";
    }

    if (input === "error" || input === "danger" || input === "warning" || input === "warn" || input === "failed" || input === "fail") {
        return "danger";
    }

    if (input === "step" || input === "steps" || input === "process" || input === "note" || input === "message") {
        return "info";
    }

    return "info";
}

function removeAlert(element) {
    const alert = element.closest ? element.closest('.alert') : element;
    if (!alert) return;

    alert.classList.add("fade-out");

    setTimeout(() => alert.remove(), 300);
}
