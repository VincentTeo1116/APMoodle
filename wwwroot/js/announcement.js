// DOM Elements
let searchInput, filterDateRange, filterCreatedBy, tableBody, rows;
let modal, modalTitle, modalContent, modalClose, modalCloseBtn;
let filteredCountSpan;
let pendingDeleteId = null;
let pendingDeleteRow = null;
let pendingDeleteTitle = null;

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    initializeElements();
    attachEventListeners();
    initializeActionButtons();
    initializeCreateButton();
    updateFilteredCount();
    createDeleteModal();
});

// Initialize all DOM element references
function initializeElements() {
    searchInput = document.getElementById('searchInput');
    filterDateRange = document.getElementById('filterDateRange');
    filterCreatedBy = document.getElementById('filterCreatedBy');
    tableBody = document.getElementById('announcementTableBody');
    filteredCountSpan = document.getElementById('filteredCount');
    
    if (tableBody) {
        rows = tableBody.querySelectorAll('tr:not(.empty-row)');
    }
    
    modal = document.getElementById('announcementModal');
    modalTitle = document.getElementById('modalTitle');
    modalContent = document.getElementById('modalContent');
    modalClose = document.querySelector('.modal-close');
    modalCloseBtn = document.getElementById('modalCloseBtn');
}

// Attach main event listeners
function attachEventListeners() {
    // Search and Filter
    if (searchInput) {
        searchInput.addEventListener('input', filterTable);
    }
    if (filterDateRange) {
        filterDateRange.addEventListener('change', filterTable);
    }
    if (filterCreatedBy) {
        filterCreatedBy.addEventListener('change', filterTable);
    }
    
    // Modal close events
    if (modalClose) {
        modalClose.addEventListener('click', closeModal);
    }
    if (modalCloseBtn) {
        modalCloseBtn.addEventListener('click', closeModal);
    }
    
    // Close modal when clicking outside
    window.addEventListener('click', function(e) {
        if (e.target === modal) {
            closeModal();
        }
    });
}

// Helper function to check date filter
function checkDateFilter(createdDate, filterValue) {
    if (filterValue === 'all') return true;
    
    const announcementDate = new Date(createdDate);
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    
    switch(filterValue) {
        case 'today':
            const todayStart = new Date(today);
            const todayEnd = new Date(today);
            todayEnd.setHours(23, 59, 59, 999);
            return announcementDate >= todayStart && announcementDate <= todayEnd;
            
        case 'week':
            const weekAgo = new Date(today);
            weekAgo.setDate(today.getDate() - 7);
            return announcementDate >= weekAgo;
            
        case 'month':
            const monthAgo = new Date(today);
            monthAgo.setMonth(today.getMonth() - 1);
            return announcementDate >= monthAgo;
            
        case 'year':
            const yearAgo = new Date(today);
            yearAgo.setFullYear(today.getFullYear() - 1);
            return announcementDate >= yearAgo;
            
        default:
            return true;
    }
}

// Filter table based on search and filter criteria
function filterTable() {
    if (!searchInput || !filterDateRange || !filterCreatedBy) return;
    
    const searchTerm = searchInput.value.toLowerCase();
    const dateFilter = filterDateRange.value;
    const creatorFilter = filterCreatedBy.value;
    
    // Re-query rows to get current state
    const currentRows = tableBody.querySelectorAll('tr:not(.empty-row)');
    let visibleCount = 0;
    
    currentRows.forEach(row => {
        const title = row.querySelector('.title-cell')?.textContent.toLowerCase() || '';
        const message = row.querySelector('.message-cell')?.textContent.toLowerCase() || '';
        const creator = row.getAttribute('data-created-by') || '';
        const createdDate = row.getAttribute('data-created-date') || '';
        
        const matchesSearch = title.includes(searchTerm) || message.includes(searchTerm);
        const matchesCreator = creatorFilter === 'all' || creator === creatorFilter;
        const matchesDate = checkDateFilter(createdDate, dateFilter);
        
        if (matchesSearch && matchesCreator && matchesDate) {
            row.style.display = '';
            visibleCount++;
        } else {
            row.style.display = 'none';
        }
    });
    
    // Update filtered count
    if (filteredCountSpan) {
        filteredCountSpan.textContent = visibleCount;
    }
    
    // Show/hide empty message if no visible rows
    const emptyMessage = tableBody.querySelector('.empty-row');
    
    if (visibleCount === 0 && !emptyMessage && currentRows.length > 0) {
        const emptyRow = document.createElement('tr');
        emptyRow.className = 'empty-row';
        emptyRow.innerHTML = '<td colspan="5" class="empty-message">No matching announcements found.</td>';
        tableBody.appendChild(emptyRow);
    } else if (visibleCount > 0 && emptyMessage && emptyMessage.classList.contains('empty-row')) {
        emptyMessage.remove();
    }
}

