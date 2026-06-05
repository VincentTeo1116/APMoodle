// editAnnouncement.js

// Show error modal - uses CSS from site.css
function showErrorModal(message) {
    let modal = document.getElementById('errorModal');
    if (!modal) {
        modal = document.createElement('div');
        modal.id = 'errorModal';
        modal.className = 'modal-overlay-full';
        modal.style.display = 'none';
        modal.innerHTML = `
            <div class="modal-container">
                <div class="modal-icon error-icon">
                    <i class="bi bi-exclamation-triangle-fill"></i>
                </div>
                <h3 class="modal-title">Validation Error</h3>
                <p class="modal-message" id="errorModalMessage"></p>
                <div class="modal-actions">
                    <button class="modal-btn modal-confirm" id="errorModalOkBtn">
                        <i class="bi bi-check-circle"></i> OK
                    </button>
                </div>
            </div>
        `;
        document.body.appendChild(modal);
    }
    
    const messageSpan = document.getElementById('errorModalMessage');
    if (messageSpan) {
        messageSpan.textContent = message;
    }
    
    modal.style.display = 'flex';
    
    const okBtn = document.getElementById('errorModalOkBtn');
    const newOkBtn = okBtn.cloneNode(true);
    okBtn.parentNode.replaceChild(newOkBtn, okBtn);
    
    newOkBtn.onclick = function() {
        modal.style.display = 'none';
    };
    
    modal.onclick = function(e) {
        if (e.target === modal) {
            modal.style.display = 'none';
        }
    };
}

// Show success modal
function showSuccessModal(message, redirectUrl = null) {
    let modal = document.getElementById('successModal');
    if (!modal) {
        modal = document.createElement('div');
        modal.id = 'successModal';
        modal.className = 'modal-overlay-full';
        modal.style.display = 'none';
        modal.innerHTML = `
            <div class="modal-container">
                <div class="modal-icon success-icon">
                    <i class="bi bi-check-circle-fill"></i>
                </div>
                <h3 class="modal-title">Success!</h3>
                <p class="modal-message" id="successModalMessage"></p>
                <div class="modal-actions">
                    <button class="modal-btn modal-success" id="successModalOkBtn">
                        <i class="bi bi-check-circle"></i> OK
                    </button>
                </div>
            </div>
        `;
        document.body.appendChild(modal);
    }
    
    document.getElementById('successModalMessage').textContent = message;
    modal.style.display = 'flex';
    
    const okBtn = document.getElementById('successModalOkBtn');
    const newOkBtn = okBtn.cloneNode(true);
    okBtn.parentNode.replaceChild(newOkBtn, okBtn);
    
    newOkBtn.onclick = () => {
        modal.style.display = 'none';
        if (redirectUrl) {
            window.location.href = redirectUrl;
        }
    };
    
    modal.onclick = (e) => {
        if (e.target === modal) {
            modal.style.display = 'none';
            if (redirectUrl) {
                window.location.href = redirectUrl;
            }
        }
    };
    
    if (!redirectUrl) {
        setTimeout(() => {
            if (modal.style.display === 'flex') {
                modal.style.display = 'none';
            }
        }, 2000);
    }
}

document.addEventListener('DOMContentLoaded', function() {
    console.log('DOM loaded - initializing edit page');
    
    initializeCharacterCounters();
    initializeLivePreview();
    initializeDeleteModal();
    initializeSuccessPopup();
    initializeErrorPopup();
    
    // Setup cancel button - ALWAYS show modal
    const cancelBtn = document.getElementById('cancelBtn');
    if (cancelBtn) {
        const newCancelBtn = cancelBtn.cloneNode(true);
        cancelBtn.parentNode.replaceChild(newCancelBtn, cancelBtn);
        
        newCancelBtn.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            console.log('Cancel button clicked - showing modal');
            showCancelConfirmModal();
        });
    }
    
    // Setup save button
    const saveBtn = document.getElementById('saveBtn');
    if (saveBtn) {
        const newSaveBtn = saveBtn.cloneNode(true);
        saveBtn.parentNode.replaceChild(newSaveBtn, saveBtn);
        
        newSaveBtn.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            
            const titleInput = document.getElementById('title');
            const messageTextarea = document.getElementById('message');
            const title = titleInput ? titleInput.value.trim() : '';
            const message = messageTextarea ? messageTextarea.value.trim() : '';

            if (!title) {
                showErrorModal('Please enter an announcement title');
                if (titleInput) titleInput.focus();
                return;
            }

            if (!message) {
                showErrorModal('Please enter an announcement message');
                if (messageTextarea) messageTextarea.focus();
                return;
            }

            if (title.length > 200) {
                showErrorModal('Title cannot exceed 200 characters');
                if (titleInput) titleInput.focus();
                return;
            }

            if (message.length > 2000) {
                showErrorModal('Message cannot exceed 2000 characters');
                if (messageTextarea) messageTextarea.focus();
                return;
            }
            
            showSaveConfirmModal();
        });
    }
    
    // Setup form submit
    const editForm = document.getElementById('editForm');
    if (editForm) {
        editForm.addEventListener('submit', function(e) {
            e.preventDefault();
            validateAndSubmit(e);
        });
    }
});

