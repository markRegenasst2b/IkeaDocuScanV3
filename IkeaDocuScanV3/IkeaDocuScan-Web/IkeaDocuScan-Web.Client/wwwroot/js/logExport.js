// Log Export Helper
window.logExportHelper = {
    triggerDownload: function (url) {
        try {
            // Create a temporary anchor element
            var link = document.createElement('a');
            link.href = url;
            link.style.display = 'none';

            // Add to body, click, and remove
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);

            console.log('Export triggered for URL:', url);
            return true;
        } catch (error) {
            console.error('Error triggering export:', error);
            return false;
        }
    }
};
