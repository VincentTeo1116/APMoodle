document.addEventListener("DOMContentLoaded", function () {

    const loginTab = document.getElementById("loginTab");
    const registerTab = document.getElementById("registerTab");

    if (!loginTab || !registerTab) return;

    // REGISTER PAGE → register tab active
    registerTab.classList.add("active");
    loginTab.classList.remove("active");

    // LOGIN CLICK → go login page
    loginTab.addEventListener("click", function () {
        window.location.href = "/FrontEnd/Login";
    });

    // DATE PICKER
    if (document.querySelector("#dob")) {
        flatpickr("#dob", {
            dateFormat: "Y-m-d",
            maxDate: "today",
            disableMobile: true,
            animate: true
        });
    }

});