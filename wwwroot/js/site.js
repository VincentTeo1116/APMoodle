// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// Expandable Vertical Menu Toggle
document.addEventListener('DOMContentLoaded', function () {
    const menuToggle = document.getElementById('menuToggle');
    const expandableMenu = document.getElementById('expandableMenu');
    const menuOverlay = document.getElementById('menuOverlay');

    if (menuToggle && expandableMenu && menuOverlay) {
        // Toggle menu on button click
        menuToggle.addEventListener('click', function (e) {
            e.stopPropagation();
            expandableMenu.classList.toggle('open');
            menuOverlay.classList.toggle('show');
        });

        // Close menu when clicking overlay
        menuOverlay.addEventListener('click', function () {
            expandableMenu.classList.remove('open');
            menuOverlay.classList.remove('show');
        });

        // Close menu when pressing ESC key
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape' && expandableMenu.classList.contains('open')) {
                expandableMenu.classList.remove('open');
                menuOverlay.classList.remove('show');
            }
        });
    }
});

// Toggle menu expansion
document.addEventListener('DOMContentLoaded', function() {
    const menuContainer = document.getElementById('expandableMenu');
    const menuToggle = document.getElementById('menuToggle');
    const expandIcon = document.getElementById('expandIcon');
    
    if (menuToggle && menuContainer) {
        menuToggle.addEventListener('click', function(e) {
            e.stopPropagation();
            menuContainer.classList.toggle('expanded');
            
            // Update icon text if needed
            if (expandIcon) {
                if (menuContainer.classList.contains('expanded')) {
                    expandIcon.textContent = '❮';  // Collapse icon
                } else {
                    expandIcon.textContent = '❯';  // Expand icon
                }
            }
        });
    }
});

window.addEventListener('scroll', function() {
    const navbar = document.querySelector('.custom-navbar');
    if (window.scrollY > 10) {
        navbar.classList.add('sticky-scroll');
    } else {
        navbar.classList.remove('sticky-scroll');
    }
});