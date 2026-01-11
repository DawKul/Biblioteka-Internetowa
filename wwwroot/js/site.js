// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Theme toggle functionality
(function() {
    'use strict';

    const body = document.getElementById('bodyElement');
    const themeToggle = document.getElementById('themeToggle');
    const themeIcon = document.getElementById('themeIcon');

    // Load user theme preference on page load
    async function loadTheme() {
        try {
            const response = await fetch('/Theme/GetTheme');
            if (response.ok) {
                const data = await response.json();
                applyTheme(data.isDarkMode);
            }
        } catch (error) {
            console.error('Error loading theme:', error);
        }
    }

    // Apply theme to body
    function applyTheme(isDarkMode) {
        if (isDarkMode) {
            body.classList.add('dark-mode');
            if (themeIcon) {
                themeIcon.textContent = '☀️';
            }
        } else {
            body.classList.remove('dark-mode');
            if (themeIcon) {
                themeIcon.textContent = '🌙';
            }
        }
    }

    // Toggle theme
    async function toggleTheme() {
        try {
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
            const response = await fetch('/Theme/Toggle', {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': token,
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const data = await response.json();
                applyTheme(data.isDarkMode);
            } else {
                console.error('Error toggling theme:', response.statusText);
            }
        } catch (error) {
            console.error('Error toggling theme:', error);
        }
    }

    // Initialize theme on page load
    if (body) {
        loadTheme();
    }

    // Add click event listener to theme toggle button
    if (themeToggle) {
        themeToggle.addEventListener('click', toggleTheme);
    }
})();