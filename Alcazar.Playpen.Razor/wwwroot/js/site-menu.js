// Handle main menu and nested dropdowns
document.addEventListener('DOMContentLoaded', function () {

    // MAIN MENU HOVER FUNCTIONALITY
    document.querySelectorAll('.navbar-nav .dropdown').forEach(function (dropdown) {
        let hoverTimer;

        dropdown.addEventListener('mouseenter', function (e) {
            // On desktop, show main dropdown on hover
            if (window.innerWidth > 768) {
                clearTimeout(hoverTimer);
                const dropdownMenu = this.querySelector('.dropdown-menu');
                if (dropdownMenu) {
                    // Close any other open dropdowns first
                    document.querySelectorAll('.navbar-nav .dropdown .dropdown-menu').forEach(function (otherMenu) {
                        if (otherMenu !== dropdownMenu) {
                            otherMenu.style.display = 'none';
                        }
                    });

                    dropdownMenu.style.display = 'block';
                    this.classList.add('show');
                    dropdownMenu.classList.add('show');
                }
            }
        });

        dropdown.addEventListener('mouseleave', function (e) {
            // On desktop, hide main dropdown when mouse leaves (with small delay)
            if (window.innerWidth > 768) {
                const dropdownMenu = this.querySelector('.dropdown-menu');
                const dropdown = this;

                hoverTimer = setTimeout(function () {
                    if (dropdownMenu) {
                        dropdownMenu.style.display = 'none';
                        dropdown.classList.remove('show');
                        dropdownMenu.classList.remove('show');

                        // Also hide any submenus
                        dropdown.querySelectorAll('.dropdown-submenu .dropdown-menu').forEach(function (submenu) {
                            submenu.style.display = 'none';
                        });
                    }
                }, 100); // Small delay to prevent flickering
            }
        });
    });

    // SUBMENU HOVER FUNCTIONALITY
    document.querySelectorAll('.dropdown-submenu').forEach(function (element) {
        element.addEventListener('mouseenter', function (e) {
            // On desktop, show submenu on hover
            if (window.innerWidth > 768) {
                const submenu = this.querySelector('.dropdown-menu');
                if (submenu) {
                    // Hide other submenus in the same parent dropdown
                    const parentDropdown = this.closest('.dropdown-menu');
                    if (parentDropdown) {
                        parentDropdown.querySelectorAll('.dropdown-submenu .dropdown-menu').forEach(function (otherSubmenu) {
                            if (otherSubmenu !== submenu) {
                                otherSubmenu.style.display = 'none';
                            }
                        });
                    }

                    submenu.style.display = 'block';
                }
            }
        });

        element.addEventListener('mouseleave', function (e) {
            // On desktop, hide submenu when mouse leaves
            if (window.innerWidth > 768) {
                const submenu = this.querySelector('.dropdown-menu');
                if (submenu) {
                    submenu.style.display = 'none';
                }
            }
        });
    });

    // MOBILE/FALLBACK CLICK FUNCTIONALITY

    // Handle click events for dropdown submenus on mobile and as fallback
    document.querySelectorAll('.dropdown-submenu .dropdown-toggle').forEach(function (element) {
        element.addEventListener('click', function (e) {
            // On mobile or as fallback, prevent the default link behavior and toggle the submenu
            if (window.innerWidth <= 768) {
                e.preventDefault();
                e.stopPropagation();

                const submenu = this.nextElementSibling;
                if (submenu) {
                    const isVisible = submenu.style.display === 'block';
                    submenu.style.display = isVisible ? 'none' : 'block';
                }
            }
        });
    });

    // Prevent the main dropdown from closing when clicking on submenu items
    document.querySelectorAll('.dropdown-submenu .dropdown-menu').forEach(function (submenu) {
        submenu.addEventListener('click', function (e) {
            e.stopPropagation();
        });
    });

    // CLEANUP AND UTILITY FUNCTIONS

    // Close all dropdowns when clicking outside
    document.addEventListener('click', function (e) {
        if (!e.target.closest('.dropdown')) {
            // Close main dropdowns
            document.querySelectorAll('.navbar-nav .dropdown').forEach(function (dropdown) {
                const dropdownMenu = dropdown.querySelector('.dropdown-menu');
                if (dropdownMenu) {
                    dropdownMenu.style.display = 'none';
                    dropdown.classList.remove('show');
                    dropdownMenu.classList.remove('show');
                }
            });

            // Close submenus
            document.querySelectorAll('.dropdown-submenu .dropdown-menu').forEach(function (submenu) {
                submenu.style.display = 'none';
            });
        }
    });

    // Handle window resize to reset menu display
    window.addEventListener('resize', function () {
        // Reset all dropdown states on resize
        document.querySelectorAll('.navbar-nav .dropdown').forEach(function (dropdown) {
            const dropdownMenu = dropdown.querySelector('.dropdown-menu');
            if (dropdownMenu) {
                dropdownMenu.style.display = 'none';
                dropdown.classList.remove('show');
                dropdownMenu.classList.remove('show');
            }
        });

        document.querySelectorAll('.dropdown-submenu .dropdown-menu').forEach(function (submenu) {
            submenu.style.display = 'none';
        });
    });

    // Handle escape key to close dropdowns
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
// Close all dropdowns on 