function initializeCharacterCounters() {
    const titleInput = document.getElementById('title');
    const messageInput = document.getElementById('message');
    const titleCount = document.getElementById('titleCount');
    const messageCount = document.getElementById('messageCount');

    if (titleInput && titleCount) {
        titleInput.addEventListener('input', function() {
            titleCount.textContent = this.value.length;
        });
        titleCount.textContent = titleInput.value.length;
    }

    if (messageInput && messageCount) {
        messageInput.addEventListener('input', function() {
            messageCount.textContent = this.value.length;
        });
        messageCount.textContent = messageInput.value.length;
    }
}

function initializeLivePreview() {
    const titleInput = document.getElementById('title');
    const messageTextarea = document.getElementById('message');
    const previewTitle = document.getElementById('previewTitle');
    const previewMessage = document.getElementById('previewMessage');
    const refreshBtn = document.getElementById('refreshPreview');
    
    if (!previewTitle || !previewMessage) return;
    
    function updatePreview() {
        if (titleInput) {
            previewTitle.textContent = titleInput.value.trim() || 'Untitled Announcement';
        }
        if (messageTextarea) {
            let text = messageTextarea.value.substring(0, 500);
            if (messageTextarea.value.length > 500) text += '...';
            previewMessage.innerHTML = text.replace(/\n/g, '<br>');
        }
    }
    
    if (titleInput) titleInput.addEventListener('input', updatePreview);
    if (messageTextarea) messageTextarea.addEventListener('input', updatePreview);
    if (refreshBtn) refreshBtn.addEventListener('click', updatePreview);
    updatePreview();
}

function initializeDeleteModal() {
    const deleteBtn = document.getElementById('deleteBtn');
    const deleteModal = document.getElementById('deleteModal');
    const modalCancelBtn = document.getElementById('modalCancelBtn');
    const modalConfirmBtn = document.getElementById('modalConfirmBtn');
    
    if (deleteBtn) {
        const newDeleteBtn = deleteBtn.cloneNode(true);
        deleteBtn.parentNode.replaceChild(newDeleteBtn, deleteBtn);
        
        newDeleteBtn.addEventListener('click', () => {
            const titleInput = document.getElementById('title');
            const title = titleInput ? titleInput.value.trim() : window.announcementData?.title;
            
            // Show delete confirmation modal
            showDeleteConfirmModal(title);
        });
    }
}

// Show delete confirmation modal using AJAX
function showDeleteConfirmModal(title) {
    let modal = document.getElementById('deleteConfirmModal');
    if (!modal) {
        modal = document.createElement('div');
        modal.id = 'deleteConfirmModal';
        modal.className = 'modal-overlay-full';
        modal.style.display = 'none';
        modal.innerHTML = `
            <div class="modal-container">
                <div class="modal-icon warning-icon">
                    <i class="bi bi-exclamation-triangle-fill"></i>
                </div>
                <h3 class="modal-title">Confirm Delete</h3>
                <p class="modal-message" id="deleteConfirmMessage">Are you sure you want to delete this announcement?</p>
                <p id="deleteTitle" style="font-weight: 600; color: #ef4444; background: #fef2f2; padding: 10px; border-radius: 10px; margin: 16px 0;"></p>
                <div class="modal-warning">
                    <i class="bi bi-shield-exclamation"></i>
                    <span>This action cannot be undone.</span>
                </div>
                <div class="modal-actions">
                    <button class="modal-btn modal-cancel" id="deleteCancelBtn">
                        <i class="bi bi-x-circle"></i> Cancel
                    </button>
                    <button class="modal-btn modal-danger" id="deleteConfirmBtn">
                        <i class="bi bi-trash"></i> Delete
                    </button>
                </div>
            </div>
        `;
        document.body.appendChild(modal);
    }
    
    document.getElementById('deleteTitle').textContent = `"${title}"`;
    modal.style.display = 'flex';
    
    const cancelBtn = document.getElementById('deleteCancelBtn');
    const confirmBtn = document.getElementById('deleteConfirmBtn');
    
    // Remove old listeners
    const newCancelBtn = cancelBtn.cloneNode(true);
    cancelBtn.parentNode.replaceChild(newCancelBtn, cancelBtn);
    
    const newConfirmBtn = confirmBtn.cloneNode(true);
    confirmBtn.parentNode.replaceChild(newConfirmBtn, confirmBtn);
    
    newCancelBtn.onclick = () => {
        modal.style.display = 'none';
    };
    
    newConfirmBtn.onclick = () => {
        modal.style.display = 'none';
        performDelete();
    };
    
    modal.onclick = (e) => {
        if (e.target === modal) {
            modal.style.display = 'none';
        }
    };
}

