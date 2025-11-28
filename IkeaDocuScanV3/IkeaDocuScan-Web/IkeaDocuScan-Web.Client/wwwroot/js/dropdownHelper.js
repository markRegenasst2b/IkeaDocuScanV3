// Dropdown Helper - Initializes Bootstrap dropdowns with Popper.js fixed strategy
// This allows dropdown menus to escape overflow:hidden/auto containers (like table-responsive)

window.dropdownHelper = {
    // Initialize all dropdowns within a container with fixed positioning strategy
    initializeFixedDropdowns: function (containerSelector) {
        var container = containerSelector ? document.querySelector(containerSelector) : document;
        if (!container) {
            console.warn('dropdownHelper: Container not found:', containerSelector);
            return;
        }

        var dropdownToggleList = container.querySelectorAll('[data-bs-toggle="dropdown"]');

        dropdownToggleList.forEach(function (dropdownToggleEl) {
            // Check if already initialized with our custom config
            if (dropdownToggleEl.hasAttribute('data-dropdown-fixed-initialized')) {
                return;
            }

            // Dispose existing dropdown instance if any
            var existingInstance = bootstrap.Dropdown.getInstance(dropdownToggleEl);
            if (existingInstance) {
                existingInstance.dispose();
            }

            // Create new dropdown with fixed positioning strategy
            new bootstrap.Dropdown(dropdownToggleEl, {
                popperConfig: function (defaultBsPopperConfig) {
                    return {
                        ...defaultBsPopperConfig,
                        strategy: 'fixed'
                    };
                }
            });

            // Mark as initialized
            dropdownToggleEl.setAttribute('data-dropdown-fixed-initialized', 'true');
        });
    },

    // Dispose all dropdown instances in a container
    disposeDropdowns: function (containerSelector) {
        var container = containerSelector ? document.querySelector(containerSelector) : document;
        if (!container) {
            return;
        }

        var dropdownToggleList = container.querySelectorAll('[data-bs-toggle="dropdown"]');

        dropdownToggleList.forEach(function (dropdownToggleEl) {
            var instance = bootstrap.Dropdown.getInstance(dropdownToggleEl);
            if (instance) {
                instance.dispose();
            }
            dropdownToggleEl.removeAttribute('data-dropdown-fixed-initialized');
        });
    }
};
