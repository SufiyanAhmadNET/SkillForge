
//max 3 alert message at a time 
const MAX_ALERTS = 3;

function showAlert(message, type = "blue", duration = 2500) {
    const container = document.getElementById("alert-container");

    if (container.children.length >= MAX_ALERTS) {
        container.firstChild.remove();
    }

    const alert = document.createElement("div");
    alert.className = `alert alert-${type} d-flex justify-content-between align-items-start`;

    alert.innerHTML = `
    <span class="me-2">${message}</span>
    <button style="border:none;background:none;font-size:18px;cursor:pointer;">×</button>
`;

    container.appendChild(alert);

    setTimeout(() => removeAlert(alert), duration);
}

function removeAlert(element) {
    const alert = element.closest ? element.closest('.alert') : element;

    alert.classList.add("fade-out");

    setTimeout(() => alert.remove(), 300);
}  