// Update filtered count display
function updateFilteredCount() {
    const visibleRows = Array.from(tableBody.querySelectorAll('tr:not(.empty-row)'))
        .filter(row => row.style.display !== 'none');
    if (filteredCountSpan) {
        filteredCountSpan.textContent = visibleRows.length;
    }
}

// Initialize all action buttons (View, Edit, Delete)
function initializeActionButtons() {
    // View buttons
    document.querySelectorAll('.view-btn').forEach(btn => {
        btn.removeEventListener('click', handleViewClick);
        btn.addEventListener('click', handleViewClick);
    });
    
    // Edit buttons
    document.querySelectorAll('.edit-btn').forEach(btn => {
        btn.removeEventListener('click', handleEditClick);
        btn.addEventListener('click', handleEditClick);
    });
    
    // Delete buttons
    document.querySelectorAll('.delete-btn').forEach(btn => {
        btn.removeEventListener('click', handleDeleteClick);
        btn.addEventListener('click', handleDeleteClick);
    });
}

// Handle View button click
async function handleViewClick(e) {
    const btn = e.currentTarget;
    const id = btn.getAttribute('data-id');
    
    try {
        showLoadingModal();
        
        const response = await fetch(`?handler=Details&id=${id}`);
        if (!response.ok) throw new Error('Failed to load details');
        
        const data = await response.json();
        
        const content = `
            <div class="detail-item">
                <strong>Title:</strong>
                <p>${escapeHtml(data.title)}</p>
            </div>
            <div class="detail-item">
                <strong>Message:</strong>
                <p>${escapeHtml(data.message)}</p>
            </div>
            <div class="detail-item">
                <strong>Created By:</strong>
                <p>${escapeHtml(data.createdBy)}</p>
            </div>
            <div class="detail-item">
                <strong>Created At:</strong>
                <p>${escapeHtml(data.createdAt)}</p>
            </div>
        `;
        
        openModal('Announcement Details', content);
    } catch (error) {
        console.error('Error:', error);
        showTemporaryMessage('Error loading announcement details', 'error');
    }
}

// Handle Edit button click
function handleEditClick(e) {
    const btn = e.currentTarget;
    const id = btn.getAttribute('data-id');
    window.location.href = `/BackEnd/EditAnnouncement?id=${id}`;
}

// Handle Delete button click - Show custom modal
function handleDeleteClick(e) {
    const btn = e.currentTarget;
    const id = btn.getAttribute('data-id');
    const row = btn.closest('tr');
    const title = row.querySelector('.title-cell').textContent;
    
    // Show confirmation modal
    showDeleteModal(id, row, title);
}

// Create and add delete confirmation modal to the page
function createDeleteModal() {
    // Check if modal already exists
    if (document.getElementById('deleteConfirmModal')) return;
    
    const modalHtml = `
        <div id="deleteConfirmModal" class="delete-modal" style="display: none;">
            <div class="delete-modal-overlay"></div>
            <div class="delete-modal-container">
                <div class="delete-modal-content">
                    <div class="delete-modal-icon">
                        <i class="bi bi-exclamation-triangle-fill"></i>
                    </div>
                    <h3>Confirm Delete</h3>
                    <p>Are you sure you want to delete this announcement?</p>
                    <p id="deleteTitle" class="delete-title"></p>
                    <p class="delete-warning">This action cannot be undone.</p>
                    <div class="delete-modal-actions">
                        <button class="delete-modal-btn cancel-btn" id="deleteCancelBtn">
                            <i class="bi bi-x-circle"></i>
                            Cancel
                        </button>
                        <button class="delete-modal-btn confirm-btn" id="deleteConfirmBtn">
                            <i class="bi bi-trash"></i>
                            Delete
                        </button>
                    </div>
                </div>
            </div>
        </div>
    `;
    
    document.body.insertAdjacentHTML('beforeend', modalHtml);
    
    // Add event listeners to modal buttons
    const cancelBtn = document.getElementById('deleteCancelBtn');
    const confirmBtn = document.getElementById('deleteConfirmBtn');
    const overlay = document.querySelector('.delete-modal-overlay');
    
    if (cancelBtn) cancelBtn.addEventListener('click', closeDeleteModal);
    if (confirmBtn) confirmBtn.addEventListener('click', confirmDelete);
    if (overlay) overlay.addEventListener('click', closeDeleteModal);
    
    // Close on Escape key
    document.addEventListener('keydown', function(e) {
        if (e.key === 'Escape') {
            const modal = document.getElementById('deleteConfirmModal');
            if (modal && modal.style.display === 'flex') {
                closeDeleteModal();
            }
        }
    });
}

