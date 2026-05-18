// DOM Elements
let currentStep = 1;
let formData = {
    title: '',
    message: ''
};

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    console.log('DOM loaded - initializing');
    initializeCharacterCounters();
    attachEventListeners();
    checkForSuccessAndShowPopup();
});

// Check if we should show success popup
function checkForSuccessAndShowPopup() {
    // Check for success parameter in URL
    const urlParams = new URLSearchParams(window.location.search);
    const successParam = urlParams.get('success');
    
    console.log('Checking for success popup, success param:', successParam);
    
    if (successParam === 'true') {
        console.log('Success param found, showing popup');
        // Show simple alert popup
        alert('✓ Announcement published successfully!');
        
        // Redirect to announcement list after OK is clicked
        window.location.href = '/FrontEnd/AnnouncementList';
    } else {
        console.log('No success param found');
    }
}

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

    // Validation
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

    // Store form data
    formData.title = title;
    formData.message = message;

    // Update review content
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

// Export for debugging
window.addAnnouncementHelpers = {
    goToStep,
    goToReview,
    goToPublish,
    cancelAnnouncement,
    checkForSuccessAndShowPopup
};