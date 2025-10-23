// File Preview Enhancement Functions

window.filePreview = {
    // Full-screen functionality
    toggleFullScreen: function (elementId) {
        const element = document.getElementById(elementId);
        if (!element) {
            console.error('Element not found:', elementId);
            return false;
        }

        if (!document.fullscreenElement) {
            // Enter fullscreen
            if (element.requestFullscreen) {
                element.requestFullscreen();
            } else if (element.webkitRequestFullscreen) { // Safari
                element.webkitRequestFullscreen();
            } else if (element.msRequestFullscreen) { // IE11
                element.msRequestFullscreen();
            }
            return true;
        } else {
            // Exit fullscreen
            if (document.exitFullscreen) {
                document.exitFullscreen();
            } else if (document.webkitExitFullscreen) { // Safari
                document.webkitExitFullscreen();
            } else if (document.msExitFullscreen) { // IE11
                document.msExitFullscreen();
            }
            return false;
        }
    },

    // Check if fullscreen is supported
    isFullscreenSupported: function () {
        return !!(
            document.fullscreenEnabled ||
            document.webkitFullscreenEnabled ||
            document.msFullscreenEnabled
        );
    },

    // Monitor fullscreen state changes
    onFullscreenChange: function (dotnetHelper) {
        const handler = () => {
            const isFullscreen = !!(
                document.fullscreenElement ||
                document.webkitFullscreenElement ||
                document.msFullscreenElement
            );
            dotnetHelper.invokeMethodAsync('OnFullscreenChanged', isFullscreen);
        };

        document.addEventListener('fullscreenchange', handler);
        document.addEventListener('webkitfullscreenchange', handler);
        document.addEventListener('msfullscreenchange', handler);

        return {
            dispose: () => {
                document.removeEventListener('fullscreenchange', handler);
                document.removeEventListener('webkitfullscreenchange', handler);
                document.removeEventListener('msfullscreenchange', handler);
            }
        };
    }
};
