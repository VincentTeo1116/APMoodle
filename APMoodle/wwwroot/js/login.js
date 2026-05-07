
function togglePassword() {
    var pass = document.getElementById("password");

    if (pass.type === "password") {
        pass.type = "text";
    } else {
        pass.type = "password";
    }
}

// LOGIN TAB
document.getElementById("loginTab").addEventListener("click", function () {
    this.classList.add("active");
    document.getElementById("registerTab").classList.remove("active");

    document.getElementById("loginForm").style.display = "block";
    document.getElementById("registerForm").style.display = "none";
});

// REGISTER TAB
document.getElementById("registerTab").addEventListener("click", function () {
    this.classList.add("active");
    document.getElementById("loginTab").classList.remove("active");

    document.getElementById("loginForm").style.display = "none";
    document.getElementById("registerForm").style.display = "block";
});