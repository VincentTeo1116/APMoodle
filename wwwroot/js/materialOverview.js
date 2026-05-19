// materialOverview.js - Delete modal functions

let currentMaterialId = null;
let currentModuleId = null;

// Show delete modal
function showDeleteModal(button) {
    // Get data from button attributes
    currentMaterialId = button.getAttribute('data-id');
    currentModuleId = button.getAttribute('data-module-id');
    const materialTitle = button.getAttribute('data-title');
    
    // Set the title in the modal
    const deleteTitleElement = document.getElementById('deleteTitle');
    if (deleteTitleElement) {
        deleteTitleElement.innerHTML = `<strong>"${materialTitle}"</strong>`;
    }
    
    // Show the modal
    const modal = document.getElementById('deleteConfirmModal');
    if (modal) {
        modal.style.display = 'flex';
    }
}

// Hide delete modal
function hideDeleteModal() {
    const modal = document.getElementById('deleteConfirmModal');
    if (modal) {
        modal.style.display = 'none';
    }
    currentMaterialId = null;
    currentModuleId = null;
}

// Perform delete action
async function performDelete() {
    if (!currentMaterialId) return;
    
    try {
        const response = await fetch(`/FrontEnd/TeachingMaterialOverview/DeleteMaterial/${currentMaterialId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        });
        
        if (response.ok) {
            // Redirect to teaching material page
            window.location.href = `/FrontEnd/TeachingMaterial/${currentModuleId}`;
        } else {
            const error = await response.text();
            alert(`Delete failed: ${error}`);
            hideDeleteModal();
        }
    } catch (error) {
        console.error('Delete error:', error);
        alert('Delete failed: ' + error.message);
        hideDeleteModal();
    }
}

// Initialize event listeners when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    // Cancel button
    const cancelBtn = document.getElementById('deleteCancelBtn');
    if (cancelBtn) {
        cancelBtn.addEventListener('click', hideDeleteModal);
    }
    
    // Confirm button
    const confirmBtn = document.getElementById('deleteConfirmBtn');
    if (confirmBtn) {
        confirmBtn.addEventListener('click', performDelete);
    }
    
    // Click outside to close
    const overlay = document.querySelector('.delete-modal-overlay');
    if (overlay) {
        overlay.addEventListener('click', hideDeleteModal);
    }
    
    // ESC key to close
    document.addEventListener('keydown', function(e) {
        if (e.key === 'Escape') {
            hideDeleteModal();
        }
    });
});