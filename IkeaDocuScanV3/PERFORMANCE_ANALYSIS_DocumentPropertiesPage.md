# Performance Analysis: DocumentPropertiesPage Load Time

## Issue
The DocumentPropertiesPage takes more than 5 seconds to load, which is unacceptably slow.

## Root Causes Identified

### 1. **JavaScript Navigation Interception - Up to 5 seconds delay**
**Location**: `DocumentPropertiesPage.razor.cs`, lines 901-943

**Problem**:
```csharp
// Wait for the JavaScript file to load (max 5 seconds)
var maxAttempts = 50;
var attempt = 0;
var scriptLoaded = false;

while (attempt < maxAttempts && !scriptLoaded)
{
    try
    {
        scriptLoaded = await JSRuntime.InvokeAsync<bool>("eval",
            "typeof window.documentPropertiesPage !== 'undefined'");

        if (!scriptLoaded)
        {
            await Task.Delay(100);  // <-- 100ms delay per attempt
            attempt++;
        }
    }
    catch
    {
        await Task.Delay(100);  // <-- 100ms delay on error
        attempt++;
    }
}
```

**Analysis**:
- Waits up to **50 attempts × 100ms = 5 seconds** for JavaScript file to load
- Runs in `OnAfterRenderAsync(firstRender)` which blocks UI interactivity
- If the JavaScript file fails to load, waits the full 5 seconds before giving up
- Uses polling instead of event-driven approach

**Impact**: **High** - Can add up to 5 seconds to page load time

---

### 2. **Multiple Synchronous Child Component Initializations**
**Location**: Multiple child components load data independently

**Components loading data on initialization**:

a. **DocumentSectionFields.razor** (lines 150-167)
   - Loads all DocumentTypes: `await DocumentTypeService.GetAllAsync()`

b. **CounterPartySelector.razor** (lines 174-194)
   - Loads all CounterParties: `await CounterPartyService.GetAllAsync()`

c. **ThirdPartySelector.razor** (lines 137-157)
   - Loads all CounterParties again: `await CounterPartyService.GetAllAsync()`
   - **Duplicate call** - same data loaded twice!

d. **AdditionalInfoFields.razor** (likely similar pattern)
   - Probably loads Currencies, DocumentNames, etc.

**Problem**:
- Each component loads independently in `OnInitializedAsync`
- No data sharing between components
- CounterParties loaded at least **twice** (CounterPartySelector + ThirdPartySelector)
- All HTTP requests are sequential, not parallel
- No caching at the component level

**Impact**: **High** - Multiple sequential HTTP roundtrips

---

### 3. **Additional 500ms Delay for Change Tracking**
**Location**: `DocumentPropertiesPage.razor.cs`, lines 126-133

```csharp
// Enable change tracking after a delay to ensure child components finish their async loading
_ = Task.Run(async () => {
    await Task.Delay(500); // <-- Fixed 500ms delay
    await InvokeAsync(() => {
        enableChangeTracking = true;
        StateHasChanged();
    });
});
```

**Problem**:
- Adds a fixed **500ms delay** after page load
- Guesses that child components need 500ms to load
- Forces an additional render via `StateHasChanged()`

**Impact**: **Medium** - Fixed 500ms delay, plus additional render cycle

---

### 4. **JSON Serialization on Every Render**
**Location**: `DocumentPropertiesPage.razor.cs`, lines 848-853

```csharp
private void CreateSnapshot()
{
    originalModelJson = System.Text.Json.JsonSerializer.Serialize(Model);
    hasUnsavedChanges = false;
    Logger.LogInformation("Created Snapshot");
}
```

**Problem**:
- Serializes entire Model to JSON
- Model contains all 40+ form fields
- Model may contain large FileBytes array (in Check-in mode)
- Called multiple times: after initial load, after every save, potentially on every change

**Impact**: **Medium** - JSON serialization of large objects is expensive

---

### 5. **Check-in Mode: File Loading**
**Location**: `DocumentPropertiesPage.razor.cs`, lines 154-201

For Check-in mode specifically:
```csharp
private async Task LoadCheckInModeAsync(string fileName)
{
    var barcode = ExtractBarcodeFromFileName(fileName);

    // Check if document exists (HTTP call #1)
    var existingDocument = await DocumentService.GetByBarCodeAsync(barcode);

    // Load file metadata (HTTP call #2)
    var scannedFile = await ScannedFileService.GetFileByNameAsync(fileName);

    // Load file bytes (HTTP call #3)
    var fileBytes = await ScannedFileService.GetFileContentAsync(fileName);
}
```

