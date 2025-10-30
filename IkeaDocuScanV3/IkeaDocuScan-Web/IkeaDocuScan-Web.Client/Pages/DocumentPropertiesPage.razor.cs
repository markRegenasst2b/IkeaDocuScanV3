using IkeaDocuScan.Shared.DTOs.Documents;
using IkeaDocuScan.Shared.Enums;
using IkeaDocuScan.Shared.Interfaces;
using IkeaDocuScan_Web.Client.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;

namespace IkeaDocuScan_Web.Client.Pages;

/// <summary>
/// Code-behind for DocumentPropertiesPage component.
/// Handles three operational modes: Edit, Register, Check-in
/// </summary>
public partial class DocumentPropertiesPage : ComponentBase, IDisposable
{
    // ========================================
    // PARAMETERS (from route)
    // ========================================

    [Parameter]
    public int? BarCode { get; set; }

    [Parameter]
    public string? FileName { get; set; }

    // ========================================
    // STATE
    // ========================================

    private DocumentPropertiesViewModel Model { get; set; } = new();
    private bool isLoading = true;
    private bool isLoadingChildren = false; // True while waiting for child components to load
    private bool isSaving = false;
    private string? errorMessage;
    private string? successMessage;
    private string? warningMessage;
    private List<string> validationErrors = new();

    // Copy/Paste state
    private bool hasCopiedData = false;
    private DateTime? copiedDataExpiration;

    // Duplicate detection
    private bool showDuplicateModal = false;
    private List<int> similarDocumentBarcodes = new();
    private bool duplicateConfirmed = false;

    // Change tracking
    private string? originalModelJson;
    private bool hasUnsavedChanges = false;
    private bool isCheckingForChanges = false; // Prevent recursive calls
    private bool enableChangeTracking = false; // Only enable after initial load completes
    private int? lastDocumentTypeId = null; // Track DocumentType changes to apply field configuration

    // Reference data loaded once and shared with all child components
    private List<IkeaDocuScan.Shared.DTOs.DocumentTypes.DocumentTypeDto> documentTypes = new();
    private List<IkeaDocuScan.Shared.DTOs.CounterParties.CounterPartyDto> counterParties = new();
    private List<IkeaDocuScan.Shared.DTOs.Currencies.CurrencyDto> currencies = new();
    private List<IkeaDocuScan.Shared.DTOs.DocumentNames.DocumentNameDto> documentNames = new();

    // Child component load tracking
    private const int TotalChildComponents = 6; // DocumentSection, CounterParty, ThirdParty, Action, Flags, AdditionalInfo
    private int loadedChildComponentCount = 0;
    private bool acceptChildCallbacks = false; // Only accept callbacks after parent finishes loading
    private readonly object childLoadLock = new();
    private bool isFirstLoad = true; // Track if this is the first load or a re-navigation

    // ========================================
    // LIFECYCLE METHODS
    // ========================================

