/**
 * Excel Download Helper
 * Provides functions for downloading Excel files from byte arrays
 */

/**
 * Downloads a file from a byte array
 * @param {string} fileName - The name of the file to download
 * @param {Uint8Array} byteArray - The file content as byte array
 */
window.downloadFileFromBytes = function (fileName, byteArray) {
    try {
        // Create blob from byte array
        const blob = new Blob([byteArray], {
            type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
        });

        // Create download link
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = fileName;

        // Trigger download
        document.body.appendChild(link);
        link.click();

        // Cleanup
        document.body.removeChild(link);
        window.URL.revokeObjectURL(url);

        console.log(`Excel file '${fileName}' download initiated`);
    } catch (error) {
        console.error('Error downloading Excel file:', error);
        alert('Failed to download Excel file: ' + error.message);
    }
};

/**
 * Downloads a file from a base64 string (alternative method)
 * @param {string} fileName - The name of the file to download
 * @param {string} base64String - The file content as base64 string
 */
window.downloadFileFromBase64 = function (fileName, base64String) {
    try {
        // Convert base64 to byte array
        const byteCharacters = atob(base64String);
        const byteNumbers = new Array(byteCharacters.length);

        for (let i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.charCodeAt(i);
        }

        const byteArray = new Uint8Array(byteNumbers);

        // Use the byte array download function
        window.downloadFileFromBytes(fileName, byteArray);
    } catch (error) {
        console.error('Error downloading Excel file from base64:', error);
        alert('Failed to download Excel file: ' + error.message);
    }
};

console.log('Excel download helper loaded');
