// DOM Elements
let searchInput, filterStatus, filterLecturer, tableBody;
let modal, modalTitle, modalContent, modalClose, modalCloseBtn, modalViewBtn;
let filteredCountSpan;
let pendingDeleteId = null;
let pendingDeleteRow = null;
let pendingDeleteTitle = null;
let currentModuleId = null;

document.addEventListener('DOMContentLoaded', function() {
    initializeElements();
    attachEventListeners();
    initializeActionButtons();
    initializeCreateButton();
    updateFilteredCount();
    createDeleteModal();
});

function initializeElements() {
    searchInput = document.getElementById('searchInput');
    filterStatus = document.getElementById('filterStatus');
    filterLecturer = document.getElementById('filterLecturer');
    tableBody = document.getElementById('moduleTableBody');
    filteredCountSpan = document.getElementById('filteredCount');
    
    modal = document.getElementById('moduleModal');
    modalTitle = document.getElementById('modalTitle');
    modalContent = document.getElementById('modalContent');
    modalClose = document.querySelector('.modal-close');
    modalCloseBtn = document.getElementById('modalCloseBtn');
    modalViewBtn = document.getElementById('modalViewBtn');
}

function attachEventListeners() {
    searchInput?.addEventListener('input', filterTable);
    filterStatus?.addEventListener('change', filterTable);
    filterLecturer?.addEventListener('change', filterTable);
    modalClose?.addEventListener('click', closeModal);
    modalCloseBtn?.addEventListener('click', closeModal);
    window.addEventListener('click', function(e) {
        if (e.target === modal) closeModal();
    });
    modalViewBtn?.addEventListener('click', function() {
        if (currentModuleId) {
            window.location.href = `/FrontEnd/TeachingMaterial/${currentModuleId}`;
        }
    });
}

function filterTable() {
    const searchTerm = searchInput.value.toLowerCase();
    const statusFilter = filterStatus.value;
    const lecturerFilter = filterLecturer.value;
    
    const rows = tableBody.querySelectorAll('tr:not(.empty-row)');
    let visibleCount = 0;
    rows.forEach(row => {
        const code = row.querySelector('.code-cell')?.textContent.toLowerCase() || '';
        const name = row.querySelector('.title-cell')?.textContent.toLowerCase() || '';
        const lecturer = row.getAttribute('data-lecturer') || '';
        const status = row.getAttribute('data-status') || '';
        const matches = code.includes(searchTerm) || name.includes(searchTerm);
        const matchesStatus = statusFilter === 'all' || status.toLowerCase() === statusFilter;
        const matchesLecturer = lecturerFilter === 'all' || lecturer === lecturerFilter;
        if (matches && matchesStatus && matchesLecturer) {
            row.style.display = '';
            visibleCount++;
        } else {
            row.style.display = 'none';
        }
    });
    filteredCountSpan.textContent = visibleCount;
    // Show/hide empty message
    const empty = tableBody.querySelector('.empty-row');
    if (visibleCount === 0 && !empty && rows.length > 0) {
        const emptyRow = document.createElement('tr');
        emptyRow.className = 'empty-row';
        emptyRow.innerHTML = '<td colspan="6" class="empty-message">No matching modules found.</td>';
        tableBody.appendChild(emptyRow);
    } else if (visibleCount > 0 && empty && empty.classList.contains('empty-row')) {
        empty.remove();
    }
}

function updateFilteredCount() {
    const visible = Array.from(tableBody.querySelectorAll('tr:not(.empty-row)'))
        .filter(row => row.style.display !== 'none');
    filteredCountSpan.textContent = visible.length;
}

function initializeActionButtons() {
    document.querySelectorAll('.view-btn').forEach(btn => {
        btn.removeEventListener('click', handleViewClick);
        btn.addEventListener('click', handleViewClick);
    });
    document.querySelectorAll('.edit-btn').forEach(btn => {
        btn.removeEventListener('click', handleEditClick);
        btn.addEventListener('click', handleEditClick);
    });
    document.querySelectorAll('.delete-btn').forEach(btn => {
        btn.removeEventListener('click', handleDeleteClick);
        btn.addEventListener('click', handleDeleteClick);
    });
}

