document.addEventListener('DOMContentLoaded', function () {
    const originalPriceInput = document.getElementById('originalPrice');
    const discountPctInput = document.getElementById('discountPct');
    const pricePreview = document.getElementById('pricePreview');

    if (!originalPriceInput || !discountPctInput || !pricePreview) return;

    function calcPrice() {
        const price = parseFloat(originalPriceInput.value) || 0;
        const pct = parseFloat(discountPctInput.value) || 0;

        if (price <= 0) {
            pricePreview.style.display = 'none';
            return;
        }

        const discRs = Math.round(price * pct / 100);
        const final = price - discRs;

        document.getElementById('displayOriginal').textContent = '₹' + price.toLocaleString('en-IN');
        document.getElementById('displayDiscount').textContent = '₹' + discRs.toLocaleString('en-IN');
        document.getElementById('displayFinal').textContent = '₹' + final.toLocaleString('en-IN');
        
        const badge = document.getElementById('displayBadge');
        if (badge) {
            badge.textContent = pct > 0 ? pct + '% OFF' : 'No discount';
            badge.className = pct > 0 ? 'badge bg-success' : 'badge bg-secondary';
        }

        pricePreview.style.display = 'block';
    }

    originalPriceInput.addEventListener('input', calcPrice);
    discountPctInput.addEventListener('input', calcPrice);

    // Run immediately in case values are pre-filled
    calcPrice();
});
