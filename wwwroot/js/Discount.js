<script>
                    
                        function calcPrice() {
                          const price    = parseFloat(document.getElementById('originalPrice').value) || 0;
                          const pct      = parseFloat(document.getElementById('discountPct').value)  || 0;
                          const preview  = document.getElementById('pricePreview');

                          if (price <= 0) { preview.style.display = 'none'; return; }

                          const discRs = Math.round(price * pct / 100);
                          const final  = price - discRs;

                          document.getElementById('displayOriginal').textContent = '₹' + price.toLocaleString('en-IN');
                          document.getElementById('displayDiscount').textContent  = '₹' + discRs.toLocaleString('en-IN');
                          document.getElementById('displayFinal').textContent     = '₹' + final.toLocaleString('en-IN');
                          document.getElementById('displayBadge').textContent     = pct > 0 ? pct + '% OFF' : 'No discount';
                          document.getElementById('displayBadge').className       = pct > 0 ? 'badge bg-success' : 'badge bg-secondary';

                          preview.style.display = 'block';
                        }

                        document.getElementById('originalPrice').addEventListener('input', calcPrice);
                        document.getElementById('discountPct').addEventListener('input', calcPrice);
                    </script>