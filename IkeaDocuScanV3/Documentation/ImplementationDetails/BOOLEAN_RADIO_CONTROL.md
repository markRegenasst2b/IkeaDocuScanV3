# BooleanRadioControl Component

## Overview

`BooleanRadioControl.razor` is a reusable Blazor component that represents a nullable Boolean value using two inline radio buttons (True/False) instead of a dropdown selector. It provides a more visually intuitive way to select binary options.

## Location

```
IkeaDocuScan-Web/IkeaDocuScan-Web.Client/Components/Shared/BooleanRadioControl.razor
```

## Features

- **Tri-state support**: null (not selected), true, or false
- **Visual clarity**: Two inline radio buttons labeled "True" and "False"
- **Clear button**: Shows a clear (×) button when a value is selected (not disabled)
- **Disabled state**: Both radio buttons become disabled and grayed out
- **Bootstrap 5 styling**: Consistent with existing form controls
- **Two-way binding**: Supports `@bind-Value` syntax
- **Field visibility integration**: Works with `IsFieldMandatory()` and `IsFieldDisabled()` from DocumentPropertiesViewModel

## Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `Value` | `bool?` | No | `null` | The bound nullable boolean value |
| `ValueChanged` | `EventCallback<bool?>` | No | - | Event callback when value changes |
| `Disabled` | `bool` | No | `false` | Whether the control is disabled |
| `Label` | `string?` | No | `null` | Optional label text displayed before radio buttons |
| `ShowClearButton` | `bool` | No | `false` | Whether to show the clear (×) button when a value is selected |

## Value States

| State | Description | Visual Representation |
|-------|-------------|----------------------|
| `null` | Not selected (initial state) | Neither radio button checked, no clear button |
| `true` | True selected | "True" radio button checked, label in blue bold, clear button visible (if enabled) |
| `false` | False selected | "False" radio button checked, label in blue bold, clear button visible (if enabled) |
| Disabled | Control is disabled | Both radio buttons grayed out, no clear button |

## Basic Usage

### Simple usage with two-way binding

```razor
<BooleanRadioControl @bind-Value="@Model.Fax" />
```

### With label

```razor
<BooleanRadioControl Label="Is Active?" @bind-Value="@IsActive" />
```

### With disabled state

```razor
<BooleanRadioControl Label="Confidential?"
                    @bind-Value="@Model.Confidential"
                    Disabled="@IsReadOnly" />
```

### With value change callback

```razor
<BooleanRadioControl @bind-Value="@Model.Fax"
                    @bind-Value:after="OnFaxChanged" />

@code {
    private async Task OnFaxChanged()
    {
        // Handle value change
        await ModelChanged.InvokeAsync(Model);
    }
}
```

### With clear button (for filters)

```razor
<BooleanRadioControl Label="Fax:"
                    @bind-Value="@searchRequest.Fax"
                    ShowClearButton="true" />
```

### Without clear button (for form fields)

```razor
<BooleanRadioControl @bind-Value="@Model.Confidential"
                    Disabled="@(IsReadOnly || Model.IsFieldDisabled("Confidential"))" />
```

## Integration with DocumentPropertiesViewModel

The component integrates seamlessly with field visibility configuration:

```razor
<div class="row mb-3 field-horizontal">
    <label class="col-sm-3 col-form-label text-end">
        Fax
        @if (Model.IsFieldMandatory("Fax"))
        {
            <span class="required">*</span>
        }
    </label>
    <div class="col-sm-9">
        <BooleanRadioControl @bind-Value="@Model.Fax"
                            @bind-Value:after="OnFieldChanged"
                            Disabled="@(IsReadOnly || Model.IsFieldDisabled("Fax"))" />
    </div>
</div>
```

## Fields Using BooleanRadioControl

The following boolean fields have been updated to use `BooleanRadioControl`:

### Document Properties Form (DocumentPropertiesPage)

#### FlagsSectionFields.razor
- **Fax** - Whether document was received via fax
- **OriginalReceived** - Whether original document was received
- **TranslationReceived** - Whether translated version was received
- **Confidential** - Whether document is confidential

#### AdditionalInfoFields.razor
- **BankConfirmation** - Whether bank confirmation was received

### Search Documents Page (SearchDocuments.razor)

The Document Attributes filter section uses `BooleanRadioControl` with `ShowClearButton="true"` for:
- **Fax** - Filter by fax received status
- **Original** - Filter by original document received status
- **Confidential** - Filter by confidential status
- **Bank Conf** - Filter by bank confirmation status

**Clear Button Enabled:** All search filters show a clear (×) button to reset the filter to "Any" (null).

Note: On the search page, custom CSS adjustments make the controls more compact to fit the filter layout.

## Visual Design

