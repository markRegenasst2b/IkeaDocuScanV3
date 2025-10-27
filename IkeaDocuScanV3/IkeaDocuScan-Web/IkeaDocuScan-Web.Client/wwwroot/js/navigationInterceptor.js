// Navigation interception for unsaved changes warning
window.documentPropertiesPage = {
    dotNetRef: null,
    clickHandler: null,
    allowNextNavigation: false,

    init: function(dotNetRef) {
        this.dotNetRef = dotNetRef;

        // Create click handler that intercepts navigation
        this.clickHandler = (e) => {
            // Allow programmatic navigation
            if (this.allowNextNavigation) {
                this.allowNextNavigation = false;
                return;
            }

            const link = e.target.closest('a[href]');

            if (link && link.href && !link.target) {
                const href = link.getAttribute('href');

                // Skip hash links and javascript: links
                if (href && !href.startsWith('#') && !href.startsWith('javascript:')) {
                    window.documentPropertiesPage.handleNavigation(e, href, link);
                }
            }
        };

        // Attach handler in capture phase to intercept before Blazor
        document.addEventListener('click', this.clickHandler, true);
    },

    handleNavigation: function(e, targetUrl, originalLink) {
        // Prevent all navigation immediately
        e.preventDefault();
        e.stopPropagation();
        e.stopImmediatePropagation();

        // Check asynchronously if we should allow navigation
        (async () => {
            try {
                const hasUnsavedChanges = await this.dotNetRef.invokeMethodAsync('CheckHasUnsavedChanges');

                if (hasUnsavedChanges) {
                    // Use native browser confirm dialog
                    const confirmed = confirm('You have unsaved changes. Are you sure you want to leave this page?');

                    if (confirmed) {
                        await this.dotNetRef.invokeMethodAsync('ClearUnsavedChangesFlag');
                        // Allow the next navigation and trigger the original link
                        this.allowNextNavigation = true;
                        originalLink.click();
                    }
                    // If not confirmed, do nothing (navigation already prevented)
                } else {
                    // No unsaved changes, allow navigation
                    this.allowNextNavigation = true;
                    originalLink.click();
                }
            } catch (err) {
                console.error('Navigation interception error:', err);
                // On error, allow navigation
                this.allowNextNavigation = true;
                originalLink.click();
            }
        })();

        return false;
    },

    dispose: function() {
        if (this.clickHandler) {
            document.removeEventListener('click', this.clickHandler, true);
        }
        this.clickHandler = null;
        this.dotNetRef = null;
    }
};
