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
                CreateSnapshot();
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
                                CreateSnapshot();
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
                CreateSnapshot();

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
        // Extract barcode from filename first
        var barcode = ExtractBarcodeFromFileName(fileName);

        // Check if document with this barcode already exists
        var existingDocument = await DocumentService.GetByBarCodeAsync(barcode);
        if (existingDocument != null)
        {
            errorMessage = $"A document with barcode '{barcode}' already exists in the system. This file cannot be checked in. Please return to the scanned documents list.";
            Logger.LogWarning("Attempted to check in file {FileName} with barcode {BarCode} that already exists (Document ID: {DocumentId})",
                fileName, barcode, existingDocument.Id);
            return;
        }

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

        // Create model with loaded file data
        Model = new DocumentPropertiesViewModel
        {
            Mode = DocumentPropertyMode.CheckIn,
            PropertySetNumber = 2, // DispatchDate enabled
            FileName = fileName,
            BarCode = barcode,
            FileBytes = fileBytes,
            SourceFilePath = scannedFile.FullPath
        };
        Logger.LogDebug($"Model FileBytes = {Model?.FileBytes?.Length.ToString() ?? "-unk-"}");
    }

    private void LoadRegisterMode()
    {
        Model = new DocumentPropertiesViewModel
        {
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

            // Validate
            if (!ValidateModel())
            {
                Logger.LogWarning("Validation failed");
                return;
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
            await HandlePostSave();

            // Create new snapshot after successful save
            CreateSnapshot();
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
        var createDto = MapToCreateDto();
        var result = await DocumentService.CreateAsync(createDto);

        Model.Id = result.Id;

        successMessage = Model.Mode == DocumentPropertyMode.Register
            ? "Successfully registered document!"
            : "Successfully checked in document!";

        Logger.LogInformation("Document created successfully with ID {DocumentId}", result.Id);

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
                Mode = currentMode,
                PropertySetNumber = currentPropertySet
            };

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

        // Barcode validation
        if (string.IsNullOrWhiteSpace(Model.BarCode))
        {
            validationErrors.Add("Bar Code is required.");
        }
        else if (!int.TryParse(Model.BarCode, out _))
        {
            validationErrors.Add("Bar Code must be a valid integer.");
        }

        // Required fields
        if (!Model.DocumentTypeId.HasValue)
            validationErrors.Add("Document Type is required.");

        if (string.IsNullOrWhiteSpace(Model.CounterPartyId))
            validationErrors.Add("Counterparty is required.");

        if (!Model.DateOfContract.HasValue)
            validationErrors.Add("Date of Contract is required.");

        if (!Model.ReceivingDate.HasValue)
            validationErrors.Add("Receiving Date is required.");

        if (!Model.SendingOutDate.HasValue)
            validationErrors.Add("Sending Out Date is required.");

        if (!Model.ForwardedToSignatoriesDate.HasValue)
            validationErrors.Add("Forwarded to Signatories Date is required.");

        if (Model.IsDispatchDateEnabled && !Model.DispatchDate.HasValue)
            validationErrors.Add("Dispatch Date is required.");

        if (string.IsNullOrWhiteSpace(Model.Comment))
            validationErrors.Add("Comment is required.");

        if (string.IsNullOrWhiteSpace(Model.DocumentNo))
            validationErrors.Add("Document No. is required.");

        if (string.IsNullOrWhiteSpace(Model.VersionNo))
            validationErrors.Add("Version No. is required.");

        // Conditional validations
        if (Model.Amount.HasValue && string.IsNullOrWhiteSpace(Model.CurrencyCode))
        {
            validationErrors.Add("Currency is required when Amount is entered.");
        }

        // Action section: all or none
        bool hasActionDate = Model.ActionDate.HasValue;
        bool hasActionDesc = !string.IsNullOrWhiteSpace(Model.ActionDescription);
        if (hasActionDate != hasActionDesc)
        {
            validationErrors.Add("Action Date and Action Description must both be filled or both be empty.");
        }

        // Flags must be true or false, not null
        if (!Model.Fax.HasValue)
            validationErrors.Add("Fax flag must be set to Yes or No.");

        if (!Model.OriginalReceived.HasValue)
            validationErrors.Add("Original Received flag must be set to Yes or No.");

        if (!Model.TranslationReceived.HasValue)
            validationErrors.Add("Translation Received flag must be set to Yes or No.");

        if (!Model.Confidential.HasValue)
            validationErrors.Add("Confidential flag must be set to Yes or No.");

        if (!Model.BankConfirmation.HasValue)
            validationErrors.Add("Bank Confirmation flag must be set to Yes or No.");

        return validationErrors.Count == 0;
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
            Model.DispatchDate = copiedModel.DispatchDate;
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

        NavigationManager.NavigateTo("/documents");
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

    // ========================================
    // DTO MAPPING
    // ========================================

    private DocumentPropertiesViewModel MapFromDto(DocumentDto dto)
    {
        var model = new DocumentPropertiesViewModel
        {
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
            BankConfirmation = Model.BankConfirmation
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

    private void CreateSnapshot()
    {
        // Create snapshot excluding FileBytes for performance
        // FileBytes can be very large (MB) and doesn't change after load
        var snapshotData = CreateSnapshotObject();
        originalModelJson = System.Text.Json.JsonSerializer.Serialize(snapshotData);
        hasUnsavedChanges = false;
        Logger.LogInformation("Created Snapshot (FileBytes excluded from comparison)");
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