- **Layout**: Horizontal inline layout with optional label
- **Radio buttons**: Standard Bootstrap 5 form-check-input styling
- **Selected state**: Bold blue text (#0d6efd) on label
- **Disabled state**: Reduced opacity (0.6) and cursor: not-allowed
- **Clear button**: Small link-style button with × icon, appears only when value is set and control is not disabled
- **Spacing**: Consistent 8-12px gaps between elements

## Comparison with TriStateDropdown

| Feature | TriStateDropdown | BooleanRadioControl |
|---------|------------------|---------------------|
| Control Type | Dropdown/Select | Radio Buttons |
| Visual Feedback | Selected text in dropdown | Checked radio button with bold blue label |
| Selection Method | Click to open dropdown, then select | Direct click on radio button |
| Clear Method | Select "-- Select --" option | Click × button (if `ShowClearButton="true"`) |
| Screen Space | Vertical (dropdown height) | Horizontal (inline) |
| User Experience | 2 clicks (open + select) | 1 click (direct select) |
| Clear Button | Always visible as dropdown option | Optional, controlled by `ShowClearButton` parameter |

## Accessibility

- Uses semantic HTML radio input elements
- Unique IDs generated per instance to avoid conflicts
- Labels properly associated with inputs via `for` attribute
- Disabled state properly communicated via `disabled` attribute
- Keyboard navigation supported (Tab, Arrow keys, Space/Enter to select)

## CSS Styling

Key CSS classes:
- `.boolean-radio-control` - Main container with flex layout
- `.form-label-radio` - Label styling (min-width: 120px)
- `.radio-button-group` - Radio buttons + clear button container
- `.form-check-inline` - Bootstrap class for inline radio buttons
- `.btn-clear` - Clear button styling (appears on hover, turns red)

## Browser Compatibility

Compatible with all modern browsers:
- Chrome/Edge 90+
- Firefox 88+
- Safari 14+
- Opera 76+

## Performance

- Lightweight component with minimal JavaScript
- No external dependencies
- Static instance counter for unique ID generation
- Immediate UI updates via Blazor's reactive rendering

## Page-Specific Styling

### Search Documents Page

The SearchDocuments.razor page includes custom CSS to make the controls more compact:

```css
/* BooleanRadioControl adjustments for search page */
.filter-section .boolean-radio-control {
    gap: 8px;
}

.filter-section .boolean-radio-control .form-label-radio {
    font-weight: 500;
    font-size: 0.875rem;
    min-width: 90px;
    white-space: nowrap;
}

.filter-section .boolean-radio-control .radio-button-group {
    gap: 6px;
}

.filter-section .boolean-radio-control .form-check-inline {
    font-size: 0.875rem;
}

.filter-section .boolean-radio-control .btn-clear {
    font-size: 0.95rem;
}
```

This ensures the radio controls fit well within the compact filter layout.

## Migration from TriStateDropdown

To migrate an existing field from `TriStateDropdown` to `BooleanRadioControl`:

### Document Properties Form:
1. Replace the component tag:
   ```razor
   <!-- OLD -->
   <TriStateDropdown @bind-Value="@Model.Fax"
                    Disabled="@IsReadOnly"
                    Label="Fax" />

   <!-- NEW -->
   <BooleanRadioControl @bind-Value="@Model.Fax"
                       Disabled="@IsReadOnly" />
   ```

2. Remove the `Label` parameter if using external label (as in horizontal form layouts)
3. No changes needed to the model or binding logic
4. Visual appearance will change but functionality remains identical

### Search Page:
1. Replace the dropdown select:
   ```razor
   <!-- OLD -->
   <select class="form-select form-select-sm"
           value="@(searchRequest.Fax?.ToString() ?? "")"
           @onchange="@(e => OnFaxChanged(e.Value?.ToString()))">
       <option value="">Any</option>
       <option value="True">Yes</option>
       <option value="False">No</option>
   </select>

   <!-- NEW -->
   <BooleanRadioControl Label="Fax:"
                       @bind-Value="@searchRequest.Fax"
                       ShowClearButton="true" />
   ```

2. Remove the change handler method (e.g., `OnFaxChanged`) from the code-behind
3. Two-way binding handles updates automatically
4. **Important:** Set `ShowClearButton="true"` for filters to allow clearing the selection

## Known Limitations

1. The clear button uses Bootstrap Icons (`bi-x-circle`), which must be included in the project
2. Radio buttons are always horizontal - no vertical layout option
3. Custom labels for True/False are not supported (always displays "True" and "False")
4. Minimum recommended width: 200px for proper layout

## Future Enhancements

Potential future improvements:
- Custom labels for True/False options (e.g., "Yes"/"No", "On"/"Off")
- Vertical layout option
- Custom styling themes
- Confirmation dialog on clear action for critical fields
- Integration with form validation messages
