document.addEventListener("DOMContentLoaded", function () {
    const loginTab = document.getElementById("loginTab");
    const registerTab = document.getElementById("registerTab");
    const form = document.getElementById("registerForm");
    const registerBtn = document.getElementById("registerBtn");

    const fullNameInput = document.getElementById("FullName");
    const dobInput = document.getElementById("dob");
    const contactInput = document.getElementById("Contact");
    const emailInput = document.getElementById("Email");
    const genderSelect = document.getElementById("Gender");

    if (dobInput) {
        flatpickr(dobInput, {
            dateFormat: "Y-m-d",
            maxDate: "today",
            disableMobile: true,
            animate: true,
            onChange: function(selectedDates) {
                validateDOB();
                document.getElementById('validationSummary').classList.remove('show');
                // Re‑enable button if it was disabled due to validation
                enableRegisterButton();
            }
        });
    }

    if (loginTab) {
        loginTab.addEventListener("click", function () {
            window.location.href = "/FrontEnd/Login";
        });
    }

    function disableRegisterButton() {
        if (registerBtn) {
            registerBtn.disabled = true;
            registerBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Registering...';
        }
    }

    function enableRegisterButton() {
        if (registerBtn) {
            registerBtn.disabled = false;
            registerBtn.innerHTML = 'REGISTER';
        }
    }

    const serverError = document.querySelector('.alert-error:not(.dynamic)');
    if (serverError) {
        enableRegisterButton();
    }


    function validateName() {
        const group = document.getElementById('nameGroup');
        const error = document.getElementById('nameError');
        const success = document.getElementById('nameSuccess');
        const value = fullNameInput.value.trim();
        if (value.length < 2) {
            group.classList.add('has-error');
            error.classList.add('show');
            success.classList.remove('show');
            return false;
        } else {
            group.classList.remove('has-error');
            error.classList.remove('show');
            success.classList.add('show');
            return true;
        }
    }

    function validateDOB() {
        const group = document.getElementById('dobGroup');
        const error = document.getElementById('dobError');
        const success = document.getElementById('dobSuccess');
        const value = dobInput.value;
        if (!value) {
            group.classList.add('has-error');
            error.textContent = 'Please select a valid date of birth (must be 15-80 years old).';
            error.classList.add('show');
            success.classList.remove('show');
            return false;
        }
        const dob = new Date(value);
        const today = new Date();
        let age = today.getFullYear() - dob.getFullYear();
        const monthDiff = today.getMonth() - dob.getMonth();
        if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < dob.getDate())) age--;
        if (age < 15 || age > 80) {
            group.classList.add('has-error');
            error.textContent = 'Please select a valid date of birth (must be 15-80 years old).';
            error.classList.add('show');
            success.classList.remove('show');
            return false;
        } else {
            group.classList.remove('has-error');
            error.classList.remove('show');
            success.classList.add('show');
            return true;
        }
    }

    function validateContact() {
        const group = document.getElementById('contactGroup');
        const error = document.getElementById('contactError');
        const success = document.getElementById('contactSuccess');
        const value = contactInput.value.trim();
        const phonePattern = /^0\d{9,10}$/;
        if (!phonePattern.test(value)) {
            group.classList.add('has-error');
            error.classList.add('show');
            success.classList.remove('show');
            return false;
        } else {
            group.classList.remove('has-error');
            error.classList.remove('show');
            success.classList.add('show');
            return true;
        }
    }

    function validateEmail() {
        const group = document.getElementById('emailGroup');
        const error = document.getElementById('emailError');
        const success = document.getElementById('emailSuccess');
        const value = emailInput.value.trim();
        const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailPattern.test(value)) {
            group.classList.add('has-error');
            error.classList.add('show');
            success.classList.remove('show');
            return false;
        } else {
            group.classList.remove('has-error');
            error.classList.remove('show');
            success.classList.add('show');
            return true;
        }
    }

    function validateGender() {
        const group = document.getElementById('genderGroup');
        const error = document.getElementById('genderError');
        const success = document.getElementById('genderSuccess');
        const value = genderSelect.value;
        if (!value) {
            group.classList.add('has-error');
            error.classList.add('show');
            success.classList.remove('show');
            return false;
        } else {
            group.classList.remove('has-error');
            error.classList.remove('show');
            success.classList.add('show');
            return true;
        }
    }

    fullNameInput.addEventListener('input', function() {
        validateName();
        // Re‑enable button if it was disabled due to validation error
        if (registerBtn && registerBtn.disabled) {
            enableRegisterButton();
        }
    });
    fullNameInput.addEventListener('blur', validateName);

    contactInput.addEventListener('input', function() {
        validateContact();
        if (registerBtn && registerBtn.disabled) {
            enableRegisterButton();
        }
    });
    contactInput.addEventListener('blur', validateContact);

    emailInput.addEventListener('input', function() {
        validateEmail();
        if (registerBtn && registerBtn.disabled) {
            enableRegisterButton();
        }
    });
    emailInput.addEventListener('blur', validateEmail);

    genderSelect.addEventListener('change', function() {
        validateGender();
        if (registerBtn && registerBtn.disabled) {
            enableRegisterButton();
        }
    });

    form.addEventListener('submit', function(e) {
        const isNameValid = validateName();
        const isDOBValid = validateDOB();
        const isContactValid = validateContact();
        const isEmailValid = validateEmail();
        const isGenderValid = validateGender();

        const errors = [];
        if (!isNameValid) errors.push('Please enter a valid name (at least 2 characters).');
        if (!isDOBValid) errors.push('Please select a valid date of birth (must be 15-80 years old).');
        if (!isContactValid) errors.push('Please enter a valid phone number (10-11 digits, starting with 0).');
        if (!isEmailValid) errors.push('Please enter a valid email address.');
        if (!isGenderValid) errors.push('Please select a gender.');

        if (errors.length > 0) {
            e.preventDefault();
            const summary = document.getElementById('validationSummary');
            const list = document.getElementById('validationList');
            list.innerHTML = errors.map(err => `<li>${err}</li>`).join('');
            summary.classList.add('show');
            const firstError = document.querySelector('.form-group.has-error');
            if (firstError) {
                firstError.scrollIntoView({ behavior: 'smooth', block: 'center' });
                const input = firstError.querySelector('input, select');
                if (input) input.focus();
            }
            // Re‑enable button because validation failed
            enableRegisterButton();
            return false;
        }

        disableRegisterButton();

        return true;
    });

    document.querySelectorAll('input, select').forEach(function(el) {
        el.addEventListener('input', function() {
            document.getElementById('validationSummary').classList.remove('show');
        });
        el.addEventListener('change', function() {
            document.getElementById('validationSummary').classList.remove('show');
        });
    });

});