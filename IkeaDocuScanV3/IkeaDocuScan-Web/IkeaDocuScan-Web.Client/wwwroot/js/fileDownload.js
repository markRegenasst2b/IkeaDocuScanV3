/**
 * File Download Helper
 * Provides functions for downloading files from URLs
 */

/**
 * Downloads a file from a URL
 * @param {string} url - The URL to download from
 * @param {string} fileName - The name of the file to download
 */
window.downloadFile = function (url, fileName) {
    try {
        // Create invisible download link
        const link = document.createElement('a');
        link.href = url;
        link.download = fileName;
        link.style.display = 'none';

        // Append to body, trigger click, then remove
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);

        console.log(`File download initiated: ${fileName} from ${url}`);
    } catch (error) {
        console.error('Error downloading file:', error);
        alert('Failed to download file: ' + error.message);
    }
};

console.log('File download helper loaded');