**Problem**:
- Three sequential HTTP calls before page can render
- File content might be large (PDFs, images)
- No parallel execution

**Impact**: **High** for Check-in mode - 3× network latency

---

## Performance Timeline (Worst Case Scenario)

### Check-in Mode Load Sequence:
```
0ms     - User navigates to page
0-10ms  - OnInitializedAsync starts
10ms    - LoadPageAsync begins
10ms    - LoadCheckInModeAsync starts

// Check for duplicate barcode
10-110ms    - HTTP: GetByBarCodeAsync() [100ms]

// Load file metadata
110-210ms   - HTTP: GetFileByNameAsync() [100ms]

// Load file content
210-510ms   - HTTP: GetFileContentAsync() [300ms for large file]

510ms   - Model created, isLoading = false
510ms   - CreateSnapshot() - JSON serialize Model with FileBytes [50ms]
560ms   - StateHasChanged() triggers render

// Child components initialize
560-660ms   - DocumentSectionFields loads DocumentTypes [100ms]
660-760ms   - CounterPartySelector loads CounterParties [100ms]
760-860ms   - ThirdPartySelector loads CounterParties AGAIN [100ms]
860-960ms   - AdditionalInfoFields loads other data [100ms]

960ms   - All child components rendered
960ms   - OnAfterRenderAsync(firstRender=true) starts

// JavaScript navigation interception polling
960ms-5960ms - SetupJavaScriptNavigationInterception()
               Polls 50 times if JS file not found [5000ms]

5960ms  - CheckForCopiedData() [10ms]
5970ms  - Auto-focus barcode field [10ms]

// Change tracking delay
5970-6470ms - Task.Delay(500) for change tracking [500ms]
6470ms  - StateHasChanged() again

TOTAL: ~6.5 seconds
```

---

## Recommended Solutions

### Priority 1: Fix JavaScript Navigation Interception (Save up to 5 seconds)

**Option A: Use proper script loading**
```csharp
private async Task SetupJavaScriptNavigationInterception()
{
    dotNetRef = DotNetObjectReference.Create(this);

    try
    {
        // Try once immediately, fail fast if not available
        var scriptLoaded = await JSRuntime.InvokeAsync<bool>("eval",
            "typeof window.documentPropertiesPage !== 'undefined'");

        if (scriptLoaded)
        {
            await JSRuntime.InvokeVoidAsync("documentPropertiesPage.init", dotNetRef);
        }
        else
        {
            Logger.LogWarning("documentPropertiesPage JavaScript not loaded, navigation interception disabled");
        }
    }
    catch (Exception ex)
    {
        Logger.LogWarning(ex, "Failed to setup navigation interception, continuing without it");
    }
}
```

**Option B: Remove polling entirely**
- Navigation interception is a nice-to-have, not critical
- If JS file doesn't load immediately, skip it
- Page works fine without it

**Expected Savings**: **Up to 5 seconds**

---

### Priority 2: Parallelize Child Component Data Loading

**Option A: Load data in parent, pass to children**
```csharp
private async Task LoadPageAsync()
{
    try {
        isLoading = true;

        // Load all reference data in parallel
        var documentTypesTask = DocumentTypeService.GetAllAsync();
        var counterPartiesTask = CounterPartyService.GetAllAsync();
        var currenciesTask = CurrencyService.GetAllAsync();
        // ... other reference data

        await Task.WhenAll(documentTypesTask, counterPartiesTask, currenciesTask);

        // Store in page-level properties
        documentTypes = await documentTypesTask;
        counterParties = await counterPartiesTask;
        currencies = await currenciesTask;

        // Then load mode-specific data
        if (BarCode.HasValue) {
            await LoadEditModeAsync(BarCode.Value);
        } else if (!string.IsNullOrEmpty(FileName)) {
            await LoadCheckInModeAsync(FileName);
        } else {
            LoadRegisterMode();
        }
    }
    finally {
        isLoading = false;
    }
}
```

