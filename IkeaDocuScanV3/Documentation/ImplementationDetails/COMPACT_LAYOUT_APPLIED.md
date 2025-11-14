# Compact Horizontal Layout - Applied

**Date:** 2025-01-24
**Status:** ✅ Complete
**Objective:** Set configurable max widths for dropdowns/inputs and use horizontal layout

---

## Summary of Changes

All field sections in DocumentPropertiesPage now use:
- **Horizontal layout** (label and control on same line)
- **Configurable maximum widths** for dropdowns, inputs, and date pickers
- **CSS variables** for easy width adjustments
- **Responsive design** that stacks vertically on small screens

---

## CSS Configuration (DocumentPropertiesPage.razor)

Added configurable CSS variables at `:root` level:

```css
:root {
    --dropdown-max-width: 20em;   /* Dropdowns/selects */
    --input-max-width: 20em;      /* Text inputs */
    --date-max-width: 12em;       /* Date pickers */
    --label-width: 10em;          /* Label width in horizontal layout */
}
```

**Important:** The `@media` query in the `<style>` block must be escaped as `@@media` to prevent Razor from parsing it as Razor code (line 335).

### How to Adjust Widths

To change the maximum widths, simply modify the CSS variables in DocumentPropertiesPage.razor:

```css
/* Example: Make dropdowns narrower */
:root {
    --dropdown-max-width: 15em;  /* Changed from 20em */
}

/* Example: Use pixels instead of em */
:root {
    --dropdown-max-width: 200px;
    --input-max-width: 250px;
    --date-max-width: 150px;
    --label-width: 120px;
}
```

---

## Layout Structure

### Horizontal Layout

**Before (Vertical):**
```razor
<Field>
    <FieldLabel>Document Type *</FieldLabel>
    <FieldBody>
        <Select TValue="int?" @bind-SelectedValue="@Model.DocumentTypeId">
            ...
        </Select>
    </FieldBody>
</Field>
```

**After (Horizontal):**
```razor
<Field Horizontal Class="field-horizontal">
    <FieldLabel>Document Type<span class="required">*</span></FieldLabel>
    <FieldBody>
        <Select TValue="int?" @bind-SelectedValue="@Model.DocumentTypeId" Class="compact-dropdown">
            ...
        </Select>
    </FieldBody>
</Field>
```

### Key Changes

1. **Added `Horizontal` attribute** to Field component
2. **Added `field-horizontal` CSS class** for custom styling
3. **Added `compact-dropdown`, `compact-input`, `compact-date` classes** to controls
4. **Shortened long labels** where appropriate (e.g., "Fwd. to Signatories Date")

---

## Files Modified

| File | Fields Updated | Layout |
|------|---------------|---------|
| **DocumentPropertiesPage.razor** | N/A | Added CSS configuration |
| **DocumentSectionFields.razor** | 7 fields | All horizontal |
| **ActionSectionFields.razor** | 2 fields | Date horizontal, memo vertical |
| **FlagsSectionFields.razor** | 4 fields | All horizontal |
| **AdditionalInfoFields.razor** | 10 fields | All horizontal |

**Total:** 23 fields converted to compact horizontal layout

---

## CSS Classes Applied

### Control Classes

| Class | Applied To | Max Width |
|-------|-----------|-----------|
| `compact-dropdown` | All `<Select>` dropdowns | 20em (configurable) |
| `compact-input` | All `<TextEdit>` inputs | 20em (configurable) |
| `compact-date` | All `<DocumentDatePicker>` | 12em (configurable) |

### Layout Classes

| Class | Applied To | Purpose |
|-------|-----------|----------|
| `field-horizontal` | `<Field>` components | Enables label + control on same line |

---

## Responsive Behavior

**Desktop (> 768px):**
- Label and control on same line
- Label right-aligned, fixed width (10em)
- Control has max-width as configured

**Mobile (≤ 768px):**
- Layout stacks vertically
- Label left-aligned, full width
- Control full width (max-width still applies)

---

## Examples of Shortened Labels

To fit better in the horizontal layout, some labels were abbreviated:

| Original Label | New Label |
|---------------|-----------|
| "Forwarded To Signatories Date" | "Fwd. to Signatories Date" |
| "Associated to PUA/Agreement No." | "Assoc. to PUA/Agr. No." |
| "Associated to Appendix No." | "Assoc. to Appendix No." |

**Note:** The full label text is still available in the component for accessibility.

---

## Field-by-Field Breakdown

### DocumentSectionFields (7 fields)

1. ✅ Document Type - Horizontal, compact dropdown
2. ✅ Counterparty No. - Horizontal, compact input
3. ✅ Counterparty - Horizontal, compact dropdown
4. ✅ Date of Contract - Horizontal, compact date
5. ✅ Receiving Date - Horizontal, compact date
6. ✅ Sending Out Date - Horizontal, compact date
7. ✅ Fwd. to Signatories Date - Horizontal, compact date
8. ✅ Dispatch Date - Horizontal, compact date

