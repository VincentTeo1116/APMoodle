﻿// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Expandable Vertical Menu Toggle - Single implementation
document.addEventListener('DOMContentLoaded', function () {
    const menuContainer = document.getElementById('expandableMenu');
    const menuToggle = document.getElementById('menuToggle');
    const expandIcon = document.getElementById('expandIcon');
    const expandText = document.querySelector('.expand-text');

    if (menuToggle && menuContainer) {
        // Check localStorage for saved state
        const isExpanded = localStorage.getItem('menuExpanded') === 'true';
        
        // Apply saved state on load
        if (isExpanded) {
            menuContainer.classList.add('expanded');
            if (expandIcon) expandIcon.innerHTML = '❮';
            if (expandText) expandText.textContent = 'Collapse';
        } else {
            menuContainer.classList.remove('expanded');
            if (expandIcon) expandIcon.innerHTML = '❯';
            if (expandText) expandText.textContent = 'Expand';
        }
        
        // Toggle menu on button click
        menuToggle.addEventListener('click', function (e) {
            e.stopPropagation();
            menuContainer.classList.toggle('expanded');
            
            // Update icon and text
            if (expandIcon) {
                if (menuContainer.classList.contains('expanded')) {
                    expandIcon.innerHTML = '❮';
                    if (expandText) expandText.textContent = 'Collapse';
                    localStorage.setItem('menuExpanded', 'true');
                } else {
                    expandIcon.innerHTML = '❯';
                    if (expandText) expandText.textContent = 'Expand';
                    localStorage.setItem('menuExpanded', 'false');
                }
            }
        });
    }
});

// Navbar scroll effect
window.addEventListener('scroll', function() {
    const navbar = document.querySelector('.custom-navbar');
    if (navbar) {
        if (window.scrollY > 10) {
            navbar.classList.add('sticky-scroll');
        } else {
            navbar.classList.remove('sticky-scroll');
        }
    }
});

// Logout confirmation function
function confirmLogout() {
    if (confirm('Are you sure you want to logout?')) {
        window.location.href = '/FrontEnd/Logout';
    }
}