// Perform delete using AJAX/Fetch
async function performDelete() {
    const id = window.announcementData?.id;
    if (!id) return;
    
    // Show loading state on delete button if needed
    const confirmBtn = document.getElementById('deleteConfirmBtn');
    if (confirmBtn) {
        confirmBtn.disabled = true;
        confirmBtn.innerHTML = '<i class="bi bi-hourglass-split"></i> Deleting...';
    }
    
    try {
        const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
        const token = tokenElement ? tokenElement.value : '';
        
        const formData = new FormData();
        formData.append('id', id);
        
        const response = await fetch(window.location.pathname + '?handler=Delete', {
            method: 'POST',
            headers: {
                'RequestVerificationToken': token
            },
            body: formData
        });
        
        const result = await response.json();
        
        if (result.success) {
            showSuccessModal('Announcement deleted successfully!', '/FrontEnd/AnnouncementList');
        } else {
            showErrorModal(result.message || 'Failed to delete announcement');
            if (confirmBtn) {
                confirmBtn.disabled = false;
                confirmBtn.innerHTML = '<i class="bi bi-trash"></i> Delete';
            }
        }
    } catch (error) {
        console.error('Delete Error:', error);
        showErrorModal('Error deleting announcement');
        if (confirmBtn) {
            confirmBtn.disabled = false;
            confirmBtn.innerHTML = '<i class="bi bi-trash"></i> Delete';
        }
    }
}

function showSaveConfirmModal() {
    let modal = document.getElementById('saveConfirmModal');
    if (!modal) {
        modal = document.createElement('div');
        modal.id = 'saveConfirmModal';
        modal.className = 'modal-overlay-full';
        modal.style.display = 'none';
        modal.innerHTML = `
            <div class="modal-container">
                <div class="modal-icon question-icon">
                    <i class="bi bi-question-circle-fill"></i>
                </div>
                <h3 class="modal-title">Save Changes?</h3>
                <p class="modal-message">Are you sure you want to save these changes?</p>
                <div class="modal-actions">
                    <button class="modal-btn modal-cancel" id="saveModalCancelBtn"><i class="bi bi-x-circle"></i> Cancel</button>
                    <button class="modal-btn modal-confirm" id="saveModalConfirmBtn"><i class="bi bi-check-circle"></i> Yes, Save</button>
                </div>
            </div>
        `;
        document.body.appendChild(modal);
    }
    
    modal.style.display = 'flex';
    
    const cancelBtn = document.getElementById('saveModalCancelBtn');
    const confirmBtn = document.getElementById('saveModalConfirmBtn');
    
    const newCancelBtn = cancelBtn.cloneNode(true);
    cancelBtn.parentNode.replaceChild(newCancelBtn, cancelBtn);
    
    const newConfirmBtn = confirmBtn.cloneNode(true);
    confirmBtn.parentNode.replaceChild(newConfirmBtn, confirmBtn);
    
    newCancelBtn.onclick = () => modal.style.display = 'none';
    newConfirmBtn.onclick = () => {
        modal.style.display = 'none';
        validateAndSubmit();
    };
    modal.onclick = (e) => { if (e.target === modal) modal.style.display = 'none'; };
}

