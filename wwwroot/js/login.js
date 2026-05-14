document.addEventListener("DOMContentLoaded", function () {

    const loginTab = document.getElementById("loginTab");
    const registerTab = document.getElementById("registerTab");
    const loginForm = document.getElementById("loginForm");
    const registerForm = document.getElementById("registerForm");
    const registerLink = document.querySelector('.register-text a');

    // Tab switching functionality
    if (loginTab && registerTab) {
        // LOGIN CLICK
        loginTab.addEventListener("click", function () {
            loginTab.classList.add("active");
            registerTab.classList.remove("active");

            if (loginForm) loginForm.style.display = "block";
            if (registerForm) registerForm.style.display = "none";
        });

        // REGISTER CLICK → redirect to register page
        registerTab.addEventListener("click", function () {
            window.location.href = "FrontEnd/Register";
        });
    }

    // Handle "JOIN US NOW" link
    if (registerLink) {
        registerLink.addEventListener('click', function(e) {
            e.preventDefault();
            window.location.href = "FrontEnd/Register";
        });
    }

    // Client-side validation for login form
    if (loginForm) {
        loginForm.addEventListener('submit', function(e) {
            const userIdInput = loginForm.querySelector('input[name="Input.UserId"]');
            const passwordInput = loginForm.querySelector('input[name="Input.Password"]');
            
            let hasError = false;
            let errorMessage = "";
            
            // Remove any existing dynamic error messages
            const existingError = document.querySelector('.alert-error.dynamic');
            if (existingError) {
                existingError.remove();
            }
            
            // Validate User ID
            if (!userIdInput || !userIdInput.value.trim()) {
                errorMessage = "Please enter User ID or Email";
                hasError = true;
            }
            // Validate Password
            else if (!passwordInput || !passwordInput.value) {
                errorMessage = "Please enter your password";
                hasError = true;
            }
            
            if (hasError) {
                e.preventDefault();
                showErrorMessage(errorMessage);
            }
        });
    }
    
    // Function to show error message
    function showErrorMessage(message) {
        // Remove any existing dynamic error messages
        const existingError = document.querySelector('.alert-error.dynamic');
        if (existingError) {
            existingError.remove();
        }
        
        // Create new error message
        const errorDiv = document.createElement('div');
        errorDiv.className = 'alert-error dynamic';
        errorDiv.innerHTML = `❌ ${message}`;
        
        // Insert at the top of the form
        if (loginForm) {
            loginForm.insertBefore(errorDiv, loginForm.firstChild);
        }
        
        // Auto-remove after 4 seconds
        setTimeout(() => {
            if (errorDiv.parentNode) {
                errorDiv.remove();
            }
        }, 4000);
    }
    
    // Input cleanup on focus (remove error styling)
    const inputs = document.querySelectorAll('input');
    inputs.forEach(input => {
        input.addEventListener('focus', function() {
            this.style.borderColor = '';
            // Remove any dynamic error when user starts typing
            const dynamicError = document.querySelector('.alert-error.dynamic');
            if (dynamicError) {
                dynamicError.remove();
            }
        });
    });
    
    // Prevent form resubmission on page refresh
    if (window.history.replaceState) {
        window.history.replaceState(null, null, window.location.href);
    }
});

// PASSWORD TOGGLE
function togglePassword() {
    const pass = document.getElementById("password");

    if (!pass) return;

    pass.type = pass.type === "password" ? "text" : "password";
}