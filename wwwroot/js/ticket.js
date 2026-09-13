// Copy reference number to clipboard
function copyToClipboard(text) {
    navigator.clipboard.writeText(text).then(() => {
        const button = event.target.closest('button');
        const originalHTML = button.innerHTML;
        button.innerHTML = '<i class="fas fa-check me-2"></i> Copied!';
        button.classList.remove('btn-dark');
        button.classList.add('btn-success');

        setTimeout(() => {
            button.innerHTML = originalHTML;
            button.classList.remove('btn-success');
            button.classList.add('btn-dark');
        }, 2000);
    });
}

// Print summary
function printPage(refNo) {
    const printContent = document.querySelector('.step-content').cloneNode(true);
    const printWindow = window.open('', '_blank');

    const htmlContent = `
        <html>
            <head>
                <title>Ticket Summary - ${refNo}</title>
                <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" rel="stylesheet">
                <style>
                    body { font-family: Arial, sans-serif; padding: 20px; }
                    .print-header { text-align: center; margin-bottom: 30px; border-bottom: 2px solid #c4252a; padding-bottom: 20px; }
                    .print-section { margin-bottom: 20px; border: 1px solid #ddd; padding: 15px; border-radius: 5px; }
                    .print-label { font-weight: bold; color: #c4252a; margin-bottom: 5px; }
                    .ref-number { font-size: 24px; font-weight: bold; color: #c4252a; text-align: center; margin: 20px 0; }
                    @media print {
                        .no-print { display: none; }
                        .print-section { break-inside: avoid; }
                    }
                </style>
            </head>
            <body>
                <div class="print-header">
                    <h2>ELSEWEDY ELECTRIC</h2>
                    <h3>Report Summary</h3>
                    <div class="ref-number">Reference: ${refNo}</div>
                    <p>Generated on: ${new Date().toLocaleString()}</p>
                </div>
                ${printContent.innerHTML}
            </body>
        </html>
    `;

    printWindow.document.write(htmlContent);
    printWindow.document.close();
    printWindow.focus();
    printWindow.print();
}

// Password strength indicator
function initPasswordStrength() {
    const passwordInput = document.getElementById('passwordInput');
    if (passwordInput) {
        passwordInput.addEventListener('input', function () {
            const password = this.value;
            const requirements = document.querySelectorAll('.password-requirements li i');

            // Reset all requirements
            requirements.forEach(icon => {
                icon.className = 'far fa-circle text-muted me-2';
            });

            // Check requirements
            if (password.length >= 8) {
                requirements[0].className = 'fas fa-check-circle text-success me-2';
            }

            if (/[A-Z]/.test(password)) {
                requirements[1].className = 'fas fa-check-circle text-success me-2';
            }

            if (/[0-9]/.test(password)) {
                requirements[2].className = 'fas fa-check-circle text-success me-2';
            }

            if (/[^A-Za-z0-9]/.test(password)) {
                requirements[3].className = 'fas fa-check-circle text-success me-2';
            }
        });
    }
}

// Initialize when page loads
document.addEventListener('DOMContentLoaded', function () {
    initPasswordStrength();
});