function showCancelConfirmModal() {
    let modal = document.getElementById('cancelConfirmModal');
    if (!modal) {
        modal = document.createElement('div');
        modal.id = 'cancelConfirmModal';
        modal.className = 'modal-overlay-full';
        modal.style.display = 'none';
        modal.innerHTML = `
            <div class="modal-container">
                <div class="modal-icon warning-icon">
                    <i class="bi bi-exclamation-triangle-fill"></i>
                </div>
                <h3 class="modal-title">Cancel Edit</h3>
                <p class="modal-message">Are you sure you want to cancel? Any unsaved changes will be lost.</p>
                <div class="modal-warning">
                    <i class="bi bi-shield-exclamation"></i>
                    <span>Any unsaved changes will be lost.</span>
                </div>
                <div class="modal-actions">
                    <button class="modal-btn modal-cancel" id="cancelModalStayBtn"><i class="bi bi-x-circle"></i> Stay</button>
                    <button class="modal-btn modal-danger" id="cancelModalLeaveBtn"><i class="bi bi-trash3"></i> Leave</button>
                </div>
            </div>
        `;
        document.body.appendChild(modal);
    }
    
    modal.style.display = 'flex';
    
    const stayBtn = document.getElementById('cancelModalStayBtn');
    const leaveBtn = document.getElementById('cancelModalLeaveBtn');
    
    const newStayBtn = stayBtn.cloneNode(true);
    stayBtn.parentNode.replaceChild(newStayBtn, stayBtn);
    
    const newLeaveBtn = leaveBtn.cloneNode(true);
    leaveBtn.parentNode.replaceChild(newLeaveBtn, leaveBtn);
    
    newStayBtn.onclick = () => {
        console.log('Stay button clicked - staying on page');
        modal.style.display = 'none';
    };
    newLeaveBtn.onclick = () => {
        console.log('Leave button clicked - redirecting to list');
        modal.style.display = 'none';
        window.location.href = '/FrontEnd/AnnouncementList';
    };
    modal.onclick = (e) => { if (e.target === modal) modal.style.display = 'none'; };
}

function initializeSuccessPopup() {
    const popup = document.getElementById('successPopup');
    const continueBtn = document.getElementById('popupContinueBtn');
    if (popup) {
        popup.style.cssText = 'display:flex;position:fixed;top:0;left:0;width:100%;height:100%;z-index:10000;align-items:center;justify-content:center;background-color:rgba(0,0,0,0.5)';
        if (continueBtn) {
            const newContinueBtn = continueBtn.cloneNode(true);
            continueBtn.parentNode.replaceChild(newContinueBtn, continueBtn);
            newContinueBtn.onclick = () => window.location.href = '/FrontEnd/AnnouncementList';
        }
    }
}

function initializeErrorPopup() {
    const popup = document.getElementById('errorPopup');
    const continueBtn = document.getElementById('errorPopupContinueBtn');
    if (popup) {
        popup.style.cssText = 'display:flex;position:fixed;top:0;left:0;width:100%;height:100%;z-index:10000;align-items:center;justify-content:center;background-color:rgba(0,0,0,0.5)';
        if (continueBtn) {
            const newContinueBtn = continueBtn.cloneNode(true);
            continueBtn.parentNode.replaceChild(newContinueBtn, continueBtn);
            newContinueBtn.onclick = () => window.location.href = '/FrontEnd/AnnouncementList';
        }
    }
}

function validateAndSubmit(event) {
    if (event) event.preventDefault();
    
    const titleInput = document.getElementById('title');
    const messageTextarea = document.getElementById('message');
    const editForm = document.getElementById('editForm');
    
    const title = titleInput ? titleInput.value.trim() : '';
    const message = messageTextarea ? messageTextarea.value.trim() : '';

    if (!title) {
        showErrorModal('Please enter an announcement title');
        if (titleInput) titleInput.focus();
        return false;
    }

    if (!message) {
        showErrorModal('Please enter an announcement message');
        if (messageTextarea) messageTextarea.focus();
        return false;
    }

    if (title.length > 200) {
        showErrorModal('Title cannot exceed 200 characters');
        if (titleInput) titleInput.focus();
        return false;
    }

    if (message.length > 2000) {
        showErrorModal('Message cannot exceed 2000 characters');
        if (messageTextarea) messageTextarea.focus();
        return false;
    }
    
    // Submit the form directly without animation
    if (editForm) {
        editForm.submit();
    }
    
    return true;
}