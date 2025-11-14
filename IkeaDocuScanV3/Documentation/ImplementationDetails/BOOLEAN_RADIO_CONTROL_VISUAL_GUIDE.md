# BooleanRadioControl - Visual Guide

This guide shows the visual appearance of the BooleanRadioControl component in different states.

---

## Component Layout

```
┌─────────────────────────────────────────────────┐
│ Label:  ○ True   ○ False                       │
└─────────────────────────────────────────────────┘
```

With clear button when value is selected:
```
┌─────────────────────────────────────────────────┐
│ Label:  ● True   ○ False   ✕                   │
└─────────────────────────────────────────────────┘
```

---

## State Examples

### 1. Not Selected (null)
```
Fax:  ○ True   ○ False
```
- Neither radio button is filled
- No clear button visible
- Default state when form loads

### 2. True Selected (with clear button)
```
Fax:  ● True   ○ False   ✕
```
- "True" radio button is filled (●)
- "True" label is bold and blue (#0d6efd)
- Clear button (✕) visible on right (if `ShowClearButton="true"`)
- Clicking ✕ returns to "Not Selected" state

### 3. False Selected (with clear button)
```
Fax:  ○ True   ● False   ✕
```
- "False" radio button is filled (●)
- "False" label is bold and blue (#0d6efd)
- Clear button (✕) visible on right (if `ShowClearButton="true"`)
- Clicking ✕ returns to "Not Selected" state

### 2b. True Selected (without clear button)
```
Fax:  ● True   ○ False
```
- Same as #2 but no clear button shown
- Used on Document Properties form
- User must click opposite radio to change

### 3b. False Selected (without clear button)
```
Fax:  ○ True   ● False
```
- Same as #3 but no clear button shown
- Used on Document Properties form
- User must click opposite radio to change

### 4. Disabled (Not Applicable)
```
Fax:  ○ True   ○ False  [grayed out, no clear button]
```
- Both radio buttons grayed out (opacity: 0.6)
- Labels grayed out
- No clear button
- Cursor shows "not-allowed"
- Cannot be clicked

---

## Document Properties Page Layout

Horizontal form layout with external labels (NO CLEAR BUTTONS):

```
┌───────────────────────────────────────────────────────────┐
│                     Fax   ○ True   ○ False                │
│                                                           │
│        Original Received   ○ True   ○ False               │
│                                                           │
│     Translation Received   ● True   ○ False               │
│                                                           │
│            Confidential*   ○ True   ○ False               │
│                                                           │
│       Bank Confirmation    ○ True   ● False               │
└───────────────────────────────────────────────────────────┘
```

Notes:
- Labels right-aligned in 3-column grid
- Required fields show red asterisk (*)
- Controls in 9-column grid
- **No clear buttons** - `ShowClearButton` defaults to `false`
- User must click opposite radio button to change selection
- Consistent spacing with other form fields

---

## Search Documents Page Layout

Compact inline layout with integrated labels (WITH CLEAR BUTTONS):

```
┌─────────────────────────────────────────────────────────────┐
│ Document Attributes                                         │
│ ┌──────────────┬──────────────┬──────────────┬────────────┐ │
│ │Fax:          │Original:     │Confidential: │Bank Conf:  │ │
│ │○ True        │● True   ✕    │○ True        │○ True      │ │
│ │○ False       │○ False       │● False   ✕   │○ False     │ │
│ └──────────────┴──────────────┴──────────────┴────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

Notes:
- 4 controls per row (col-md-3 each)
- Labels integrated in component (Label parameter)
- **Clear buttons enabled** - `ShowClearButton="true"` on all filters
- Compact font sizes (0.875rem)
- Reduced gaps (6-8px)
- Clear buttons appear inline to reset filter to "Any"
- Essential for filtering - allows users to remove filter criteria

---

## Color Reference

### Default State
- Radio button border: `#dee2e6` (Bootstrap gray-300)
- Label text: `#212529` (Bootstrap dark)
- Background: white

### Selected State
- Filled radio button: `#0d6efd` (Bootstrap primary blue)
- Selected label: `#0d6efd` bold (font-weight: 600)
- Background: white

### Hover State
- Radio button: slightly darker border
- Clear button: `#dc3545` (Bootstrap danger red)

### Disabled State
- Radio buttons: `#dee2e6` with opacity 0.6
- Labels: `#6c757d` with opacity 0.6
- Cursor: not-allowed
- Background: white (no gray background)

---

## Comparison with TriStateDropdown

### TriStateDropdown (OLD)
```
┌───────────────────────────────────┐
│ Fax                               │
│ ┌─────────────────────────────┐   │
│ │ Yes                        ▼│   │
│ └─────────────────────────────┘   │
└───────────────────────────────────┘

When clicked:
┌─────────────────────────────┐
│ -- Select --                │
│ Yes                         │
│ No                          │
└─────────────────────────────┘
```

### BooleanRadioControl (NEW)
```
┌───────────────────────────────────┐
│ Fax    ● True   ○ False   ✕       │
└───────────────────────────────────┘
```

**Advantages:**
- ✅ Immediate visibility of selection
- ✅ One-click selection (vs. two clicks)
- ✅ No dropdown overlay
- ✅ Clear button always visible
- ✅ More space-efficient horizontally
- ✅ Better for touch interfaces

---

## Responsive Behavior

### Desktop (≥992px)
```
┌──────────────┬──────────────┬──────────────┬──────────────┐
│Fax:          │Original:     │Confidential: │Bank Conf:    │
│○ True        │● True   ✕    │○ True        │○ True        │
│○ False       │○ False       │● False   ✕   │○ False       │
└──────────────┴──────────────┴──────────────┴──────────────┘
```
4 controls per row (col-md-3)

### Tablet (768-991px)
```
┌──────────────┬──────────────┐  ┌──────────────┬──────────────┐
│Fax:          │Original:     │  │Confidential: │Bank Conf:    │
│○ True        │● True   ✕    │  │○ True        │○ True        │
│○ False       │○ False       │  │● False   ✕   │○ False       │
└──────────────┴──────────────┘  └──────────────┴──────────────┘
```
2 controls per row, wraps to 2 rows

### Mobile (<768px)
```
┌─────────────────────────┐
│Fax:                     │
│○ True   ○ False         │
└─────────────────────────┘
┌─────────────────────────┐
│Original:                │
│● True   ○ False   ✕     │
└─────────────────────────┘
┌─────────────────────────┐
│Confidential:            │
│○ True   ● False   ✕     │
└─────────────────────────┘
┌─────────────────────────┐
│Bank Conf:               │
│○ True   ○ False         │
└─────────────────────────┘
```
1 control per row, stacks vertically

---

## Accessibility Features

### Screen Reader Announcement

**Not Selected:**
```
"Fax, radio group, nothing selected"
```

**True Selected:**
```
"Fax, radio group, True, checked, 1 of 2"
```

**False Selected:**
```
"Fax, radio group, False, checked, 2 of 2"
```

**Disabled:**
```
"Fax, radio group, disabled"
```

### Keyboard Navigation

1. **Tab:** Focus next control
2. **Shift+Tab:** Focus previous control
3. **Arrow Keys:** Move between True/False when focused
4. **Space/Enter:** Select focused option
5. **Tab to Clear Button:** Focus clear button (if value selected)
6. **Enter on Clear:** Clear selection

---

## CSS Classes

The component uses the following CSS classes:

### Main Container
- `.boolean-radio-control` - Flex container with gap

### Label
- `.form-label-radio` - Label styling with min-width

### Radio Group
- `.radio-button-group` - Container for radios + clear button
- `.form-check-inline` - Bootstrap class for inline radio
- `.form-check-input` - Bootstrap class for radio input
- `.form-check-label` - Bootstrap class for radio label

### Clear Button
- `.btn-clear` - Clear button styling

### State Modifiers
- `:checked` - Radio is selected
- `:disabled` - Control is disabled
- `:hover` - Mouse over state

---

## Browser Rendering

The component renders consistently across modern browsers:

### Chrome/Edge
```
Fax:  ⚪ True   ⚫ False   ✕
```
Circular radio buttons, smooth animations

### Firefox
```
Fax:  ○ True   ● False   ✕
```
Circular radio buttons, slightly different fill style

### Safari
```
Fax:  ◯ True   ◉ False   ✕
```
Circular radio buttons, macOS-style rendering

All browsers show consistent spacing, colors, and functionality.

---

## Print Styling (Future Enhancement)

When printing, the component could show:
```
Fax: [X] True  [ ] False
```

Or simply:
```
Fax: True
```

This would require adding `@media print` CSS rules.

---

## Dark Mode Support (Future Enhancement)

Dark mode colors could be:
- Background: `#212529`
- Text: `#e9ecef`
- Radio border: `#495057`
- Selected radio: `#0d6efd` (same)
- Clear button hover: `#dc3545` (same)

Would require CSS variables and dark mode detection.
