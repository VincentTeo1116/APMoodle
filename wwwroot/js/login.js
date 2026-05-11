document.addEventListener("DOMContentLoaded", function () {

    const loginTab = document.getElementById("loginTab");
    const registerTab = document.getElementById("registerTab");

    const loginForm = document.getElementById("loginForm");
    const registerForm = document.getElementById("registerForm");

    // if elements not exist, stop
    if (!loginTab || !registerTab) return;

    // LOGIN CLICK
    loginTab.addEventListener("click", function () {

        loginTab.classList.add("active");
        registerTab.classList.remove("active");

        if (loginForm) loginForm.style.display = "block";
        if (registerForm) registerForm.style.display = "none";
    });

    // REGISTER CLICK → redirect to register page
    registerTab.addEventListener("click", function () {

        window.location.href = "/FrontEnd/Register";
    });

});

// PASSWORD TOGGLE
function togglePassword() {
    const pass = document.getElementById("password");

    if (!pass) return;

    pass.type = pass.type === "password" ? "text" : "password";
}