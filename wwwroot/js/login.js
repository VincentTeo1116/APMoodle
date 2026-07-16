document.addEventListener("DOMContentLoaded", function () {

    const loginTab = document.getElementById("loginTab");
    const registerTab = document.getElementById("registerTab");
    const loginForm = document.getElementById("loginForm");
    const registerForm = document.getElementById("registerForm");
    const registerLink = document.querySelector('.register-text a');

    const togglePasswordBtn = document.getElementById('togglePassword');
    const passwordInput = document.getElementById('password');

    const loginBtn = loginForm?.querySelector('button[type="submit"]');
    const registerBtn = registerTab; 

    if (togglePasswordBtn && passwordInput) {
        togglePasswordBtn.addEventListener('click', function () {
            const type = passwordInput.getAttribute('type') === 'password' ? 'text' : 'password';
            passwordInput.setAttribute('type', type);
            const icon = this.querySelector('i');
            if (icon) {
                if (type === 'password') {
                    icon.classList.remove('fa-eye');
                    icon.classList.add('fa-eye-slash');
                } else {
                    icon.classList.remove('fa-eye-slash');
                    icon.classList.add('fa-eye');
                }
            }
        });
    }

    if (loginTab && registerTab) {
        loginTab.addEventListener("click", function () {
            loginTab.classList.add("active");
            registerTab.classList.remove("active");
            if (loginForm) loginForm.style.display = "block";
            if (registerForm) registerForm.style.display = "none";
        });

        registerTab.addEventListener("click", function () {
            // Disable register button to prevent double clicks before redirect
            if (registerTab) {
                registerTab.disabled = true;
                registerTab.textContent = 'Redirecting...';
            }
            window.location.href = "/FrontEnd/Register";
        });
    }

    if (registerLink) {
        registerLink.addEventListener('click', function(e) {
            e.preventDefault();
            if (registerTab) {
                registerTab.disabled = true;
                registerTab.textContent = 'Redirecting...';
            }
            window.location.href = "/FrontEnd/Register";
        });
    }

    if (loginForm) {
        loginForm.addEventListener('submit', function(e) {
            const userIdInput = loginForm.querySelector('input[name="Input.UserId"]');
            const passwordInputField = loginForm.querySelector('input[name="Input.Password"]');
            
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
            else if (!passwordInputField || !passwordInputField.value) {
                errorMessage = "Please enter your password";
                hasError = true;
            }
            
            if (hasError) {
                e.preventDefault();
                showErrorMessage(errorMessage);
                // Re-enable login button if it was disabled
                if (loginBtn) {
                    loginBtn.disabled = false;
                    loginBtn.innerHTML = 'LOGIN';
                }
                return;
            }

            if (loginBtn) {
                loginBtn.disabled = true;
                loginBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Logging in...';
            }
        });
    }

    function showErrorMessage(message) {
        const existingError = document.querySelector('.alert-error.dynamic');
        if (existingError) {
            existingError.remove();
        }
        const errorDiv = document.createElement('div');
        errorDiv.className = 'alert-error dynamic';
        errorDiv.innerHTML = `❌ ${message}`;
        if (loginForm) {
            loginForm.insertBefore(errorDiv, loginForm.firstChild);
        }
        setTimeout(() => {
            if (errorDiv.parentNode) {
                errorDiv.remove();
            }
        }, 4000);
    }

    const serverError = document.querySelector('.alert-error:not(.dynamic)');
    if (serverError && loginBtn) {
        loginBtn.disabled = false;
        loginBtn.innerHTML = 'LOGIN';
    }

    const inputs = document.querySelectorAll('input');
    inputs.forEach(input => {
        input.addEventListener('focus', function() {
            this.style.borderColor = '';
            const dynamicError = document.querySelector('.alert-error.dynamic');
            if (dynamicError) {
                dynamicError.remove();
            }
            if (loginBtn && loginBtn.disabled) {
                loginBtn.disabled = false;
                loginBtn.innerHTML = 'LOGIN';
            }
        });
    });

    if (window.history.replaceState) {
        window.history.replaceState(null, null, window.location.href);
    }
});