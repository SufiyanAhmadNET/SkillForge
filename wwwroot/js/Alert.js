/**
 * Modern Alert System
 * Handles floating toast notifications with auto-dismiss
 */

const MAX_ALERTS = 3;
const DEFAULT_DURATION = 4000; // 4 seconds

/**
 * Main function to show an alert
 * @param {string} message - The text to display
 * @param {string} type - success, danger, warning, info
 * @param {number} duration - ms to show before auto-dismiss
 * @param {string} containerId - Target container ID
 */
function showAlert(message, type = "info", duration = DEFAULT_DURATION, containerId = "alert-container") {
    const container = document.getElementById(containerId);
    if (!container) return;

    // Normalize type to match CSS classes
    const normalizedType = normalizeAlertType(type);

    // Maintain max alerts count
    if (container.children.length >= MAX_ALERTS) {
        const oldest = container.querySelector('.alert:not(.fade-out)');
        if (oldest) removeAlert(oldest);
    }

    // Create alert element
    const alert = document.createElement("div");
    alert.className = `alert alert-${normalizedType}`;
    alert.setAttribute("role", "alert");

    alert.innerHTML = `
        <div class="alert-message">${message}</div>
        <button type="button" class="alert-close-btn" title="Close"></button>
    `;

    // Add close button functionality
    const closeBtn = alert.querySelector(".alert-close-btn");
    if (closeBtn) {
        closeBtn.addEventListener("click", () => removeAlert(alert));
    }

    // Append to container
    container.appendChild(alert);

    // Auto-dismiss logic
    if (duration > 0) {
        setTimeout(() => {
            if (alert.parentElement) removeAlert(alert);
        }, duration);
    }
}

/**
 * Maps various input types to standard bootstrap/CSS types
 */
function normalizeAlertType(type) {
    const t = (type || "").toString().toLowerCase().trim();
    
    if (["success", "ok", "saved", "verified"].includes(t)) return "success";
    if (["error", "danger", "failed", "fail"].includes(t)) return "danger";
    if (["warning", "warn", "alert"].includes(t)) return "warning";
    if (["info", "note", "message"].includes(t)) return "info";
    
    return "info";
}

/**
 * Removes an alert with a fade-out animation
 */
function removeAlert(alertElement) {
    if (!alertElement || alertElement.classList.contains("fade-out")) return;

    alertElement.classList.add("fade-out");
    
    // Wait for animation to finish before removing from DOM
    setTimeout(() => {
        if (alertElement.parentElement) {
            alertElement.remove();
        }
    }, 300);
}