async function handleViewClick(e) {
    const btn = e.currentTarget;
    const id = btn.getAttribute('data-id');
    currentModuleId = id; // Store for the modal view button
    try {
        modalContent.innerHTML = '<div style="text-align:center; padding:40px;"><div class="loading-spinner"></div><p>Loading...</p></div>';
        modal.style.display = 'flex';
        const response = await fetch(`?handler=Details&id=${id}`);
        if (!response.ok) throw new Error('Failed');
        const data = await response.json();
        modalTitle.textContent = 'Module Details';
        const statusBadge = data.status === 'Active' 
            ? '<span class="badge badge-status-published">Active</span>' 
            : '<span class="badge badge-status-archived">Removed</span>';
        modalContent.innerHTML = `
            <div class="detail-header">
                <h2 class="detail-title">${escapeHtml(data.name)}</h2>
                <div class="detail-meta">
                    <span><i class="bi bi-code"></i> Code: ${escapeHtml(data.code)}</span>
                    <span><i class="bi bi-person-circle"></i> Lecturer: ${escapeHtml(data.lecturer)}</span>
                </div>
                <div style="margin-top:8px;">Status: ${statusBadge}</div>
                <div style="margin-top:8px;">
                    <span><i class="bi bi-calendar-check"></i> Start: ${escapeHtml(data.startDate)}</span>
                    <span style="margin-left:16px;"><i class="bi bi-calendar-x"></i> End: ${escapeHtml(data.endDate)}</span>
                </div>
            </div>
            <div class="detail-divider"></div>
            <div class="detail-content">
                <p><strong>Description:</strong></p>
                <p>${escapeHtml(data.description)}</p>
                <p style="margin-top:12px;"><strong>Invitation Code:</strong> ${escapeHtml(data.invitationCode)}</p>
            </div>
        `;
    } catch (error) {
        modalContent.innerHTML = '<div style="text-align:center;padding:40px;color:#ef4444;"><i class="bi bi-exclamation-triangle-fill" style="font-size:48px;"></i><p>Error loading details</p></div>';
    }
}

function handleEditClick(e) {
    const id = e.currentTarget.getAttribute('data-id');
    window.location.href = `/FrontEnd/EditModule/${id}`;
}

function handleDeleteClick(e) {
    const btn = e.currentTarget;
    const id = btn.getAttribute('data-id');
    const row = btn.closest('tr');
    const title = row.querySelector('.title-cell').textContent;
    showDeleteModal(id, row, title);
}

function createDeleteModal() {
    if (document.getElementById('deleteConfirmModal')) return;
    const modalHtml = `
        <div id="deleteConfirmModal" class="modal-overlay-full" style="display: none;">
            <div class="modal-container">
                <div class="modal-icon warning-icon"><i class="bi bi-exclamation-triangle-fill"></i></div>
                <h3 class="modal-title">Confirm Delete</h3>
                <p class="modal-message">Are you sure you want to delete this module?</p>
                <p id="deleteTitle" style="font-weight:600;color:#ef4444;background:#fef2f2;padding:10px;border-radius:10px;margin:16px 0;"></p>
                <div class="modal-warning"><i class="bi bi-shield-exclamation"></i><span>This action cannot be undone.</span></div>
                <div class="modal-actions">
                    <button class="modal-btn modal-cancel" id="deleteCancelBtn"><i class="bi bi-x-circle"></i> Cancel</button>
                    <button class="modal-btn modal-danger" id="deleteConfirmBtn"><i class="bi bi-trash"></i> Delete</button>
                </div>
            </div>
        </div>
    `;
    document.body.insertAdjacentHTML('beforeend', modalHtml);
    document.getElementById('deleteCancelBtn').addEventListener('click', closeDeleteModal);
    document.getElementById('deleteConfirmBtn').addEventListener('click', confirmDelete);
    document.addEventListener('keydown', e => {
        if (e.key === 'Escape') {
            const m = document.getElementById('deleteConfirmModal');
            if (m && m.style.display === 'flex') closeDeleteModal();
        }
    });
}