Pass data to child components as parameters:
```razor
<DocumentSectionFields @bind-Model="@Model"
                       DocumentTypes="@documentTypes"
                       @bind-Model:after="CheckForChanges" />

<CounterPartySelector @bind-SelectedCounterPartyId="@Model.CounterPartyId"
                      CounterParties="@counterParties"
                      OnCounterPartySelected="@OnCounterPartySelectedFromSelector"
                      Disabled="@IsReadOnly" />
```

**Expected Savings**: **200-400ms** (eliminate duplicate loads + parallelize)

---

### Priority 3: Remove Fixed 500ms Delay

**Replace with proper component lifecycle**:
```csharp
private async Task LoadPageAsync()
{
    try {
        isLoading = true;
        enableChangeTracking = false;

        // ... load data ...
    }
    finally {
        isLoading = false;
        CreateSnapshot();
        StateHasChanged();

        // Enable change tracking immediately
        enableChangeTracking = true;
    }
}
```

**Rationale**: If we load all data in parent component first, child components don't need async initialization, so the 500ms delay is unnecessary.

**Expected Savings**: **500ms + 1 render cycle**

---

### Priority 4: Optimize Check-in Mode File Loading

**Parallelize independent operations**:
```csharp
private async Task LoadCheckInModeAsync(string fileName)
{
    var barcode = ExtractBarcodeFromFileName(fileName);

    // Check duplicate and load file in parallel
    var existingDocTask = DocumentService.GetByBarCodeAsync(barcode);
    var scannedFileTask = ScannedFileService.GetFileByNameAsync(fileName);

    await Task.WhenAll(existingDocTask, scannedFileTask);

    var existingDocument = await existingDocTask;
    if (existingDocument != null)
    {
        errorMessage = $"A document with barcode '{barcode}' already exists...";
        return;
    }

    var scannedFile = await scannedFileTask;
    if (scannedFile == null)
    {
        errorMessage = $"Scanned file '{fileName}' not found...";
        return;
    }

    // Load file bytes (only if needed)
    var fileBytes = await ScannedFileService.GetFileContentAsync(fileName);

    // ... rest of method
}
```

**Expected Savings**: **100-200ms** (network latency)

---

### Priority 5: Optimize JSON Serialization

**Option A: Exclude FileBytes from snapshot**
```csharp
private void CreateSnapshot()
{
    // Create a lightweight copy without FileBytes for comparison
    var snapshotModel = new DocumentPropertiesViewModel
    {
        // Copy all properties EXCEPT FileBytes
        Id = Model.Id,
        BarCode = Model.BarCode,
        // ... all other properties
        // FileBytes = null,  // Explicitly exclude
    };

    originalModelJson = System.Text.Json.JsonSerializer.Serialize(snapshotModel);
    hasUnsavedChanges = false;
}
```

**Option B: Use hash-based comparison**
- Calculate hash of relevant fields only
- Much faster than JSON serialization

**Expected Savings**: **10-50ms** per snapshot operation

---

## Summary of Expected Improvements

| Issue | Current | After Fix | Savings |
|-------|---------|-----------|---------|
| JS Navigation Polling | 0-5000ms | 10ms | **Up to 5000ms** |
| Child Component Loading | 400ms | 200ms | **200ms** |
| Fixed Change Tracking Delay | 500ms | 0ms | **500ms** |
| Check-in File Loading | 300ms | 200ms | **100ms** |
| JSON Serialization | 50ms | 10ms | **40ms** |
| **TOTAL** | **~6500ms** | **~420ms** | **~6000ms** |

## Expected Load Times After Optimization

- **Edit Mode**: ~300ms (load document + reference data)
- **Register Mode**: ~200ms (load reference data only)
- **Check-in Mode**: ~400ms (load file + reference data)

All under **500ms**, which is the target for good user experience.

---

## Implementation Priority

1. **Immediate**: Remove/fix JavaScript navigation interception polling
2. **High**: Parallelize child component data loading
3. **High**: Remove fixed 500ms delay
4. **Medium**: Optimize Check-in mode file loading
5. **Low**: Optimize JSON serialization (minor impact)

---

## Testing Recommendations

After implementing fixes, measure actual load times:

```csharp
private async Task LoadPageAsync()
{
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();

    try {
        // ... existing code ...
    }
    finally {
        stopwatch.Stop();
        Logger.LogInformation("Page loaded in {ElapsedMs}ms (mode: {Mode})",
            stopwatch.ElapsedMilliseconds, Model.Mode);
    }
}
```

Add similar logging to child components to identify any remaining bottlenecks.