**Note:** Location, Affiliated To (read-only fields) and ThirdPartySelector remain unchanged.

### ActionSectionFields (2 fields)

1. ✅ Action Date - Horizontal, compact date
2. ⚠️ Action Description - **Vertical** (multi-line text area)
3. ✅ E-Mail Reminder Group - Horizontal, compact dropdown (hidden by default)

### FlagsSectionFields (4 fields)

1. ✅ Fax - Horizontal, tri-state dropdown
2. ✅ Original Received - Horizontal, tri-state dropdown
3. ✅ Translation Received - Horizontal, tri-state dropdown
4. ✅ Confidential - Horizontal, tri-state dropdown

### AdditionalInfoFields (10 fields)

1. ✅ Document Name - Horizontal, compact dropdown
2. ✅ Document No. - Horizontal, compact input
3. ✅ Version No. - Horizontal, compact input
4. ✅ Assoc. to PUA/Agr. No. - Horizontal, compact input
5. ✅ Assoc. to Appendix No. - Horizontal, compact input
6. ✅ Valid Until/As Of - Horizontal, compact date
7. ✅ Amount - Horizontal, numeric input (10em)
8. ✅ Currency - Horizontal, compact dropdown
9. ✅ Authorisation to - Horizontal, compact input
10. ✅ Bank Confirmation - Horizontal, tri-state dropdown

---

## Visual Improvements

### Before
```
Document Type *
[                    Long Dropdown                    ]

Counterparty No.
[              Text Input                 ]

Date of Contract *
[         Date Picker          ]
```

### After
```
Document Type*:        [Dropdown max 20em]
Counterparty No.:      [Input max 20em]
Date of Contract*:     [Date max 12em]
```

**Benefits:**
- ✅ More compact, less scrolling
- ✅ Easier to scan visually
- ✅ Consistent alignment
- ✅ Better use of screen space
- ✅ Matches desktop application style

---

## Testing Checklist

Test the layout on different screen sizes:

- [ ] **Desktop (1920x1080):** Labels and controls side-by-side, proper spacing
- [ ] **Laptop (1366x768):** Layout still horizontal, no overflow
- [ ] **Tablet (768px):** Layout switches to vertical stacking
- [ ] **Mobile (375px):** Full vertical layout, controls full width

Test control widths:

- [ ] **Dropdowns:** Max width 20em, not stretching beyond
- [ ] **Text inputs:** Max width 20em, not stretching beyond
- [ ] **Date pickers:** Max width 12em, not stretching beyond
- [ ] **Long dropdown options:** Text doesn't overflow or wrap awkwardly

Test responsiveness:

- [ ] **Resize browser:** Layout transitions smoothly at 768px breakpoint
- [ ] **Mobile view:** All fields accessible and usable

---

## Customization Guide

### Change Maximum Widths

Edit `DocumentPropertiesPage.razor`, find the `<style>` section:

```css
:root {
    --dropdown-max-width: 25em;  /* Increase dropdown width */
    --input-max-width: 30em;     /* Increase input width */
    --date-max-width: 15em;      /* Increase date picker width */
    --label-width: 12em;         /* Increase label width */
}
```

### Revert a Field to Vertical Layout

Remove `Horizontal` attribute and `field-horizontal` class:

```razor
<!-- Vertical layout -->
<Field>
    <FieldLabel>My Field</FieldLabel>
    <FieldBody>
        <TextEdit ... />
    </FieldBody>
</Field>
```

### Make a Specific Control Full Width

Override the max-width inline:

```razor
<TextEdit @bind-Text="@Model.SomeField" Style="max-width: 100%;" />
```

---

## Browser Compatibility

✅ Chrome/Edge - Fully supported
✅ Firefox - Fully supported
✅ Safari - Fully supported
✅ Mobile browsers - Fully supported

CSS features used:
- CSS Variables (`:root`, `var()`)
- Flexbox (`display: flex`)
- Media queries (`@media`)

All features are well-supported in modern browsers.

---

## Future Enhancements

Potential improvements:

1. **Make widths user-configurable** - Store in localStorage or user preferences
2. **Add density modes** - Compact/Normal/Spacious
3. **Add field tooltips** - Show full label text on hover for abbreviated labels
4. **Keyboard shortcuts** - Tab navigation optimization for horizontal layout

---

**Status:** Layout improvements complete ✅
**Performance:** No impact, pure CSS
**Backward Compatible:** Yes, can be reverted by removing CSS classes
**User Experience:** Significantly improved compactness and readability
