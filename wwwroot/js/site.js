// ── heart toggle AJAX ──
function toggleWishlist(event, btn, courseId) {
    // stop event from bubbling up to card click
    event.preventDefault();
    event.stopPropagation();

    $.post('/User/Home/ToggleWishlist', { courseId: courseId }, function (res) {
        if (res.success) {
            btn.classList.toggle('wishlisted');
            const icon = btn.querySelector('i');
            if (res.added) {
                icon.classList.remove('bi-heart');
                icon.classList.add('bi-heart-fill');
                btn.title = "Remove from wishlist";
                if (typeof showAlert === "function") {
                    showAlert("Added to wishlist", "success");
                }
                // heart pop effect
                btn.style.transform = "scale(1.3)";
                setTimeout(() => btn.style.transform = "", 200);
            } else {
                icon.classList.remove('bi-heart-fill');
                icon.classList.add('bi-heart');
                btn.title = "Add to wishlist";
                if (typeof showAlert === "function") {
                    showAlert("Removed from wishlist", "info");
                }
            }
        } else {
            if (res.message === "Please login first") {
                window.location.href = '/User/Auth/StudentLogin';
            } else {
                if (typeof showAlert === "function") {
                    showAlert(res.message || "Failed to update wishlist", "danger");
                }
            }
        }
    });
}

