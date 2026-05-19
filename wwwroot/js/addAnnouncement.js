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
        
        // Force the popup to be visible and centered
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
        
        // Handle continue button click
        if (continueBtn) {
            continueBtn.onclick = function(e) {
                e.preventDefault();
                window.location.href = '/FrontEnd/AnnouncementList';
            };
        }
        
        // Click on overlay to close
        const overlay = popup.querySelector('.popup-overlay');
        if (overlay) {
            overlay.onclick = function() {
                window.location.href = '/FrontEnd/AnnouncementList';
            };
        }
    }
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
        cancelBtn.addEventListener('click', cancelAnnouncement);
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
        alert('Please enter an announcement title');
        if (titleInput) titleInput.focus();
        return;
    }

    if (!message) {
        alert('Please enter an announcement message');
        if (messageInput) messageInput.focus();
        return;
    }

    if (title.length > 200) {
        alert('Title cannot exceed 200 characters');
        return;
    }

    if (message.length > 2000) {
        alert('Message cannot exceed 2000 characters');
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
        alert('Please complete all steps before publishing');
        goToStep(1);
        return false;
    }
    return true;
}

// Cancel announcement creation
function cancelAnnouncement() {
    window.location.href = '/FrontEnd/AnnouncementList';
}