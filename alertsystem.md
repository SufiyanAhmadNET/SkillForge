# SkillForge Alert System Documentation

The alert system provides immediate, floating notifications for user actions (success, error, info, etc.). It is designed to be non-intrusive, mobile-responsive, and production-grade.

## 1. How It Works (The Flow)

The system works through a combination of **Server-side TempData**, a **Shared Partial View**, and **Frontend Javascript/CSS**.

### A. Server-Side Trigger
When an action occurs in a Controller (like updating a profile), we set the message using `TempData`. `TempData` is ideal because it persists only until it is read once.

```csharp
// Inside a Controller Action
public IActionResult UpdateProfile() {
    // ... logic ...
    TempData["Alert"] = "Profile updated successfully!";
    TempData["AlertType"] = "success"; 
    return RedirectToAction("Profile");
}
```

### B. Centralized Detection
The system is centralized in `_Alert_Message_Partial.cshtml`. This partial is included in the main layouts. It automatically checks for the presence of alert data and triggers the display.

```html
@* _Alert_Message_Partial.cshtml *@
<div id="alert-container" class="alert-container"></div>

@if (TempData["Alert"] != null) {
    <script>
        document.addEventListener("DOMContentLoaded", function () {
            // Trigger the JS function defined in Alert.js
            showAlert("@TempData["Alert"]", "@(TempData["AlertType"] ?? "info")");
        });
    </script>
}
```

## 2. The Frontend Engine

### Floating Container (CSS)
The alerts are appended to a `fixed` container positioned at the top-right of the screen. On mobile devices, this container automatically adjusts to the top-center and takes more width for better visibility.

```css
/* alert.css logic */
.alert-container {
    position: fixed;
    top: 24px;
    right: 24px;
    z-index: 2100; /* Ensures it floats above everything */
}

.alert {
    /* Animation for smooth slide-in from right */
    animation: alert-slide-in 0.4s cubic-bezier(0.175, 0.885, 0.32, 1.275) forwards;
}
```

### Dynamic Rendering (JS)
The `showAlert` function in `Alert.js` handles the creation of the alert element, mapping the message type to the correct color theme, and managing the lifecycle of the notification.

```javascript
// Alert.js - The core logic
function showAlert(message, type = "info") {
    const container = document.getElementById("alert-container");
    
    // Create the alert element
    const alert = document.createElement("div");
    alert.className = `alert alert-${type}`;
    alert.innerHTML = `
        <div class="alert-message">${message}</div>
        <button class="alert-close-btn"></button>
    `;

    container.appendChild(alert);

    // Auto-dismiss after 4 seconds
    setTimeout(() => {
        removeAlert(alert);
    }, 4000);
}
```

## 3. Key Behaviors

*   **Auto-Dismiss:** Notifications stay for 4 seconds before fading out automatically.
*   **Manual Close:** Users can click the 'X' to dismiss a message immediately.
*   **Stacked Alerts:** If multiple actions happen quickly, the alerts will stack vertically in the container.
*   **One-Time Only:** Because it uses `TempData`, messages are cleared as soon as they are displayed, preventing them from appearing again on page refresh.
*   **Session Safety:** During logout, `TempData.Clear()` is called to ensure no old messages leak into the next user's session.
