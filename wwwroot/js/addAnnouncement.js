// DOM Elements
let currentStep = 1;
let formData = {
    title: '',
    message: ''
};

// Initialize success popup
function initializeSuccessPopup() {
    const popup = document.getElementById('successPopup');
    const continueBtn = document.getElementById('popupContinueBtn');
    
    if (popup) {
        console.log('Success popup found');
        
        popup.style.display = 'flex';
        popup.style.position = 'fixed';
        popup.style.top = '0';
        popup.style.left = '0';
        popup.style.width = '100%';
        popup.style.height = '100%';
        popup.style.zIndex = '10000';
        popup.style.alignItems = 'center';
        popup.style.justifyContent = 'center';
        popup.style.backgroundColor = 'rgba(0, 0, 0, 0.5)';
        
        if (continueBtn) {
            continueBtn.onclick = function(e) {
                e.preventDefault();
                window.location.href = '/FrontEnd/AnnouncementList';
            };
        }
        
        const overlay = popup.querySelector('.popup-overlay');
        if (overlay) {
            overlay.onclick = function() {
                window.location.href = '/FrontEnd/AnnouncementList';
            };
        }
    }
}

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

// Show cancel confirmation modal
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
                <h3 class="modal-title">Cancel Creation</h3>
                <p class="modal-message">Are you sure you want to cancel? Your announcement will not be saved.</p>
                <div class="modal-warning">
                    <i class="bi bi-shield-exclamation"></i>
                    <span>Any unsaved changes will be lost.</span>
                </div>
                <div class="modal-actions">
                    <button class="modal-btn modal-cancel" id="cancelModalStayBtn">
                        <i class="bi bi-x-circle"></i> Stay
                    </button>
                    <button class="modal-btn modal-danger" id="cancelModalLeaveBtn">
                        <i class="bi bi-trash3"></i> Leave
                    </button>
                </div>
            </div>
        `;
        document.body.appendChild(modal);
    }
    
    modal.style.display = 'flex';
    
    const stayBtn = document.getElementById('cancelModalStayBtn');
    const leaveBtn = document.getElementById('cancelModalLeaveBtn');
    
    // Remove old listeners
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
    
    modal.onclick = (e) => {
        if (e.target === modal) {
            modal.style.display = 'none';
        }
    };
}

// Make sure to call this when DOM loads
document.addEventListener('DOMContentLoaded', function() {
    console.log('DOM loaded - initializing');
    initializeCharacterCounters();
    attachEventListeners();
    initializeSuccessPopup();
});

// Initialize character counters
function initializeCharacterCounters() {
    const titleInput = document.getElementById('title');
    const messageInput = document.getElementById('message');
    const titleCount = document.getElementById('titleCount');
    const messageCount = document.getElementById('messageCount');

    if (titleInput) {
        titleInput.addEventListener('input', function() {
            titleCount.textContent = this.value.length;
        });
        titleCount.textContent = titleInput.value.length;
    }

    if (messageInput) {
        messageInput.addEventListener('input', function() {
            messageCount.textContent = this.value.length;
        });
        messageCount.textContent = messageInput.value.length;
    }
}

// Attach event listeners
function attachEventListeners() {
    const nextToReview = document.getElementById('nextToReview');
    const nextToPublish = document.getElementById('nextToPublish');
    const backToDetails = document.getElementById('backToDetails');
    const backToReview = document.getElementById('backToReview');
    const cancelBtn = document.getElementById('cancelBtn1');
    const publishForm = document.getElementById('publishForm');

    if (nextToReview) {
        nextToReview.addEventListener('click', goToReview);
    }
    if (nextToPublish) {
        nextToPublish.addEventListener('click', goToPublish);
    }
    if (backToDetails) {
        backToDetails.addEventListener('click', () => goToStep(1));
    }
    if (backToReview) {
        backToReview.addEventListener('click', () => goToStep(2));
    }
    if (cancelBtn) {
        cancelBtn.addEventListener('click', function(e) {
            e.preventDefault();
            // ALWAYS show the modal, even if nothing is entered
            showCancelConfirmModal();
        });
    }
    if (publishForm) {
        publishForm.addEventListener('submit', validateAndSubmit);
    }
}

// Go to Review step
function goToReview() {
    const titleInput = document.getElementById('title');
    const messageInput = document.getElementById('message');
    
    const title = titleInput ? titleInput.value.trim() : '';
    const message = messageInput ? messageInput.value.trim() : '';

    if (!title) {
        showErrorModal('Please enter an announcement title');
        if (titleInput) titleInput.focus();
        return;
    }

    if (!message) {
        showErrorModal('Please enter an announcement message');
        if (messageInput) messageInput.focus();
        return;
    }

    if (title.length > 200) {
        showErrorModal('Title cannot exceed 200 characters');
        if (titleInput) titleInput.focus();
        return;
    }

    if (message.length > 2000) {
        showErrorModal('Message cannot exceed 2000 characters');
        if (messageInput) messageInput.focus();
        return;
    }

    formData.title = title;
    formData.message = message;

    const reviewTitle = document.getElementById('reviewTitle');
    const reviewMessage = document.getElementById('reviewMessage');
    const publishTitle = document.getElementById('publishTitle');
    const publishLength = document.getElementById('publishLength');

    if (reviewTitle) reviewTitle.textContent = title;
    if (reviewMessage) reviewMessage.textContent = message;
    if (publishTitle) publishTitle.textContent = title;
    if (publishLength) publishLength.textContent = `${message.length} characters`;

    goToStep(2);
}

// Go to Publish step
function goToPublish() {
    const hiddenTitle = document.getElementById('hiddenTitle');
    const hiddenMessage = document.getElementById('hiddenMessage');
    
    if (hiddenTitle) hiddenTitle.value = formData.title;
    if (hiddenMessage) hiddenMessage.value = formData.message;
    
    goToStep(3);
}

// Navigate to specific step
function goToStep(step) {
    currentStep = step;
    
    const step1Content = document.getElementById('step1Content');
    const step2Content = document.getElementById('step2Content');
    const step3Content = document.getElementById('step3Content');
    
    if (step1Content) step1Content.style.display = 'none';
    if (step2Content) step2Content.style.display = 'none';
    if (step3Content) step3Content.style.display = 'none';
    
    const currentContent = document.getElementById(`step${step}Content`);
    if (currentContent) currentContent.style.display = 'block';
    
    updateProgressBar(step);
}

// Update progress bar
function updateProgressBar(step) {
    const steps = document.querySelectorAll('.step');
    const progressFill = document.getElementById('progressFill');
    
    let percentage = step === 1 ? 33 : step === 2 ? 66 : 100;
    if (progressFill) progressFill.style.width = `${percentage}%`;
    
    steps.forEach((stepEl, index) => {
        const stepNumber = index + 1;
        if (stepNumber <= step) {
            stepEl.classList.add('active');
        } else {
            stepEl.classList.remove('active');
        }
    });
}

// Validate and submit form
function validateAndSubmit(event) {
    if (!formData.title || !formData.message) {
        event.preventDefault();
        showErrorModal('Please complete all steps before publishing');
        goToStep(1);
        return false;
    }
    return true;
}