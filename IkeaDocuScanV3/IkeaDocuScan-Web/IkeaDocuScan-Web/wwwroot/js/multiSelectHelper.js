/**
 * Multi-select helper functions for Blazor
 * Provides utilities for working with HTML multi-select elements
 */

/**
 * Gets the selected option values from a multi-select element
 * @param {HTMLSelectElement} selectElement - The select element reference
 * @returns {number[]} Array of selected option values as integers
 */
window.getSelectedOptions = function (selectElement) {
    if (!selectElement) {
        console.warn('getSelectedOptions: No select element provided');
        return [];
    }

    // Get all selected options
    const selectedOptions = Array.from(selectElement.selectedOptions);

    // Extract and parse the values as integers
    const values = selectedOptions.map(option => parseInt(option.value, 10));

    console.log(`getSelectedOptions: Found ${values.length} selected items`, values);

    return values;
};

console.log('Multi-select helper loaded');