// Show delete confirmation modal
function showDeleteModal(id, row, title) {
    pendingDeleteId = id;
    pendingDeleteRow = row;
    pendingDeleteTitle = title;
    
    const deleteTitleSpan = document.getElementById('deleteTitle');
    if (deleteTitleSpan) {
        deleteTitleSpan.textContent = `"${title}"`;
    }
    
    const modal = document.getElementById('deleteConfirmModal');
    if (modal) {
        modal.style.display = 'flex';
    }
}

// Close delete modal
function closeDeleteModal() {
    const modal = document.getElementById('deleteConfirmModal');
    if (modal) {
        modal.style.display = 'none';
    }
    pendingDeleteId = null;
    pendingDeleteRow = null;
    pendingDeleteTitle = null;
}

async function confirmDelete() {
    if (!pendingDeleteId) return;

    try {
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

        const formData = new FormData();
        formData.append('id', pendingDeleteId);

        const currentPath = window.location.pathname;

        const response = await fetch(`${currentPath}?handler=Delete`, {
            method: 'POST',
            headers: {
                'RequestVerificationToken': token
            },
            body: formData
        });

        const result = await response.json();

        if (result.success) {
            if (pendingDeleteRow) {
                pendingDeleteRow.remove();
            }

            updateStatsAfterDelete();
            showSuccessPopup();
            filterTable();

            const remainingRows = tableBody.querySelectorAll('tr:not(.empty-row)');
            if (remainingRows.length === 0) {
                const emptyRow = document.createElement('tr');
                emptyRow.className = 'empty-row';
                emptyRow.innerHTML = `
                    <td colspan="5" class="empty-message">
                        No announcements found. Click "Create Announcement" to add one.
                    </td>`;
                tableBody.appendChild(emptyRow);
            }

        } else {
            showTemporaryMessage(result.message || 'Failed to delete announcement', 'error');
        }

    } catch (error) {
        console.error('Delete Error:', error);
        showTemporaryMessage('Error deleting announcement', 'error');
    } finally {
        closeDeleteModal();
    }
}

// Initialize Create button
function initializeCreateButton() {
    const createBtn = document.getElementById('createAnnouncementBtn');
    if (createBtn) {
        createBtn.removeEventListener('click', handleCreateClick);
        createBtn.addEventListener('click', handleCreateClick);
    }
}

// Handle Create button click
function handleCreateClick() {
    window.location.href = '/FrontEnd/AddAnnouncement';
}

// Update stats after delete
function updateStatsAfterDelete() {
    const totalSpan = document.getElementById('totalCount');
    if (totalSpan) {
        const currentTotal = parseInt(totalSpan.textContent) || 0;
        totalSpan.textContent = currentTotal - 1;
    }
    
    const latestSpan = document.getElementById('latestCount');
    if (latestSpan) {
        const remainingRows = tableBody.querySelectorAll('tr:not(.empty-row)').length;
        latestSpan.textContent = Math.min(remainingRows, 3);
    }
}

// Show temporary notification message
function showTemporaryMessage(message, type) {
    const notification = document.createElement('div');
    notification.className = `temp-notification ${type}`;
    notification.textContent = message;
    notification.style.cssText = `
        position: fixed;
        bottom: 20px;
        right: 20px;
        padding: 12px 20px;
        border-radius: 8px;
        background: ${type === 'success' ? '#10b981' : '#ef4444'};
        color: white;
        z-index: 9999;
        animation: slideIn 0.3s ease;
        font-size: 14px;
        box-shadow: 0 4px 12px rgba(0,0,0,0.15);
    `;
    
    document.body.appendChild(notification);
    
    setTimeout(() => {
        notification.style.opacity = '0';
        notification.style.transition = 'opacity 0.3s';
        setTimeout(() => notification.remove(), 300);
    }, 3000);
}

// Show loading state in modal
function showLoadingModal() {
    openModal('Loading', '<div style="text-align:center; padding: 40px;"><div class="loading-spinner"></div><p style="margin-top: 16px;">Loading announcement details...</p></div>');
}

// Modal functions
function openModal(title, content) {
    if (!modal || !modalTitle || !modalContent) return;
    modalTitle.textContent = title;
    modalContent.innerHTML = content;
    modal.style.display = 'flex';
}

function showSuccessPopup() {
    const modal = document.getElementById('successPopupModal');
    const closeBtn = document.getElementById('successCloseBtn');

    if (modal) {
        modal.style.display = 'flex';
    }

    if (closeBtn) {
        closeBtn.onclick = function () {
            modal.style.display = 'none';
        };
    }

    setTimeout(() => {
        if (modal) modal.style.display = 'none';
    }, 2500);
}

function closeModal() {
    if (modal) {
        modal.style.display = 'none';
    }
}

// Helper function to escape HTML
function escapeHtml(text) {
    if (!text) return '';
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

// Re-initialize action buttons when table content changes
function refreshActionButtons() {
    initializeActionButtons();
}

// Export for debugging if needed
window.announcementHelpers = {
    filterTable,
    refreshActionButtons,
    closeModal
};