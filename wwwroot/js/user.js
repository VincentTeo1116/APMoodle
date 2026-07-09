function handleImageError(img) {
    // Prevent infinite loop
    img.onerror = null;

    // Get data attributes with fallbacks
    const color = img.dataset.color || '#64748b';
    const initials = img.dataset.initials || '?';
    const parent = img.parentElement;

    // Safety: if parent is missing, abort
    if (!parent || !parent.nodeType) return;

    // Create the placeholder div
    const placeholder = document.createElement('div');
    placeholder.className = 'avatar-placeholder';
    placeholder.style.backgroundColor = color;
    placeholder.textContent = initials;

    // Replace the broken image
    parent.replaceChild(placeholder, img);

    // Optional: log for debugging
    console.log('Image fallback used for:', img.alt, '→', initials);
}

window.handleImageError = handleImageError;

document.addEventListener('DOMContentLoaded', function () {
    // DOM Elements
    const searchInput = document.getElementById('searchInput');
    const filterUserType = document.getElementById('filterUserType');
    const filterStatus = document.getElementById('filterStatus');
    const tableBody = document.getElementById('userTableBody');
    const totalCountEl = document.getElementById('totalCount');
    const filteredCountEl = document.getElementById('filteredCount');
    const studentCountEl = document.getElementById('studentCount');
    const lecturerCountEl = document.getElementById('lecturerCount');
    const adminCountEl = document.getElementById('adminCount');

    // Modal Elements
    const modal = document.getElementById('userModal');
    const modalClose = document.querySelector('.modal-close');
    const modalCloseBtn = document.getElementById('modalCloseBtn');
    const modalTitle = document.getElementById('modalTitle');
    const modalContent = document.getElementById('modalContent');

    // Success Popup Elements
    const successPopup = document.getElementById('successPopupModal');
    const successCloseBtn = document.getElementById('successCloseBtn');

    // Store initial rows for filtering
    let allRows = [];

    // Initialize - store all rows
    function initializeRows() {
        const rows = tableBody.querySelectorAll('tr:not(.empty-row)');
        allRows = Array.from(rows);
    }

    // Filter function
    function filterTable() {
        const searchTerm = searchInput.value.toLowerCase().trim();
        const userType = filterUserType.value;
        const status = filterStatus.value;

        let visibleRows = allRows.filter(row => {
            // Get data
            const name = row.querySelector('.user-name')?.textContent?.toLowerCase() || '';
            const email = row.querySelector('.email-cell')?.textContent?.toLowerCase() || '';
            const userCode = row.querySelector('.user-code')?.textContent?.toLowerCase() || '';
            const rowType = row.dataset.userType || '';
            const rowStatus = row.dataset.status || '';

            // Search filter
            let matchesSearch = true;
            if (searchTerm) {
                matchesSearch = name.includes(searchTerm) || 
                               email.includes(searchTerm) || 
                               userCode.includes(searchTerm);
            }

            // Type filter
            let matchesType = true;
            if (userType !== 'all') {
                matchesType = rowType.toLowerCase() === userType.toLowerCase();
            }

            // Status filter
            let matchesStatus = true;
            if (status !== 'all') {
                matchesStatus = rowStatus.toLowerCase() === status.toLowerCase();
            }

            return matchesSearch && matchesType && matchesStatus;
        });

        // Update table
        if (visibleRows.length === 0) {
            tableBody.innerHTML = `
                <tr class="empty-row">
                    <td colspan="6" class="empty-message">
                        No users found matching your filters.
                    </td>
                </tr>
            `;
        } else {
            tableBody.innerHTML = '';
            visibleRows.forEach(row => {
                tableBody.appendChild(row.cloneNode(true));
            });
            attachEventListeners();
        }

        // Update filtered count
        filteredCountEl.textContent = visibleRows.length;
    }

    // Attach event listeners to action buttons
    function attachEventListeners() {
        // View buttons
        document.querySelectorAll('.view-btn').forEach(btn => {
            btn.addEventListener('click', function(e) {
                e.stopPropagation();
                const id = this.dataset.id;
                const type = this.dataset.type;
                openViewModal(id, type);
            });
        });

        // Edit buttons
        document.querySelectorAll('.edit-btn').forEach(btn => {
            btn.addEventListener('click', function(e) {
                e.stopPropagation();
                const id = this.dataset.id;
                const type = this.dataset.type;
                window.location.href = `/FrontEnd/UserEdit?id=${id}&type=${type}`;
            });
        });

        // Toggle status buttons (single button for all statuses)
        document.querySelectorAll('.toggle-status-btn').forEach(btn => {
            btn.addEventListener('click', function(e) {
                e.stopPropagation();
                const id = this.dataset.id;
                const type = this.dataset.type;
                const status = this.dataset.status;
                const name = this.closest('tr').querySelector('.user-name').textContent;
                
                if (status === 'Pending') {
                    // Show dropdown for Pending users
                    showPendingActions(id, type, name, btn);
                } else {
                    // Direct action for Active/Inactive
                    confirmToggleStatus(id, type, name, status);
                }
            });
        });
    }

    // Show dropdown for Pending users (Activate or Reject)
    function showPendingActions(id, type, name, buttonElement) {
        // Create dropdown modal
        const dropdownModal = document.createElement('div');
        dropdownModal.className = 'modal';
        dropdownModal.style.display = 'flex';
        dropdownModal.innerHTML = `
            <div class="modal-content" style="max-width: 350px;">
                <div class="modal-header">
                    <div class="modal-header-title">
                        <i class="bi bi-person-check" style="color: #f59e0b;"></i>
                        <h3>Pending User</h3>
                    </div>
                    <span class="modal-close">&times;</span>
                </div>
                <div class="modal-body" style="padding: 20px;">
                    <p style="text-align: center; font-size: 15px; color: #1e293b; margin-bottom: 20px;">
                        User <strong>"${name}"</strong> is pending approval.
                    </p>
                    <div style="display: flex; gap: 12px; justify-content: center;">
                        <button class="btn-secondary" id="pendingActivateBtn" style="background: #0891b2; color: white; border-color: #0891b2; padding: 10px 24px;">
                            <i class="bi bi-check-circle"></i> Activate
                        </button>
                        <button class="btn-secondary" id="pendingRejectBtn" style="background: #ef4444; color: white; border-color: #ef4444; padding: 10px 24px;">
                            <i class="bi bi-x-circle"></i> Reject
                        </button>
                    </div>
                </div>
            </div>
        `;
        document.body.appendChild(dropdownModal);

        const closeDropdown = () => {
            dropdownModal.remove();
        };

        dropdownModal.querySelector('.modal-close').addEventListener('click', closeDropdown);
        dropdownModal.addEventListener('click', function(e) {
            if (e.target === dropdownModal) {
                closeDropdown();
            }
        });

        // Activate button
        document.getElementById('pendingActivateBtn').addEventListener('click', function() {
            closeDropdown();
            confirmToggleStatus(id, type, name, 'Pending');
        });

        // Reject button
        document.getElementById('pendingRejectBtn').addEventListener('click', function() {
            closeDropdown();
            confirmReject(id, type, name);
        });
    }

    // Single toggle function for all statuses
    function confirmToggleStatus(id, type, name, currentStatus) {
        let title, message, confirmText, confirmColor, icon, action, endpoint, handler, newStatus, newStatusClass, newButtonIcon, newButtonTitle, newButtonClass, successMessage;
        
        if (currentStatus === 'Active') {
            // Deactivate
            title = 'Confirm Deactivate';
            message = `Are you sure you want to deactivate user <strong>"${name}"</strong>? The user will no longer be able to access the system.`;
            confirmText = 'Deactivate';
            confirmColor = '#ef4444';
            icon = 'bi-exclamation-triangle';
            action = 'Deactivated';
            endpoint = 'UserDelete';
            handler = 'Delete';
            newStatus = 'Inactive';
            newStatusClass = 'status-inactive';
            newButtonIcon = 'bi-person-check';
            newButtonTitle = 'Reactivate';
            newButtonClass = 'reactivate-btn';
            successMessage = `User "${name}" has been deactivated successfully.`;
        } else if (currentStatus === 'Pending') {
            // Activate
            title = 'Confirm Activate';
            message = `Are you sure you want to activate user <strong>"${name}"</strong>? The user will gain full access to the system.`;
            confirmText = 'Activate';
            confirmColor = '#0891b2';
            icon = 'bi-check-circle';
            action = 'Activated';
            endpoint = 'UserActivate';
            handler = 'Activate';
            newStatus = 'Active';
            newStatusClass = 'status-active';
            newButtonIcon = 'bi-person-x';
            newButtonTitle = 'Deactivate';
            newButtonClass = 'delete-btn';
            successMessage = `User "${name}" has been activated successfully.`;
        } else {
            // Reactivate (Inactive or Rejected)
            title = 'Confirm Reactivate';
            message = `Are you sure you want to reactivate user <strong>"${name}"</strong>? The user will regain access to the system.`;
            confirmText = 'Reactivate';
            confirmColor = '#22c55e';
            icon = 'bi-arrow-repeat';
            action = 'Reactivated';
            endpoint = 'UserReactivate';
            handler = 'Reactivate';
            newStatus = 'Active';
            newStatusClass = 'status-active';
            newButtonIcon = 'bi-person-x';
            newButtonTitle = 'Deactivate';
            newButtonClass = 'delete-btn';
            successMessage = `User "${name}" has been reactivated successfully.`;
        }

        const confirmModal = document.createElement('div');
        confirmModal.className = 'modal';
        confirmModal.style.display = 'flex';
        confirmModal.innerHTML = `
            <div class="modal-content" style="max-width: 400px;">
                <div class="modal-header">
                    <div class="modal-header-title">
                        <i class="bi ${icon}" style="color: ${confirmColor};"></i>
                        <h3>${title}</h3>
                    </div>
                    <span class="modal-close">&times;</span>
                </div>
                <div class="modal-body">
                    <p style="text-align: center; font-size: 16px; color: #1e293b;">
                        ${message}
                    </p>
                    <p style="text-align: center; color: #64748b; font-size: 14px; margin-top: 8px;">
                        This action can be reversed later.
                    </p>
                </div>
                <div class="modal-footer" style="justify-content: center; gap: 12px;">
                    <button class="btn-secondary" id="cancelToggleBtn">Cancel</button>
                    <button class="btn-secondary" id="confirmToggleBtn" style="background: ${confirmColor}; color: white; border-color: ${confirmColor};">${confirmText}</button>
                </div>
            </div>
        `;
        document.body.appendChild(confirmModal);

        const closeConfirmModal = () => {
            confirmModal.remove();
        };

        confirmModal.querySelector('.modal-close').addEventListener('click', closeConfirmModal);
        document.getElementById('cancelToggleBtn').addEventListener('click', closeConfirmModal);
        confirmModal.addEventListener('click', function(e) {
            if (e.target === confirmModal) {
                closeConfirmModal();
            }
        });

        document.getElementById('confirmToggleBtn').addEventListener('click', function() {
            closeConfirmModal();
            
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            
            const formData = new FormData();
            formData.append('id', id);
            formData.append('type', type);
            
            fetch(`/FrontEnd/${endpoint}?handler=${handler}`, {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': token
                },
                body: formData
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    showSuccessPopup(`User ${action}`, successMessage);
                    const row = document.querySelector(`tr[data-id="${id}"]`);
                    if (row) {
                        // Update status in the row
                        const statusCell = row.querySelector('.status-cell');
                        const statusBadge = statusCell.querySelector('.status-badge');
                        statusBadge.className = `status-badge ${newStatusClass}`;
                        statusBadge.innerHTML = `<span class="status-dot"></span> ${newStatus}`;
                        row.dataset.status = newStatus;
                        
                        // Update the toggle button
                        const actionCell = row.querySelector('.action-cell');
                        const buttonsWrapper = actionCell.querySelector('.action-buttons-wrapper');
                        const toggleBtn = buttonsWrapper.querySelector('.toggle-status-btn');
                        if (toggleBtn) {
                            // Update button attributes and icon
                            toggleBtn.setAttribute('data-status', newStatus);
                            toggleBtn.title = newButtonTitle;
                            toggleBtn.className = `action-btn ${newButtonClass}`;
                            toggleBtn.innerHTML = `<i class="bi ${newButtonIcon}"></i>`;
                            
                            // Remove old event listener and add new one
                            const newBtn = toggleBtn.cloneNode(true);
                            toggleBtn.parentNode.replaceChild(newBtn, toggleBtn);
                            newBtn.addEventListener('click', function(e) {
                                e.stopPropagation();
                                const id = this.dataset.id;
                                const type = this.dataset.type;
                                const status = this.dataset.status;
                                const name = this.closest('tr').querySelector('.user-name').textContent;
                                
                                if (status === 'Pending') {
                                    showPendingActions(id, type, name, this);
                                } else {
                                    confirmToggleStatus(id, type, name, status);
                                }
                            });
                        }
                        
                        // Update counts
                        updateStats();
                        updateFilteredCount();
                    }
                } else {
                    alert(data.message || `Failed to ${action.toLowerCase()} user.`);
                }
            })
            .catch(error => {
                console.error('Error:', error);
                alert(`An error occurred while ${action.toLowerCase()} the user: ` + error.message);
            });
        });
    }

    // Confirm Reject (for Pending users)
    function confirmReject(id, type, name) {
        const confirmModal = document.createElement('div');
        confirmModal.className = 'modal';
        confirmModal.style.display = 'flex';
        confirmModal.innerHTML = `
            <div class="modal-content" style="max-width: 400px;">
                <div class="modal-header">
                    <div class="modal-header-title">
                        <i class="bi bi-x-circle" style="color: #ef4444;"></i>
                        <h3>Confirm Reject</h3>
                    </div>
                    <span class="modal-close">&times;</span>
                </div>
                <div class="modal-body">
                    <p style="text-align: center; font-size: 16px; color: #1e293b;">
                        Are you sure you want to reject user <strong>"${name}"</strong>?
                    </p>
                    <p style="text-align: center; color: #64748b; font-size: 14px; margin-top: 8px;">
                        The user will be set to Inactive status and will not be able to access the system.
                    </p>
                    <p style="text-align: center; color: #ef4444; font-size: 13px; margin-top: 8px;">
                        <i class="bi bi-info-circle"></i> They can be reactivated later if needed.
                    </p>
                </div>
                <div class="modal-footer" style="justify-content: center; gap: 12px;">
                    <button class="btn-secondary" id="cancelRejectBtn">Cancel</button>
                    <button class="btn-secondary" id="confirmRejectBtn" style="background: #ef4444; color: white; border-color: #ef4444;">Reject</button>
                </div>
            </div>
        `;
        document.body.appendChild(confirmModal);

        const closeConfirmModal = () => {
            confirmModal.remove();
        };

        confirmModal.querySelector('.modal-close').addEventListener('click', closeConfirmModal);
        document.getElementById('cancelRejectBtn').addEventListener('click', closeConfirmModal);
        confirmModal.addEventListener('click', function(e) {
            if (e.target === confirmModal) {
                closeConfirmModal();
            }
        });

        document.getElementById('confirmRejectBtn').addEventListener('click', function() {
            closeConfirmModal();
            
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            
            const formData = new FormData();
            formData.append('id', id);
            formData.append('type', type);
            
            fetch(`/FrontEnd/UserReject?handler=Reject`, {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': token
                },
                body: formData
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    showSuccessPopup('User Rejected', `User "${data.userName || name}" has been rejected and set to Inactive status.`);
                    const row = document.querySelector(`tr[data-id="${id}"]`);
                    if (row) {
                        // Update status in the row to Inactive
                        const statusCell = row.querySelector('.status-cell');
                        const statusBadge = statusCell.querySelector('.status-badge');
                        statusBadge.className = 'status-badge status-inactive';
                        statusBadge.innerHTML = '<span class="status-dot"></span> Inactive';
                        row.dataset.status = 'Inactive';
                        
                        // Replace buttons - remove activate and reject, add reactivate
                        const actionCell = row.querySelector('.action-cell');
                        const buttonsWrapper = actionCell.querySelector('.action-buttons-wrapper');
                        
                        // Keep view and edit buttons
                        const viewBtn = buttonsWrapper.querySelector('.view-btn');
                        const editBtn = buttonsWrapper.querySelector('.edit-btn');
                        buttonsWrapper.innerHTML = '';
                        buttonsWrapper.appendChild(viewBtn);
                        buttonsWrapper.appendChild(editBtn);
                        
                        // Add reactivate button (toggle-status-btn for Inactive users)
                        const reactivateBtn = document.createElement('button');
                        reactivateBtn.className = 'action-btn toggle-status-btn';
                        reactivateBtn.setAttribute('data-id', id);
                        reactivateBtn.setAttribute('data-type', type);
                        reactivateBtn.setAttribute('data-status', 'Inactive');
                        reactivateBtn.title = 'Reactivate';
                        reactivateBtn.innerHTML = '<i class="bi bi-person-check"></i>';
                        reactivateBtn.addEventListener('click', function(e) {
                            e.stopPropagation();
                            const id = this.dataset.id;
                            const type = this.dataset.type;
                            const status = this.dataset.status;
                            const name = this.closest('tr').querySelector('.user-name').textContent;
                            confirmToggleStatus(id, type, name, status);
                        });
                        buttonsWrapper.appendChild(reactivateBtn);
                        
                        // Update counts
                        updateStats();
                        updateFilteredCount();
                    }
                } else {
                    alert(data.message || 'Failed to reject user.');
                }
            })
            .catch(error => {
                console.error('Error:', error);
                alert('An error occurred while rejecting the user: ' + error.message);
            });
        });
    }

    // Open View Modal
    function openViewModal(id, type) {
        // Show loading state
        modalContent.innerHTML = `
            <div style="text-align: center; padding: 20px;">
                <div class="loading-spinner"></div>
                <p style="color: #64748b; margin-top: 10px;">Loading user details...</p>
            </div>
        `;
        modal.style.display = 'flex';

        // Update modal title based on user type
        const typeNames = {
            'student': 'Student',
            'lecturer': 'Lecturer',
            'admin': 'Admin'
        };
        modalTitle.textContent = `${typeNames[type.toLowerCase()] || 'User'} Details`;

        fetch(`/FrontEnd/UserDetails?handler=Details&id=${id}&type=${type}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }
                return response.json();
            })
            .then(data => {
                if (data.success) {
                    renderUserDetails(data.user);
                } else {
                    modalContent.innerHTML = `
                        <div style="text-align: center; padding: 20px; color: #ef4444;">
                            <i class="bi bi-exclamation-circle" style="font-size: 48px; display: block; margin-bottom: 16px;"></i>
                            <p>${data.message || 'Failed to load user details.'}</p>
                        </div>
                    `;
                }
            })
            .catch(error => {
                console.error('Error:', error);
                modalContent.innerHTML = `
                    <div style="text-align: center; padding: 20px; color: #ef4444;">
                        <i class="bi bi-exclamation-circle" style="font-size: 48px; display: block; margin-bottom: 16px;"></i>
                        <p>An error occurred while loading user details.</p>
                        <p style="font-size: 12px; color: #94a3b8; margin-top: 8px;">${error.message}</p>
                    </div>
                `;
            });
    }

    // Render user details in modal
    function renderUserDetails(user) {
        const roleColor = user.role === 'Student' ? '#3b82f6' : 
                          user.role === 'Lecturer' ? '#f59e0b' : '#ef4444';
        
        const avatarHtml = user.profilePic ? 
            `<img src="${user.profilePic}" alt="${user.name}" class="detail-avatar">` :
            `<div class="detail-avatar-placeholder" style="background-color: ${roleColor};">${user.initials || 'U'}</div>`;

        // Determine registered date display
        let registeredDisplay = '';
        if (user.registeredDate) {
            registeredDisplay = new Date(user.registeredDate).toLocaleDateString('en-US', { 
                year: 'numeric', 
                month: 'long', 
                day: 'numeric' 
            });
        } else if (user.role === 'Admin') {
            registeredDisplay = 'System User (Pre-configured)';
        } else {
            registeredDisplay = 'Not available';
        }

        modalContent.innerHTML = `
            <div class="detail-header">
                ${avatarHtml}
                <h4 class="detail-title">${user.name}</h4>
                <span class="role-badge role-${user.role.toLowerCase()}">
                    <i class="bi ${user.role === 'Student' ? 'bi-person' : user.role === 'Lecturer' ? 'bi-person-workspace' : 'bi-shield'}"></i>
                    ${user.role}
                </span>
            </div>
            <div class="detail-divider"></div>
            <div class="detail-row">
                <span class="detail-label">User Code</span>
                <span class="detail-value">${user.userCode}</span>
            </div>
            <div class="detail-row">
                <span class="detail-label">Email</span>
                <span class="detail-value">${user.email}</span>
            </div>
            <div class="detail-row">
                <span class="detail-label">Status</span>
                <span class="detail-value">
                    <span class="status-badge status-${user.status.toLowerCase()}">
                        <span class="status-dot"></span>
                        ${user.status}
                    </span>
                </span>
            </div>
            <div class="detail-row">
                <span class="detail-label">Registered Date</span>
                <span class="detail-value">${registeredDisplay}</span>
            </div>
            ${user.phoneNumber ? `
                <div class="detail-row">
                    <span class="detail-label">Phone Number</span>
                    <span class="detail-value">${user.phoneNumber}</span>
                </div>
            ` : ''}
            ${user.department ? `
                <div class="detail-row">
                    <span class="detail-label">Department</span>
                    <span class="detail-value">${user.department}</span>
                </div>
            ` : ''}
            ${user.gender ? `
                <div class="detail-row">
                    <span class="detail-label">Gender</span>
                    <span class="detail-value">${user.gender}</span>
                </div>
            ` : ''}
        `;
    }

    // Show success popup with custom message
    function showSuccessPopup(title, message) {
        const popup = document.getElementById('successPopupModal');
        const popupTitle = popup.querySelector('h3');
        const popupMessage = popup.querySelector('p');
        popupTitle.textContent = title;
        popupMessage.textContent = message;
        popup.style.display = 'flex';
        
        // Auto close after 3 seconds
        setTimeout(() => {
            popup.style.display = 'none';
        }, 3000);
    }

    // Update stats after status change
    function updateStats() {
        const rows = document.querySelectorAll('#userTableBody tr:not(.empty-row)');
        let students = 0, lecturers = 0, admins = 0;
        rows.forEach(row => {
            const type = row.dataset.userType;
            if (type === 'Student') students++;
            else if (type === 'Lecturer') lecturers++;
            else if (type === 'Admin') admins++;
        });
        
        document.getElementById('studentCount').textContent = students;
        document.getElementById('lecturerCount').textContent = lecturers;
        document.getElementById('adminCount').textContent = admins;
        document.getElementById('totalCount').textContent = rows.length;
    }

    // Update filtered count
    function updateFilteredCount() {
        const visibleRows = document.querySelectorAll('#userTableBody tr:not(.empty-row)');
        document.getElementById('filteredCount').textContent = visibleRows.length;
    }

    // Modal close handlers
    function closeModal() {
        modal.style.display = 'none';
    }

    modalClose.addEventListener('click', closeModal);
    modalCloseBtn.addEventListener('click', closeModal);

    // Close modal on outside click
    window.addEventListener('click', function(e) {
        if (e.target === modal) {
            closeModal();
        }
        if (e.target === successPopup) {
            successPopup.style.display = 'none';
        }
    });

    // Close success popup
    successCloseBtn.addEventListener('click', function() {
        successPopup.style.display = 'none';
        location.reload();
    });

    // Event listeners for filters
    searchInput.addEventListener('input', filterTable);
    filterUserType.addEventListener('change', filterTable);
    filterStatus.addEventListener('change', filterTable);

    // Initialize
    initializeRows();
    attachEventListeners();

    // Create User Button
    document.getElementById('createUserBtn').addEventListener('click', function() {
        window.location.href = '/FrontEnd/UserCreate';
    });
});