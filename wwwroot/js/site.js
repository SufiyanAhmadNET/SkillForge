// ── GLOBAL AUTH STATE HELPER ──
function getAuthState() {
    if (typeof isAuthenticated !== 'undefined') return isAuthenticated;
    return false; // safe fallback
}

// ── GLOBAL WISHLIST INTERACTION ──
// uses delegation to work with dynamically rendered sliders/rows
$(document).on('click', '.wish-btn', function(e) {
    e.preventDefault();
    e.stopPropagation();

    const btn = this;
    const courseId = $(btn).data('course-id');
    const isUserAuth = getAuthState();

    if (!isUserAuth) {
        requireLogin("wishlist this course", false);
        return;
    }

    // prevent double clicks
    if ($(btn).hasClass('processing')) return;
    $(btn).addClass('processing');

    $.post('/User/Home/ToggleWishlist', { courseId: courseId }, function (res) {
        $(btn).removeClass('processing');

        if (res.success) {
            $(btn).toggleClass('wishlisted', res.added);
            const icon = $(btn).find('i');
            
            if (res.added) {
                icon.removeClass('bi-heart').addClass('bi-heart-fill');
                btn.title = "Remove from wishlist";
                if (typeof showAlert === "function") {
                    showAlert("Added to wishlist", "success");
                }
                // bounce effect
                $(btn).css('transform', 'scale(1.2)');
                setTimeout(() => $(btn).css('transform', ''), 200);
            } else {
                icon.removeClass('bi-heart-fill').addClass('bi-heart');
                btn.title = "Add to wishlist";
                if (typeof showAlert === "function") {
                    showAlert("Removed from wishlist", "info");
                }
            }
        } else {
            if (res.message === "Please login first") {
                requireLogin("wishlist this course", false);
            } else {
                if (typeof showAlert === "function") {
                    showAlert(res.message || "Failed to update wishlist", "danger");
                }
            }
        }
    }).fail(function() {
        $(btn).removeClass('processing');
        if (typeof showAlert === "function") {
            showAlert("An error occurred. Please try again.", "danger");
        }
    });
});

// ── global guest handler ──
function requireLogin(action, redirect = true) {
    const msg = "Please login to " + action;
    
    if (typeof showAlert === 'function') {
        showAlert(msg, "info");
    } else {
        alert(msg);
    }
    
    if (redirect) {
        setTimeout(() => {
            window.location.href = '/User/Auth/StudentLogin';
        }, 1500);
    }
}