    protected override async Task OnInitializedAsync()
    {
        await LoadPageAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (isLoading)
            return; // First load handled by OnInitializedAsync

        // When navigating back to the same route, components are reused
        // Reset child component counter to allow re-initialization
        lock (childLoadLock)
        {
            loadedChildComponentCount = 0;
            acceptChildCallbacks = false;
        }

        await LoadPageAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Setup custom navigation interception via JavaScript
            await SetupJavaScriptNavigationInterception();

            // Check if copied data exists in localStorage
            await CheckForCopiedData();

            // Auto-focus barcode field in Register mode
            if (Model.Mode == DocumentPropertyMode.Register)
            {
                await JSRuntime.InvokeVoidAsync("eval",
                    "setTimeout(() => document.getElementById('barcodeInput')?.focus(), 100)");
            }
        }
    }

    // ========================================
    // LOAD DATA
    // ========================================

    private async Task LoadPageAsync()
    {
        var totalStopwatch = System.Diagnostics.Stopwatch.StartNew();
        var referenceDataStopwatch = System.Diagnostics.Stopwatch.StartNew();

        try {
            isLoading = true;
            enableChangeTracking = false; // Disable during load

            // Stop accepting child callbacks during page load and reset counter
            lock (childLoadLock)
            {
                acceptChildCallbacks = false;
                loadedChildComponentCount = 0;
            }

            errorMessage = null;
            successMessage = null;
            validationErrors.Clear();

            // PERFORMANCE OPTIMIZATION: Load all reference data in parallel FIRST
            // This data will be passed to child components, eliminating duplicate loads
            Logger.LogInformation("⏱️ PERF: Loading reference data in parallel...");
            var loadReferenceDataTask = Task.WhenAll(
                LoadDocumentTypesAsync(),
                LoadCounterPartiesAsync(),
                LoadCurrenciesAsync()
                // Note: DocumentNames depends on DocumentTypeId, so loaded per-document in child component
            );

            // Load page-specific data in parallel with reference data
            var pageDataStopwatch = System.Diagnostics.Stopwatch.StartNew();
            Task pageDataTask;
            if (BarCode.HasValue) {
                // EDIT MODE
                Logger.LogInformation("⏱️ PERF: Load document in Edit Mode");
                pageDataTask = LoadEditModeAsync(BarCode.Value);
            } else if (!string.IsNullOrEmpty(FileName)) {
                // CHECK-IN MODE
                Logger.LogInformation("⏱️ PERF: Load document in Check-In Mode");
                pageDataTask = LoadCheckInModeAsync(FileName);
            } else  {
                // REGISTER MODE
                Logger.LogInformation("⏱️ PERF: Open document in register mode");
                LoadRegisterMode();
                pageDataTask = Task.CompletedTask;
            }

            // Wait for both reference data and page data to complete
            await Task.WhenAll(loadReferenceDataTask, pageDataTask);

            referenceDataStopwatch.Stop();
            pageDataStopwatch.Stop();

            Logger.LogInformation("⏱️ PERF: Reference data loaded in {ReferenceMs}ms", referenceDataStopwatch.ElapsedMilliseconds);
            Logger.LogInformation("⏱️ PERF: Page data loaded in {PageMs}ms", pageDataStopwatch.ElapsedMilliseconds);
            Logger.LogInformation("⏱️ PERF: Parent data loading complete in {TotalMs}ms (parallel execution)", Math.Max(referenceDataStopwatch.ElapsedMilliseconds, pageDataStopwatch.ElapsedMilliseconds));
        } catch (Exception ex) {
            Logger.LogInformation($"Failed to load page: {ex.Message}");
            errorMessage = $"Failed to load page: {ex.Message}";

            // On error, turn off loading immediately
            isLoading = false;
            isLoadingChildren = false;
        } finally {
            totalStopwatch.Stop();
            Logger.LogInformation("⏱️ PERF: Parent LoadPageAsync complete in {TotalMs}ms", totalStopwatch.ElapsedMilliseconds);

            // Start accepting child callbacks now that parent data is loaded
            lock (childLoadLock)
            {
                acceptChildCallbacks = true;
                Logger.LogInformation("⏱️ PERF: Now accepting child callbacks. Waiting for {Total} child components to complete.", TotalChildComponents);
            }

            // Parent data loaded - children will now mount and call their callbacks
            isLoading = false;

            // On re-navigation, components are already mounted and won't call OnInitializedAsync again
            // So we skip waiting for callbacks and immediately enable the form
            if (!isFirstLoad)
            {
                Logger.LogInformation("⏱️ PERF: Re-navigation detected. Skipping child callback wait. Components already initialized.");
                isLoadingChildren = false;
                // Preserve hasUnsavedChanges if it was set to true (e.g., check-in mode with pre-registered document)
                CreateSnapshot(preserveUnsavedChanges: hasUnsavedChanges);
                enableChangeTracking = true;
                StateHasChanged();
            }
            else
            {
                // First load - wait for child components to initialize
                isLoadingChildren = true; // Keep overlay visible while children load (first load only)
                isFirstLoad = false; // Mark that we've completed first load

                Logger.LogInformation("⏱️ PERF: About to render children. Mode={Mode}, isLoading={IsLoading}, DocumentTypes={DtCount}, CounterParties={CpCount}, Currencies={CurCount}",
                    Model.Mode, isLoading, documentTypes.Count, counterParties.Count, currencies.Count);

                StateHasChanged();

                Logger.LogInformation("⏱️ PERF: StateHasChanged called. isLoading={IsLoading}, isLoadingChildren={IsLoadingChildren}",
                    isLoading, isLoadingChildren);

                // Safety timeout: Wait a bit for components to mount, then check if all called back
                _ = Task.Run(async () =>
                {
                    // Give components 500ms to mount and call their OnInitializedAsync
                    await Task.Delay(500);

                    // Then wait up to 5 more seconds for them to complete their initialization
                    await Task.Delay(5000);
                    lock (childLoadLock)
                    {
                        if (isLoadingChildren)
                        {
                            Logger.LogWarning("⚠️ Child component loading timeout! Only {Count}/{Total} components called back. Forcing completion.",
                                loadedChildComponentCount, TotalChildComponents);
                            _ = InvokeAsync(() =>
                            {
                                // Preserve hasUnsavedChanges if it was set to true (e.g., check-in mode with pre-registered document)
                                CreateSnapshot(preserveUnsavedChanges: hasUnsavedChanges);
                                enableChangeTracking = true;
                                isLoadingChildren = false;
                                StateHasChanged();
                            });
                        }
                    }
                });
            }
        }
    }

    /// <summary>
    /// Called by each child component when it completes initialization
    /// </summary>
    private async Task OnChildComponentLoadComplete()
    {
        bool allChildrenLoaded = false;
        int currentCount = 0;
        bool shouldProcess = false;

        lock (childLoadLock)
        {
            // Only process callbacks after parent finishes loading
            if (!acceptChildCallbacks)
            {
                Logger.LogDebug("Child callback received but parent not ready yet - ignoring");
                return;
            }

            shouldProcess = true;
            loadedChildComponentCount++;
            currentCount = loadedChildComponentCount;
            allChildrenLoaded = loadedChildComponentCount >= TotalChildComponents;
        }

        if (shouldProcess)
        {
            Logger.LogInformation("Child component load callback invoked. Progress: {Count}/{Total}, AllLoaded: {AllLoaded}",
                currentCount, TotalChildComponents, allChildrenLoaded);
        }

        if (allChildrenLoaded)
        {
            // All child components have finished loading - now create snapshot and enable change tracking
            await InvokeAsync(() => {
                Logger.LogInformation("Creating snapshot after all children loaded...");

                // Create snapshot NOW, after all children have loaded their data
                // Preserve hasUnsavedChanges if it was set to true (e.g., check-in mode with pre-registered document)
                CreateSnapshot(preserveUnsavedChanges: hasUnsavedChanges);

                enableChangeTracking = true;
                isLoadingChildren = false; // Hide the loading overlay now
                Logger.LogInformation("All child components loaded. Snapshot created. Change tracking ENABLED: {Enabled}. Loading complete.", enableChangeTracking);
                StateHasChanged();
            });
        }
    }

    // ========================================
    // LOAD REFERENCE DATA (Shared across all child components)
    // ========================================

    private async Task LoadDocumentTypesAsync()
    {
        try
        {
            documentTypes = await DocumentTypeService.GetAllAsync();
            Logger.LogInformation("Loaded {Count} document types", documentTypes.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading document types");
            documentTypes = new();
        }
    }

    private async Task LoadCounterPartiesAsync()
    {
        try
        {
            counterParties = await CounterPartyService.GetAllAsync();
            Logger.LogInformation("Loaded {Count} counter parties", counterParties.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading counter parties");
            counterParties = new();
        }
    }

    private async Task LoadCurrenciesAsync()
    {
        try
        {
            currencies = await CurrencyService.GetAllAsync();
            Logger.LogInformation("Loaded {Count} currencies", currencies.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading currencies");
            currencies = new();
        }
    }

    // ========================================
    // LOAD PAGE-SPECIFIC DATA
    // ========================================

    private async Task LoadEditModeAsync(int barcode)
    {
        var document = await DocumentService.GetByBarCodeAsync(barcode.ToString());

        if (document == null)
        {
            errorMessage = $"Document with barcode {barcode} not found.";
            Logger.LogWarning(errorMessage);
            return;
        }

        // Update existing Model instead of replacing it to avoid triggering child component re-initialization
        UpdateModelFromDto(Model, document);
        Model.Mode = DocumentPropertyMode.Edit;
        Model.PropertySetNumber = 2; // DispatchDate enabled
    }

    private async Task LoadCheckInModeAsync(string fileName)
    {
        // Extract barcode from filename
        var barcode = ExtractBarcodeFromFileName(fileName);
        Logger.LogInformation("Extracted barcode {BarCode} from filename {FileName}", barcode, fileName);

        // Load scanned file from CheckinDirectory via ScannedFileService
        var scannedFile = await ScannedFileService.GetFileByNameAsync(fileName);

        if (scannedFile == null)
        {
            errorMessage = $"Scanned file '{fileName}' not found in check-in directory.";
            Logger.LogWarning(errorMessage);
            return;
        }
        Logger.LogInformation($"Loaded scanned file {fileName}. ({scannedFile.SizeFormatted})");

        // Load file bytes
        var fileBytes = await ScannedFileService.GetFileContentAsync(fileName);

        if (fileBytes == null || fileBytes.Length == 0)
        {
            errorMessage = $"Failed to load content for file '{fileName}'.";
            Logger.LogWarning(errorMessage);
            return;
        }

        // Check if document with this barcode already exists
        var existingDocument = await DocumentService.GetByBarCodeAsync(barcode);

        if (existingDocument == null)
        {
            // CASE A: Document does NOT exist - create new (current behavior)
            Logger.LogInformation("No existing document found for barcode {BarCode}. Creating new document for check-in.", barcode);

            Model = new DocumentPropertiesViewModel
            {
                Logger = Logger,
                Mode = DocumentPropertyMode.CheckIn,
                PropertySetNumber = 2, // DispatchDate enabled
                FileName = fileName,
                BarCode = barcode,
                FileBytes = fileBytes,
                SourceFilePath = scannedFile.FullPath
            };
            Logger.LogDebug($"Model FileBytes = {Model?.FileBytes?.Length.ToString() ?? "-unk-"}");

            // Show warning that document was not pre-registered
            warningMessage = $"The barcode {barcode} has not been previously registered. Please proceed to perform both the registration and check-in processes concurrently, or select 'Cancel' to abort the operation.";
        }
        else
        {
            // CASE B/C: Document exists - check if it already has a file
            if (existingDocument.FileId!=null)
            {
                // CASE C: Document exists WITH file - ERROR
                errorMessage = $"Document with barcode '{barcode}' already has a file attached ('{existingDocument.FileId}'). Cannot check in file '{fileName}'. This scanned file cannot be associated with an existing document that already has a file.";
                Logger.LogWarning("Check-in prevented: Document {DocumentId} with barcode {BarCode} already has file {ExistingFileId}. Attempted to check in {NewFileName}",
                    existingDocument.Id, barcode, existingDocument.FileId, fileName);
                return;
            }
            else
            {
                // CASE B: Document exists WITHOUT file - load document and attach file
                Logger.LogInformation("Found existing document {DocumentId} with barcode {BarCode} without file. Loading document properties for check-in of {FileName}",
                    existingDocument.Id, barcode, fileName);

                // Create a new ViewModel and populate it from the existing document
                Model = new DocumentPropertiesViewModel
                {
                    Logger = Logger,
                    Mode = DocumentPropertyMode.CheckIn,
                    PropertySetNumber = 2, // DispatchDate enabled
                    FileBytes = fileBytes,
                    FileName = fileName,
                    SourceFilePath = scannedFile.FullPath
                };

                // Update the model with data from existing document
                UpdateModelFromDto(Model, existingDocument);

                // Ensure Check-In mode settings are preserved
                Model.Mode = DocumentPropertyMode.CheckIn;
                Model.PropertySetNumber = 2;
                Model.FileBytes = fileBytes;
                Model.FileName = fileName;
                Model.SourceFilePath = scannedFile.FullPath;

                successMessage = $"Loaded existing document with barcode '{barcode}'. You can modify the document properties and attach the file '{fileName}' by clicking Save.";

                // Enable Save button since we're attaching a file (this is a change)
                hasUnsavedChanges = true;

                Logger.LogInformation("Successfully loaded existing document {DocumentId} for file check-in. File will be attached on save.", existingDocument.Id);
            }
        }
    }

    private void LoadRegisterMode()
    {
        Model = new DocumentPropertiesViewModel
        {
            Logger = Logger,
            Mode = DocumentPropertyMode.Register,
            PropertySetNumber = 1, // DispatchDate disabled
            FileName = null
        };
    }

    // ========================================
    // SAVE DOCUMENT
    // ========================================

    private async Task SaveDocument()
    {
        try
        {
            isSaving = true;
            validationErrors.Clear();
            successMessage = null;
            errorMessage = null;
            warningMessage = null;

            // Validate
            if (!ValidateModel())
            {
                Logger.LogWarning("Validation failed");
                return;
            }

            // Check if barcode already exists (Register mode only)
            if (Model.Mode == DocumentPropertyMode.Register)
            {
                var existingDoc = await DocumentService.GetByBarCodeAsync(Model.BarCode);
                if (existingDoc != null)
                {
                    warningMessage = $"⚠️ Warning: A document with barcode {Model.BarCode} already exists (Document ID: {existingDoc.Id}, Name: {existingDoc.Name}). Please change the barcode to register a new document.";
                    Logger.LogWarning("Barcode {BarCode} already exists. Document ID: {DocumentId}", Model.BarCode, existingDoc.Id);
                    return;
                }
            }

            // Check for duplicates (Register and Check-in modes only)
            if ((Model.Mode == DocumentPropertyMode.Register || Model.Mode == DocumentPropertyMode.CheckIn)
                && !duplicateConfirmed)
            {
                if (await CheckForDuplicates())
                {
                    return; // Show duplicate modal, wait for user confirmation
                }
            }

            // Save based on mode
            if (Model.Mode == DocumentPropertyMode.Edit)
            {
                await SaveEditModeAsync();
            }
            else
            {
                await SaveRegisterOrCheckInModeAsync();
            }
            Logger.LogInformation($"Saved document. barcode={Model.BarCode}, filename={Model.FileName}, bytes ={Model.FileBytes?.Length.ToString() ?? "-null-"} ");

            // Post-save actions based on mode
            // Note: For Register mode, HandlePostSave will clear the form and create a new snapshot
            // For Edit/CheckIn modes, HandlePostSave navigates away so no snapshot needed
            await HandlePostSave();

            // Create new snapshot after successful save (for Edit mode only, as Register mode creates its own after clearing)
            if (Model.Mode == DocumentPropertyMode.Edit)
            {
                CreateSnapshot();
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Failed to save document: {ex.Message}";
            Logger.LogError(ex, "Failed to save document");
        }
        finally
        {
            isSaving = false;
            StateHasChanged();
        }
    }

    private async Task SaveEditModeAsync()
    {
        var updateDto = MapToUpdateDto();
        await DocumentService.UpdateAsync(updateDto);
        successMessage = "Successfully changed the document properties!";
        Logger.LogInformation("Document properties updated successfully");
        await AuditTrailService.LogAsync(
            AuditAction.Edit,
            Model.BarCode,
            "Modified document."
        );
    }

    private async Task SaveRegisterOrCheckInModeAsync()
    {
        // Check if we're updating an existing document (CASE B: Check-in to existing document)
        // or creating a new one (CASE A: New document check-in or Register mode)
        if (Model.Id.HasValue && Model.Id.Value > 0)
        {
            // Update existing document with file attachment
            Logger.LogInformation("Updating existing document {DocumentId} and attaching file in Check-In mode", Model.Id.Value);

            var updateDto = MapToUpdateDto();
            var result = await DocumentService.UpdateAsync(updateDto);

            successMessage = "Successfully updated document and attached file!";
            Logger.LogInformation("Document {DocumentId} updated successfully with file attachment", result.Id);
        }
        else
        {
            // Create new document
            Logger.LogInformation("Creating new document in {Mode} mode", Model.Mode);

            var createDto = MapToCreateDto();
            var result = await DocumentService.CreateAsync(createDto);

            Model.Id = result.Id;

            successMessage = Model.Mode == DocumentPropertyMode.Register
                ? "Successfully registered document!"
                : "Successfully checked in document!";

            Logger.LogInformation("Document created successfully with ID {DocumentId}", result.Id);
        }

        if (Model.Mode == DocumentPropertyMode.CheckIn) {
            await AuditTrailService.LogAsync(
                AuditAction.CheckIn,
                Model.BarCode,
                $"File checked in: {Model.FileName} ({Model.FileBytes?.Length ?? 0} bytes)"
            );
            
            // Delete the file from check-in directory after successful check-in
            if (!string.IsNullOrEmpty(Model.FileName))
            {
                try
                {
                    Logger.LogInformation("Attempting to delete checked-in file from directory: {FileName}", Model.FileName);
                    var deleted = await ScannedFileService.DeleteFileAsync(Model.FileName);

                    if (deleted)
                    {
                        Logger.LogInformation("Successfully deleted file from check-in directory: {FileName}", Model.FileName);
                    }
                    else
                    {
                        var errorMsg = $"Warning: File '{Model.FileName}' could not be deleted from check-in directory. The file may have already been removed.";
                        Logger.LogWarning(errorMsg);
                        errorMessage = errorMsg;
                    }
                }
                catch (Exception ex)
                {
                    var errorMsg = $"Error: Failed to delete file '{Model.FileName}' from check-in directory: {ex.Message}";
                    Logger.LogError(ex, "Failed to delete file from check-in directory: {FileName}", Model.FileName);
                    errorMessage = errorMsg;
                    throw new InvalidOperationException(errorMsg, ex);
                }
            }
            
        } else {
            await AuditTrailService.LogAsync(
                AuditAction.Register,
                Model.BarCode,
                "Registered document."
            );
        }
    }

    private async Task HandlePostSave()
    {
        duplicateConfirmed = false;

        if (Model.Mode == DocumentPropertyMode.Register)
        {
            // Stay on page, clear form for next entry, focus barcode
            var currentMode = Model.Mode;
            var currentPropertySet = Model.PropertySetNumber;

            // Disable change tracking while clearing form
            enableChangeTracking = false;

            Model = new DocumentPropertiesViewModel
            {
                Logger = Logger,
                Mode = currentMode,
                PropertySetNumber = currentPropertySet
            };

            // Create snapshot of the cleared form to prevent "unsaved changes" warning
            CreateSnapshot();

            await Task.Delay(100);
            await JSRuntime.InvokeVoidAsync("eval",
                "document.getElementById('barcodeInput')?.focus()");

            // Re-enable change tracking
            enableChangeTracking = true;
        }
        else
        {
            // Edit and Check-in modes: close/navigate back
            await Task.Delay(2000); // Show success message briefly
            NavigationManager.NavigateTo("/checkin-scanned");
        }
    }

    // ========================================
    // VALIDATION
    // ========================================

    private bool ValidateModel()
    {
        validationErrors.Clear();

        // Barcode validation (always required)
        if (string.IsNullOrWhiteSpace(Model.BarCode))
        {
            validationErrors.Add("Bar Code is required.");
        }
        else if (!int.TryParse(Model.BarCode, out _))
        {
            validationErrors.Add("Bar Code must be a valid integer.");
        }

        // Document Type (always required)
        if (!Model.DocumentTypeId.HasValue)
        {
            validationErrors.Add("Document Type is required.");
        }

        // Dynamic field validation based on DocumentType configuration
        // DOCUMENT SECTION
        ValidateMandatoryField("CounterParty", string.IsNullOrWhiteSpace(Model.CounterPartyId), "Counterparty");
        ValidateMandatoryField("DateOfContract", !Model.DateOfContract.HasValue, "Date of Contract");
        ValidateMandatoryField("ReceivingDate", !Model.ReceivingDate.HasValue, "Receiving Date");
        ValidateMandatoryField("SendingOutDate", !Model.SendingOutDate.HasValue, "Sending Out Date");
        ValidateMandatoryField("ForwardedToSignatoriesDate", !Model.ForwardedToSignatoriesDate.HasValue, "Forwarded to Signatories Date");

        // DispatchDate: validate if enabled AND mandatory
        if (Model.IsDispatchDateEnabled)
        {
            ValidateMandatoryField("DispatchDate", !Model.DispatchDate.HasValue, "Dispatch Date");
        }

        ValidateMandatoryField("Comment", string.IsNullOrWhiteSpace(Model.Comment), "Comment");

        // ACTION SECTION
        ValidateMandatoryField("ActionDate", !Model.ActionDate.HasValue, "Action Date");
        ValidateMandatoryField("ActionDescription", string.IsNullOrWhiteSpace(Model.ActionDescription), "Action Description");

        // Action section: all or none (only if at least one is mandatory or filled)
        bool hasActionDate = Model.ActionDate.HasValue;
        bool hasActionDesc = !string.IsNullOrWhiteSpace(Model.ActionDescription);
        bool isActionDateMandatory = Model.IsFieldMandatory("ActionDate");
        bool isActionDescMandatory = Model.IsFieldMandatory("ActionDescription");

        if ((hasActionDate || hasActionDesc || isActionDateMandatory || isActionDescMandatory) &&
            (hasActionDate != hasActionDesc))
        {
            validationErrors.Add("Action Date and Action Description must both be filled or both be empty.");
        }

        // FLAGS SECTION
        ValidateMandatoryField("Fax", !Model.Fax.HasValue, "Fax", "flag must be set to Yes or No");
        ValidateMandatoryField("OriginalReceived", !Model.OriginalReceived.HasValue, "Original Received", "flag must be set to Yes or No");
        ValidateMandatoryField("TranslatedVersionReceived", !Model.TranslationReceived.HasValue, "Translation Received", "flag must be set to Yes or No");
        ValidateMandatoryField("Confidential", !Model.Confidential.HasValue, "Confidential", "flag must be set to Yes or No");

        // ADDITIONAL INFO SECTION
        ValidateMandatoryField("DocumentNo", string.IsNullOrWhiteSpace(Model.DocumentNo), "Document No.");
        ValidateMandatoryField("VersionNo", string.IsNullOrWhiteSpace(Model.VersionNo), "Version No.");
        ValidateMandatoryField("AssociatedToPua", string.IsNullOrWhiteSpace(Model.AssociatedToPUA), "Associated to PUA/Agreement No.");
        ValidateMandatoryField("AssociatedToAppendix", string.IsNullOrWhiteSpace(Model.AssociatedToAppendix), "Associated to Appendix No.");
        ValidateMandatoryField("ValidUntil", !Model.ValidUntil.HasValue, "Valid Until/As Of");
        ValidateMandatoryField("Amount", !Model.Amount.HasValue, "Amount");
        ValidateMandatoryField("Authorisation", string.IsNullOrWhiteSpace(Model.Authorisation), "Authorisation to");
        ValidateMandatoryField("BankConfirmation", !Model.BankConfirmation.HasValue, "Bank Confirmation", "flag must be set to Yes or No");

        // Currency: required if Amount entered OR if Currency field is mandatory
        if (Model.Amount.HasValue && string.IsNullOrWhiteSpace(Model.CurrencyCode))
        {
            validationErrors.Add("Currency is required when Amount is entered.");
        }
        else if (Model.IsFieldMandatory("Currency") && string.IsNullOrWhiteSpace(Model.CurrencyCode))
        {
            validationErrors.Add("Currency is required.");
        }

        return validationErrors.Count == 0;
    }

    /// <summary>
    /// Validates a field only if it is marked as mandatory in the field configuration
    /// </summary>
    private void ValidateMandatoryField(string fieldName, bool isEmpty, string displayName, string suffix = "is required")
    {
        if (Model.IsFieldMandatory(fieldName) && isEmpty)
        {
            validationErrors.Add($"{displayName} {suffix}.");
        }
    }

    // ========================================
    // DUPLICATE DETECTION
    // ========================================

    private async Task<bool> CheckForDuplicates()
    {
        // TODO: Implement GetSimilarDocuments in DocumentService
        // For now, return false (no duplicates)

        // var similar = await DocumentService.GetSimilarDocuments(
        //     Model.DocumentTypeId, Model.DocumentNo, Model.VersionNo);

        // if (similar.Any())
        // {
        //     similarDocumentBarcodes = similar.Take(5).ToList();
        //     showDuplicateModal = true;
        //     StateHasChanged();
        //     return true; // Wait for user confirmation
        // }

        return false; // No duplicates
    }

    private async Task ConfirmDuplicate()
    {
        duplicateConfirmed = true;
        showDuplicateModal = false;
        await SaveDocument(); // Retry save with confirmation
    }

    private void CancelDuplicate()
    {
        showDuplicateModal = false;
    }

    // ========================================
    // COPY/PASTE FUNCTIONALITY
    // ========================================

    private async Task CopyFormData()
    {
        try
        {
            var userName = "CurrentUser"; // TODO: Get from ICurrentUserService
            var copyState = FormDataCopyState.Create(Model, userName);
            var json = copyState.ToJson();

            await JSRuntime.InvokeVoidAsync("localStorage.setItem",
                FormDataCopyState.LocalStorageKey, json);

            hasCopiedData = true;
            copiedDataExpiration = DateTime.Now.AddDays(FormDataCopyState.ExpirationDays);

            successMessage = "Form data copied successfully!";
            StateHasChanged();
        }
        catch (Exception ex)
        {
            errorMessage = $"Failed to copy form data: {ex.Message}";
        }
    }

    private async Task PasteFormData()
    {
        try
        {
            var json = await JSRuntime.InvokeAsync<string>("localStorage.getItem",
                FormDataCopyState.LocalStorageKey);

            if (string.IsNullOrEmpty(json))
            {
                errorMessage = "No copied data found.";
                return;
            }

            var copyState = FormDataCopyState.FromJson(json);
            if (copyState == null || copyState.IsExpired)
            {
                errorMessage = "Copied data has expired.";
                await JSRuntime.InvokeVoidAsync("localStorage.removeItem",
                    FormDataCopyState.LocalStorageKey);
                hasCopiedData = false;
                return;
            }

            var copiedModel = copyState.GetViewModel();
            if (copiedModel == null)
            {
                errorMessage = "Failed to parse copied data.";
                return;
            }

            // Paste all fields except BarCode and audit fields
            Model.DocumentTypeId = copiedModel.DocumentTypeId;
            Model.CounterPartyNoAlpha = copiedModel.CounterPartyNoAlpha;
            Model.CounterPartyId = copiedModel.CounterPartyId;
            Model.SelectedThirdPartyIds = new List<string>(copiedModel.SelectedThirdPartyIds);
            Model.SelectedThirdPartyNames = new List<string>(copiedModel.SelectedThirdPartyNames);
            Model.DateOfContract = copiedModel.DateOfContract;
            Model.ReceivingDate = copiedModel.ReceivingDate;
            Model.SendingOutDate = copiedModel.SendingOutDate;
            Model.ForwardedToSignatoriesDate = copiedModel.ForwardedToSignatoriesDate;

            // Only paste DispatchDate if not in Register mode (PropertySetNumber = 2 means DispatchDate is enabled)
            if (Model.PropertySetNumber == 2)
            {
                Model.DispatchDate = copiedModel.DispatchDate;
            }

            Model.Comment = copiedModel.Comment;
            Model.ActionDate = copiedModel.ActionDate;
            Model.ActionDescription = copiedModel.ActionDescription;
            Model.Fax = copiedModel.Fax;
            Model.OriginalReceived = copiedModel.OriginalReceived;
            Model.TranslationReceived = copiedModel.TranslationReceived;
            Model.Confidential = copiedModel.Confidential;
            Model.DocumentNameId = copiedModel.DocumentNameId;
            Model.DocumentNo = copiedModel.DocumentNo;
            Model.VersionNo = copiedModel.VersionNo;
            Model.AssociatedToPUA = copiedModel.AssociatedToPUA;
            Model.AssociatedToAppendix = copiedModel.AssociatedToAppendix;
            Model.ValidUntil = copiedModel.ValidUntil;
            Model.Amount = copiedModel.Amount;
            Model.CurrencyCode = copiedModel.CurrencyCode;
            Model.Authorisation = copiedModel.Authorisation;
            Model.BankConfirmation = copiedModel.BankConfirmation;

            // Apply field configuration based on pasted document type
            ApplyDocumentTypeFieldConfiguration(Model.DocumentTypeId);

            // Trigger change detection to enable Save button
            CheckForChanges();

            successMessage = "Form data pasted successfully!";
            StateHasChanged();
        }
        catch (Exception ex)
        {
            errorMessage = $"Failed to paste form data: {ex.Message}";
        }
    }

    private async Task CheckForCopiedData()
    {
        try
        {
            var json = await JSRuntime.InvokeAsync<string>("localStorage.getItem",
                FormDataCopyState.LocalStorageKey);

            if (!string.IsNullOrEmpty(json))
            {
                var copyState = FormDataCopyState.FromJson(json);
                if (copyState != null && !copyState.IsExpired)
                {
                    hasCopiedData = true;
                    copiedDataExpiration = copyState.CopiedAt.AddDays(FormDataCopyState.ExpirationDays);
                }
                else
                {
                    // Remove expired data
                    await JSRuntime.InvokeVoidAsync("localStorage.removeItem",
                        FormDataCopyState.LocalStorageKey);
                }
                StateHasChanged();
            }
        }
        catch
        {
            // Ignore errors checking for copied data
        }
    }

    // ========================================
    // BUTTON ACTIONS
    // ========================================

    private async Task ViewDocument()
    {
        if (Model.Id.HasValue)
        {
            await JSRuntime.InvokeVoidAsync("window.open",
                $"/api/documents/{Model.Id}/file", "_blank");
        }
    }

    private async Task CompareWithStandard()
    {
        // TODO: Implement compare with standard contract functionality
        await Task.CompletedTask;
    }

    private void OnBarcodeChanged()
    {
        // Clear warning message when barcode is changed
        warningMessage = null;
        Logger.LogInformation("Barcode changed to {BarCode}, warning cleared", Model.BarCode);
    }

    private async Task Cancel()
    {
        // Check for unsaved changes
        if (hasUnsavedChanges && !isSaving)
        {
            var confirmed = await JSRuntime.InvokeAsync<bool>("confirm",
                "You have unsaved changes. Are you sure you want to leave this page?");

            if (!confirmed)
            {
                return; // User cancelled
            }

            // User confirmed, clear flag and navigate
            hasUnsavedChanges = false;
        }

        // Navigate based on mode
        var destination = Model.Mode == DocumentPropertyMode.CheckIn
            ? "/checkin-scanned"
            : "/documents";

        NavigationManager.NavigateTo(destination);
    }

    // ========================================
    // HELPER METHODS
    // ========================================

    private string GetPageTitle()
    {
        return Model.Mode switch
        {
            DocumentPropertyMode.Edit => $"Edit Document - {Model.BarCode}",
            DocumentPropertyMode.Register => "Register New Document",
            DocumentPropertyMode.CheckIn => $"Check-in Document - {Model.FileName}",
            _ => "Document Properties"
        };
    }

    private string ExtractBarcodeFromFileName(string fileName)
    {
        // Extract barcode from filename like "12345.pdf" → "12345"
        return System.IO.Path.GetFileNameWithoutExtension(fileName);
    }

    private void ApplyDocumentTypeFieldConfiguration(int? documentTypeId)
    {
        // Clear existing configuration
        Model.FieldConfig.Clear();

        if (!documentTypeId.HasValue)
        {
            Logger.LogInformation("No document type selected, clearing field configuration");
            return;
        }

        // Find the document type
        var documentType = documentTypes.FirstOrDefault(dt => dt.DtId == documentTypeId.Value);
        if (documentType == null)
        {
            Logger.LogWarning("Document type {DocumentTypeId} not found", documentTypeId.Value);
            return;
        }

        Logger.LogInformation("Applying field configuration for document type: {DocumentTypeName} (ID: {DocumentTypeId})",
            documentType.DtName, documentType.DtId);

        // Map DocumentType properties to FieldConfig
        // M = Mandatory, O = Optional, N = Not Applicable (disabled)
        Model.FieldConfig["CounterParty"] = ParseFieldVisibility(documentType.CounterParty);
        Model.FieldConfig["DateOfContract"] = ParseFieldVisibility(documentType.DateOfContract);
        Model.FieldConfig["Comment"] = ParseFieldVisibility(documentType.Comment);
        Model.FieldConfig["ReceivingDate"] = ParseFieldVisibility(documentType.ReceivingDate);
        Model.FieldConfig["DispatchDate"] = ParseFieldVisibility(documentType.DispatchDate);
        Model.FieldConfig["SendingOutDate"] = ParseFieldVisibility(documentType.SendingOutDate);
        Model.FieldConfig["ForwardedToSignatoriesDate"] = ParseFieldVisibility(documentType.ForwardedToSignatoriesDate);
        Model.FieldConfig["Fax"] = ParseFieldVisibility(documentType.Fax);
        Model.FieldConfig["OriginalReceived"] = ParseFieldVisibility(documentType.OriginalReceived);
        Model.FieldConfig["TranslatedVersionReceived"] = ParseFieldVisibility(documentType.TranslatedVersionReceived);
        Model.FieldConfig["Confidential"] = ParseFieldVisibility(documentType.Confidential);
        Model.FieldConfig["DocumentNo"] = ParseFieldVisibility(documentType.DocumentNo);
        Model.FieldConfig["VersionNo"] = ParseFieldVisibility(documentType.VersionNo);
        Model.FieldConfig["AssociatedToPua"] = ParseFieldVisibility(documentType.AssociatedToPua);
        Model.FieldConfig["AssociatedToAppendix"] = ParseFieldVisibility(documentType.AssociatedToAppendix);
        Model.FieldConfig["ValidUntil"] = ParseFieldVisibility(documentType.ValidUntil);
        Model.FieldConfig["Currency"] = ParseFieldVisibility(documentType.Currency);
        Model.FieldConfig["Amount"] = ParseFieldVisibility(documentType.Amount);
        Model.FieldConfig["Authorisation"] = ParseFieldVisibility(documentType.Authorisation);
        Model.FieldConfig["BankConfirmation"] = ParseFieldVisibility(documentType.BankConfirmation);
        Model.FieldConfig["ActionDate"] = ParseFieldVisibility(documentType.ActionDate);
        Model.FieldConfig["ActionDescription"] = ParseFieldVisibility(documentType.ActionDescription);
        Model.FieldConfig["ReminderGroup"] = ParseFieldVisibility(documentType.ReminderGroup);

        Logger.LogInformation("Field configuration applied: {Count} fields configured", Model.FieldConfig.Count);
    }

    private FieldVisibility ParseFieldVisibility(string code)
    {
        return code?.ToUpperInvariant() switch
        {
            "M" => FieldVisibility.Mandatory,
            "O" => FieldVisibility.Optional,
            "N" => FieldVisibility.NotApplicable,
            _ => FieldVisibility.Optional // Default to Optional if unknown
        };
    }

    // ========================================
    // DTO MAPPING
    // ========================================

    private DocumentPropertiesViewModel MapFromDto(DocumentDto dto)
    {
        var model = new DocumentPropertiesViewModel
        {
            Logger = Logger,
            Id = dto.Id,
            BarCode = dto.BarCode.ToString(),
            Name = dto.Name,
            FileName = dto.BarCode + ".pdf", // TODO: Get actual filename from DocumentFile
            DocumentTypeId = dto.DocumentTypeId,
            CounterPartyId = dto.CounterPartyId?.ToString(),
            DateOfContract = dto.DateOfContract,
            ReceivingDate = dto.ReceivingDate,
            SendingOutDate = dto.SendingOutDate,
            ForwardedToSignatoriesDate = dto.ForwardedToSignatoriesDate,
            DispatchDate = dto.DispatchDate,
            Comment = dto.Comment,
            ActionDate = dto.ActionDate,
            ActionDescription = dto.ActionDescription,
            EmailReminderGroup = dto.ReminderGroup,
            Fax = dto.Fax,
            OriginalReceived = dto.OriginalReceived,
            TranslationReceived = dto.TranslatedVersionReceived,
            Confidential = dto.Confidential,
            DocumentNameId = dto.DocumentNameId,
            DocumentNo = dto.DocumentNo,
            VersionNo = dto.VersionNo,
            AssociatedToPUA = dto.AssociatedToPua,
            AssociatedToAppendix = dto.AssociatedToAppendix,
            ValidUntil = dto.ValidUntil,
            Amount = dto.Amount,
            CurrencyCode = dto.CurrencyCode,
            Authorisation = dto.Authorisation,
            BankConfirmation = dto.BankConfirmation,
            CreatedOn = dto.CreatedOn,
            CreatedBy = dto.CreatedBy,
            ModifiedOn = dto.ModifiedOn,
            ModifiedBy = dto.ModifiedBy
        };

        model.SetThirdPartyIdsFromString(dto.ThirdPartyId);
        model.SetThirdPartyNamesFromString(dto.ThirdParty);

        return model;
    }

    private void UpdateModelFromDto(DocumentPropertiesViewModel model, DocumentDto dto)
    {
        model.Logger = Logger;
        model.Id = dto.Id;
        model.BarCode = dto.BarCode.ToString();
        model.Name = dto.Name;
        model.FileName = dto.BarCode + ".pdf"; // TODO: Get actual filename from DocumentFile
        model.DocumentTypeId = dto.DocumentTypeId;
        model.CounterPartyId = dto.CounterPartyId?.ToString();
        model.DateOfContract = dto.DateOfContract;
        model.ReceivingDate = dto.ReceivingDate;
        model.SendingOutDate = dto.SendingOutDate;
        model.ForwardedToSignatoriesDate = dto.ForwardedToSignatoriesDate;
        model.DispatchDate = dto.DispatchDate;
        model.Comment = dto.Comment;
        model.ActionDate = dto.ActionDate;
        model.ActionDescription = dto.ActionDescription;
        model.EmailReminderGroup = dto.ReminderGroup;
        model.Fax = dto.Fax;
        model.OriginalReceived = dto.OriginalReceived;
        model.TranslationReceived = dto.TranslatedVersionReceived;
        model.Confidential = dto.Confidential;
        model.DocumentNameId = dto.DocumentNameId;
        model.DocumentNo = dto.DocumentNo;
        model.VersionNo = dto.VersionNo;
        model.AssociatedToPUA = dto.AssociatedToPua;
        model.AssociatedToAppendix = dto.AssociatedToAppendix;
        model.ValidUntil = dto.ValidUntil;
        model.Amount = dto.Amount;
        model.CurrencyCode = dto.CurrencyCode;
        model.Authorisation = dto.Authorisation;
        model.BankConfirmation = dto.BankConfirmation;
        model.CreatedOn = dto.CreatedOn;
        model.CreatedBy = dto.CreatedBy;
        model.ModifiedOn = dto.ModifiedOn;
        model.ModifiedBy = dto.ModifiedBy;

        model.SetThirdPartyIdsFromString(dto.ThirdPartyId);
        model.SetThirdPartyNamesFromString(dto.ThirdParty);

        // Apply field configuration based on document type
        ApplyDocumentTypeFieldConfiguration(dto.DocumentTypeId);
    }

    private UpdateDocumentDto MapToUpdateDto()
    {
        return new UpdateDocumentDto
        {
            Id = Model.Id!.Value,
            Name = Model.Name,
            BarCode = Model.BarCode,
            DocumentTypeId = Model.DocumentTypeId,
            CounterPartyId = int.Parse(Model.CounterPartyId ?? "-1"),
            ThirdPartyId = Model.GetThirdPartyIdString(),
            ThirdParty = Model.GetThirdPartyNameString(),
            DateOfContract = Model.DateOfContract,
            ReceivingDate = Model.ReceivingDate,
            SendingOutDate = Model.SendingOutDate,
            ForwardedToSignatoriesDate = Model.ForwardedToSignatoriesDate,
            DispatchDate = Model.DispatchDate,
            Comment = Model.Comment,
            ActionDate = Model.ActionDate,
            ActionDescription = Model.ActionDescription,
            ReminderGroup = Model.EmailReminderGroup,
            Fax = Model.Fax,
            OriginalReceived = Model.OriginalReceived,
            TranslatedVersionReceived = Model.TranslationReceived,
            Confidential = Model.Confidential,
            DocumentNameId = Model.DocumentNameId,
            DocumentNo = Model.DocumentNo,
            VersionNo = Model.VersionNo,
            AssociatedToPua = Model.AssociatedToPUA,
            AssociatedToAppendix = Model.AssociatedToAppendix,
            ValidUntil = Model.ValidUntil,
            Amount = Model.Amount,
            CurrencyCode = Model.CurrencyCode,
            Authorisation = Model.Authorisation,
            BankConfirmation = Model.BankConfirmation,
            // File upload (for attaching files to existing documents during check-in)
            FileBytes = Model.FileBytes,
            FileName = Model.FileName,
            FileType = Model.FileName != null ? Path.GetExtension(Model.FileName).TrimStart('.') : null
        };
    }

    private CreateDocumentDto MapToCreateDto()
    {
        return new CreateDocumentDto
        {
            Name = Model.Name ?? Model.FileName ?? $"{Model.BarCode}.pdf",
            BarCode = Model.BarCode,
            DocumentTypeId = Model.DocumentTypeId,
            CounterPartyId = int.Parse(Model.CounterPartyId ?? "-1"),
            ThirdPartyId = Model.GetThirdPartyIdString(),
            ThirdParty = Model.GetThirdPartyNameString(),
            DateOfContract = Model.DateOfContract,
            ReceivingDate = Model.ReceivingDate,
            SendingOutDate = Model.SendingOutDate,
            ForwardedToSignatoriesDate = Model.ForwardedToSignatoriesDate,
            DispatchDate = Model.DispatchDate,
            Comment = Model.Comment,
            ActionDate = Model.ActionDate,
            ActionDescription = Model.ActionDescription,
            ReminderGroup = Model.EmailReminderGroup,
            Fax = Model.Fax,
            OriginalReceived = Model.OriginalReceived,
            TranslatedVersionReceived = Model.TranslationReceived,
            Confidential = Model.Confidential,
            DocumentNameId = Model.DocumentNameId,
            DocumentNo = Model.DocumentNo,
            VersionNo = Model.VersionNo,
            AssociatedToPua = Model.AssociatedToPUA,
            AssociatedToAppendix = Model.AssociatedToAppendix,
            ValidUntil = Model.ValidUntil,
            Amount = Model.Amount,
            CurrencyCode = Model.CurrencyCode,
            Authorisation = Model.Authorisation,
            BankConfirmation = Model.BankConfirmation,
            // File upload (Check-in mode)
            FileBytes = Model.FileBytes,
            FileName = Model.FileName,
            FileType = Model.FileName != null ? Path.GetExtension(Model.FileName).TrimStart('.') : null
        };
    }

    // ========================================
    // CHANGE TRACKING
    // ========================================

    private void CreateSnapshot(bool preserveUnsavedChanges = false)
    {
        // Create snapshot excluding FileBytes for performance
        // FileBytes can be very large (MB) and doesn't change after load
        var snapshotData = CreateSnapshotObject();
        originalModelJson = System.Text.Json.JsonSerializer.Serialize(snapshotData);

        // Reset hasUnsavedChanges unless explicitly told to preserve it
        // (e.g., in check-in mode with pre-registered document, we want to keep it true)
        if (!preserveUnsavedChanges)
        {
            hasUnsavedChanges = false;
        }

        Logger.LogInformation("Created Snapshot (FileBytes excluded from comparison). hasUnsavedChanges={HasChanges}, preserveUnsavedChanges={Preserve}", hasUnsavedChanges, preserveUnsavedChanges);
    }

    private void CheckForChanges()
    {
        // Don't check for changes until initial load is complete
        if (!enableChangeTracking)
        {
            Logger.LogDebug("CheckForChanges called but change tracking is disabled");
            return;
        }

        // Prevent recursive calls that cause infinite loop
        if (isCheckingForChanges)
        {
            return;
        }

        try
        {
            isCheckingForChanges = true;

            // Check if DocumentType has changed and apply field configuration
            if (Model.DocumentTypeId != lastDocumentTypeId)
            {
                Logger.LogInformation("DocumentType changed from {Old} to {New}, applying field configuration",
                    lastDocumentTypeId, Model.DocumentTypeId);
                ApplyDocumentTypeFieldConfiguration(Model.DocumentTypeId);
                lastDocumentTypeId = Model.DocumentTypeId;
            }

            if (string.IsNullOrEmpty(originalModelJson))
            {
                hasUnsavedChanges = false;
                Logger.LogDebug("CheckForChanges: No original snapshot exists");
                return;
            }

            // Create current snapshot excluding FileBytes
            var currentSnapshotData = CreateSnapshotObject();
            var currentJson = System.Text.Json.JsonSerializer.Serialize(currentSnapshotData);
            var hadChanges = hasUnsavedChanges;
            hasUnsavedChanges = currentJson != originalModelJson;

            Logger.LogInformation("CheckForChanges: hasUnsavedChanges = {HasChanges} (was: {HadChanges})", hasUnsavedChanges, hadChanges);

            // Only trigger re-render if the state actually changed
            if (hadChanges != hasUnsavedChanges)
            {
                StateHasChanged();
            }
        }
        finally
        {
            isCheckingForChanges = false;
        }
    }

    /// <summary>
    /// Creates a lightweight snapshot object for change tracking, excluding FileBytes
    /// </summary>
    private object CreateSnapshotObject()
    {
        return new
        {
            Model.Id,
            Model.BarCode,
            Model.Name,
            Model.FileName,
            Model.DocumentTypeId,
            Model.CounterPartyNoAlpha,
            Model.CounterPartyId,
            Model.SelectedThirdPartyIds,
            Model.SelectedThirdPartyNames,
            Model.DateOfContract,
            Model.ReceivingDate,
            Model.SendingOutDate,
            Model.ForwardedToSignatoriesDate,
            Model.DispatchDate,
            Model.Comment,
            Model.ActionDate,
            Model.ActionDescription,
            Model.EmailReminderGroup,
            Model.Fax,
            Model.OriginalReceived,
            Model.TranslationReceived,
            Model.Confidential,
            Model.DocumentNameId,
            Model.DocumentNo,
            Model.VersionNo,
            Model.AssociatedToPUA,
            Model.AssociatedToAppendix,
            Model.ValidUntil,
            Model.Amount,
            Model.CurrencyCode,
            Model.Authorisation,
            Model.BankConfirmation
            // FileBytes intentionally excluded - can be very large and doesn't change
            // SourceFilePath intentionally excluded - doesn't affect user data
        };
    }

    // ========================================
    // JAVASCRIPT NAVIGATION INTERCEPTION
    // ========================================

    private DotNetObjectReference<DocumentPropertiesPage>? dotNetRef;

    private async Task SetupJavaScriptNavigationInterception()
    {
        dotNetRef = DotNetObjectReference.Create(this);

        try
        {
            // Wait for the JavaScript file to load (max 5 seconds)
            var maxAttempts = 50;
            var attempt = 0;
            var scriptLoaded = false;

            while (attempt < maxAttempts && !scriptLoaded)
            {
                try
                {
                    // Check if the documentPropertiesPage object exists
                    scriptLoaded = await JSRuntime.InvokeAsync<bool>("eval",
                        "typeof window.documentPropertiesPage !== 'undefined'");

                    if (!scriptLoaded)
                    {
                        await Task.Delay(100);
                        attempt++;
                    }
                }
                catch
                {
                    await Task.Delay(100);
                    attempt++;
                }
            }
            Logger.LogInformation("JavaScript navigation interception script loaded: {Loaded}, Attempts: {attempts}", scriptLoaded, attempt);
            if (scriptLoaded)
            {
                // Call the init function from the external JavaScript file
                await JSRuntime.InvokeVoidAsync("documentPropertiesPage.init", dotNetRef);
            }
        }
        catch
        {
            // Ignore errors during setup - navigation will still work without interception
        }
    }

    [JSInvokable]
    public Task<bool> CheckHasUnsavedChanges()
    {
        return Task.FromResult(hasUnsavedChanges && !isSaving);
    }

    [JSInvokable]
    public Task ClearUnsavedChangesFlag()
    {
        hasUnsavedChanges = false;
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        // Clean up JavaScript navigation interception
        if (dotNetRef != null)
        {
            try
            {
                JSRuntime.InvokeVoidAsync("documentPropertiesPage.dispose");
                dotNetRef.Dispose();
            }
            catch
            {
                // Ignore errors during disposal
            }
        }
    }
}
