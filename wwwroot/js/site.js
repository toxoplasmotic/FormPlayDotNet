/**
 * FormPlay Site JS
 * 
 * This script contains common functionality for the FormPlay application.
 */

// Update status badges on the reports list
function updateStatusBadges() {
    document.querySelectorAll('.status-badge').forEach(badge => {
        const status = badge.getAttribute('data-status');
        
        let className = 'badge ';
        let text = status;
        
        switch(status) {
            case 'New':
                className += 'bg-primary';
                break;
            case 'Pending':
                className += 'bg-warning text-dark';
                break;
            case 'AwaitingFinalApproval':
                className += 'bg-info text-dark';
                text = 'Awaiting Final Approval';
                break;
            case 'Approved':
                className += 'bg-success';
                break;
            case 'Aborted':
                className += 'bg-danger';
                break;
            default:
                className += 'bg-secondary';
        }
        
        badge.className = className;
        badge.textContent = text;
    });
}

// Initialize PDF interaction when the page contains a PDF container
function initializePdfInteraction() {
    const pdfContainer = document.getElementById('pdf-container');
    if (pdfContainer) {
        const pdfUrl = pdfContainer.getAttribute('data-pdf-url');
        const reportId = pdfContainer.getAttribute('data-report-id');
        const userId = pdfContainer.getAttribute('data-user-id');
        const isReadOnly = pdfContainer.getAttribute('data-readonly') === 'true';
        
        if (pdfUrl && window.PdfInteraction) {
            new PdfInteraction('pdf-container', pdfUrl, reportId, userId, isReadOnly);
        }
    }
}

// Confirm Deny action
function confirmDeny(reportId) {
    if (confirm('Are you sure you want to abort this TPS report?')) {
        window.location.href = `/TpsReport/Deny/${reportId}`;
    }
}

// Confirm Approve action
function confirmApprove(reportId) {
    if (confirm('Are you sure you want to approve this TPS report?')) {
        window.location.href = `/TpsReport/Approve/${reportId}`;
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    updateStatusBadges();
    
    // Show success message if present, and fade out after delay
    const successMessage = document.getElementById('success-message');
    if (successMessage && successMessage.textContent.trim().length > 0) {
        setTimeout(() => {
            successMessage.style.opacity = '0';
            setTimeout(() => {
                successMessage.style.display = 'none';
            }, 500);
        }, 3000);
    }
});