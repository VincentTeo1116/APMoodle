// ─────────────────────────────────────────────
// PASSWORD TOGGLE
// ─────────────────────────────────────────────
function togglePassword(id) {

    const pass = document.getElementById(id);

    if (pass.type === "password") {
        pass.type = "text";
    }
    else {
        pass.type = "password";
    }
}

// ─────────────────────────────────────────────
// ELEMENTS
// ─────────────────────────────────────────────
const loginTab = document.getElementById("loginTab");
const registerTab = document.getElementById("registerTab");

const loginForm = document.getElementById("loginForm");
const registerForm = document.getElementById("registerForm");

const toggleBox = document.querySelector(".toggle-box");

// ─────────────────────────────────────────────
// LOGIN TAB
// ─────────────────────────────────────────────
loginTab.addEventListener("click", function () {

    // Active State
    loginTab.classList.add("active");
    registerTab.classList.remove("active");

    // Show Login Form
    loginForm.style.display = "block";

    // Hide Register Form
    registerForm.style.display = "none";

    // Move Glass Slider Left
    toggleBox.classList.remove("register-active");
});

// ─────────────────────────────────────────────
// REGISTER TAB
// ─────────────────────────────────────────────
registerTab.addEventListener("click", function () {

    // Active State
    registerTab.classList.add("active");
    loginTab.classList.remove("active");

    // Show Register Form
    registerForm.style.display = "block";

    // Hide Login Form
    loginForm.style.display = "none";

    // Move Glass Slider Right
    toggleBox.classList.add("register-active");
});

// ─────────────────────────────────────────────
// OPTIONAL SMOOTH FORM ANIMATION
// ─────────────────────────────────────────────
window.addEventListener("DOMContentLoaded", () => {

    loginForm.style.opacity = "1";
    registerForm.style.opacity = "0";

    loginForm.style.transition = "0.3s ease";
    registerForm.style.transition = "0.3s ease";
});

// ─────────────────────────────────────────────
// FORM SWITCH EFFECT
// ─────────────────────────────────────────────
function showLogin() {

    loginForm.style.display = "block";
    registerForm.style.display = "none";

    setTimeout(() => {
        loginForm.style.opacity = "1";
        registerForm.style.opacity = "0";
    }, 50);
}

function showRegister() {

    registerForm.style.display = "block";
    loginForm.style.display = "none";

    setTimeout(() => {
        registerForm.style.opacity = "1";
        loginForm.style.opacity = "0";
    }, 50);
}

// ─────────────────────────────────────────────
// TAB EVENTS WITH ANIMATION
// ─────────────────────────────────────────────
loginTab.addEventListener("click", function () {

    loginTab.classList.add("active");
    registerTab.classList.remove("active");

    toggleBox.classList.remove("register-active");

    showLogin();
});

registerTab.addEventListener("click", function () {

    registerTab.classList.add("active");
    loginTab.classList.remove("active");

    toggleBox.classList.add("register-active");

    showRegister();
});