function showDeleteModal(id, row, title) {
    pendingDeleteId = id;
    pendingDeleteRow = row;
    pendingDeleteTitle = title;
    document.getElementById('deleteTitle').textContent = `"${title}"`;
    document.getElementById('deleteConfirmModal').style.display = 'flex';
}

function closeDeleteModal() {
    document.getElementById('deleteConfirmModal').style.display = 'none';
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
        const response = await fetch(`${window.location.pathname}?handler=Delete`, {
            method: 'POST',
            headers: { 'RequestVerificationToken': token },
            body: formData
        });
        const result = await response.json();
        if (result.success) {
            if (pendingDeleteRow) pendingDeleteRow.remove();
            updateStatsAfterDelete();
            showSuccessPopup();
            filterTable();
            // Check if empty
            const remaining = tableBody.querySelectorAll('tr:not(.empty-row)');
            if (remaining.length === 0) {
                const emptyRow = document.createElement('tr');
                emptyRow.className = 'empty-row';
                emptyRow.innerHTML = `<td colspan="6" class="empty-message">No modules found. Click "Create Module" to add one.</td>`;
                tableBody.appendChild(emptyRow);
            }
        } else {
            showTemporaryMessage(result.message || 'Delete failed', 'error');
        }
    } catch (error) {
        showTemporaryMessage('Error deleting module', 'error');
    } finally {
        closeDeleteModal();
    }
}

function initializeCreateButton() {
    document.getElementById('createModuleBtn')?.addEventListener('click', () => {
        window.location.href = '/FrontEnd/AddModule';
    });
}

function updateStatsAfterDelete() {
    const totalSpan = document.getElementById('totalCount');
    if (totalSpan) totalSpan.textContent = parseInt(totalSpan.textContent) - 1;
    const activeSpan = document.getElementById('activeCount');
    if (activeSpan) activeSpan.textContent = parseInt(activeSpan.textContent) - 1;
}

function showSuccessPopup() {
    let modal = document.getElementById('successDeleteModal');
    if (!modal) {
        modal = document.createElement('div');
        modal.id = 'successDeleteModal';
        modal.className = 'modal-overlay-full';
        modal.style.display = 'none';
        modal.innerHTML = `
            <div class="modal-container">
                <div class="modal-icon success-icon"><i class="bi bi-check-circle-fill"></i></div>
                <h3 class="modal-title">Deleted Successfully</h3>
                <p class="modal-message">The module has been removed.</p>
                <div class="modal-actions"><button class="modal-btn modal-success" id="successDeleteCloseBtn"><i class="bi bi-check-circle"></i> OK</button></div>
            </div>
        `;
        document.body.appendChild(modal);
    }
    modal.style.display = 'flex';
    document.getElementById('successDeleteCloseBtn').onclick = function() {
        modal.style.display = 'none';
        window.location.href = '/FrontEnd/ModuleList'; 
    };
    
    modal.onclick = function(e) {
        if (e.target === modal) {
            modal.style.display = 'none';
            window.location.href = '/FrontEnd/ModuleList'; 
        }
    };
    
    setTimeout(function() {
        modal.style.display = 'none';
        window.location.href = '/FrontEnd/ModuleList';
    }, 2500);
}

function showTemporaryMessage(msg, type) {
    const div = document.createElement('div');
    div.textContent = msg;
    div.style.cssText = `position:fixed;bottom:20px;right:20px;padding:12px 20px;border-radius:8px;background:${type === 'success' ? '#10b981' : '#ef4444'};color:white;z-index:9999;font-size:14px;box-shadow:0 4px 12px rgba(0,0,0,0.15);`;
    document.body.appendChild(div);
    setTimeout(() => { div.style.opacity = '0'; setTimeout(() => div.remove(), 300); }, 3000);
}

function closeModal() {
    modal.style.display = 'none';
    modalTitle.textContent = 'Module Details';
    modalContent.innerHTML = '';
    currentModuleId = null; // Reset when modal closes
}

function escapeHtml(text) {
    if (!text) return '';
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}