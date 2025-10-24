using Microsoft.AspNetCore.Components;
using IkeaDocuScan_Web.Client.Models;
using IkeaDocuScan.Shared.DTOs.Documents;
using IkeaDocuScan.Shared.Interfaces;
using Blazorise;
using Microsoft.JSInterop;

namespace IkeaDocuScan_Web.Client.Pages;

/// <summary>
/// Code-behind for DocumentPropertiesPage component.
/// Handles three operational modes: Edit, Register, Check-in
/// </summary>
public partial class DocumentPropertiesPage : ComponentBase
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
    private bool isSaving = false;
    private string? errorMessage;
    private string? successMessage;
    private List<string> validationErrors = new();

    // Copy/Paste state
    private bool hasCopiedData = false;
    private DateTime? copiedDataExpiration;

    // Duplicate detection
    private Modal duplicateModal = new();
    private List<int> similarDocumentBarcodes = new();
    private bool duplicateConfirmed = false;

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

        await LoadPageAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
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
        try
        {
            isLoading = true;
            errorMessage = null;
            successMessage = null;
            validationErrors.Clear();

            if (BarCode.HasValue)
            {
                // EDIT MODE
                await LoadEditModeAsync(BarCode.Value);
            }
            else if (!string.IsNullOrEmpty(FileName))
            {
                // CHECK-IN MODE
                await LoadCheckInModeAsync(FileName);
            }
            else
            {
                // REGISTER MODE
                LoadRegisterMode();
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Failed to load page: {ex.Message}";
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    private async Task LoadEditModeAsync(int barcode)
    {
        var document = await DocumentService.GetByBarCodeAsync(barcode.ToString());

        if (document == null)
        {
            errorMessage = $"Document with barcode {barcode} not found.";
            return;
        }

        Model = MapFromDto(document);
        Model.Mode = DocumentPropertyMode.Edit;
        Model.PropertySetNumber = 2; // DispatchDate enabled
    }

    private async Task LoadCheckInModeAsync(string fileName)
    {
        // TODO: Load scanned file from CheckinDirectory via ScannedFileService
        // For now, create new model
        Model = new DocumentPropertiesViewModel
        {
            Mode = DocumentPropertyMode.CheckIn,
            PropertySetNumber = 2, // DispatchDate enabled
            FileName = fileName,
            BarCode = ExtractBarcodeFromFileName(fileName)
        };

        // TODO: Load file bytes
        // Model.FileBytes = await LoadFileFromDirectory(fileName);
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

            // Post-save actions based on mode
            await HandlePostSave();
        }
        catch (Exception ex)
        {
            errorMessage = $"Failed to save document: {ex.Message}";
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
    }

    private async Task SaveRegisterOrCheckInModeAsync()
    {
        var createDto = MapToCreateDto();
        var result = await DocumentService.CreateAsync(createDto);

        Model.Id = result.Id;

        successMessage = Model.Mode == DocumentPropertyMode.Register
            ? "Successfully registered document!"
            : "Successfully checked in document!";
    }

    private async Task HandlePostSave()
    {
        duplicateConfirmed = false;

        if (Model.Mode == DocumentPropertyMode.Register)
        {
            // Stay on page, clear form for next entry, focus barcode
            var currentMode = Model.Mode;
            var currentPropertySet = Model.PropertySetNumber;
            Model = new DocumentPropertiesViewModel
            {
                Mode = currentMode,
                PropertySetNumber = currentPropertySet
            };

            await Task.Delay(100);
            await JSRuntime.InvokeVoidAsync("eval",
                "document.getElementById('barcodeInput')?.focus()");
        }
        else
        {
            // Edit and Check-in modes: close/navigate back
            await Task.Delay(2000); // Show success message briefly
            NavigationManager.NavigateTo("/documents");
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
        //     await duplicateModal.Show();
        //     return true; // Wait for user confirmation
        // }

        return false; // No duplicates
    }

    private async Task ConfirmDuplicate()
    {
        duplicateConfirmed = true;
        await duplicateModal.Hide();
        await SaveDocument(); // Retry save with confirmation
    }

    private async Task CancelDuplicate()
    {
        await duplicateModal.Hide();
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

    private void Cancel()
    {
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
            BankConfirmation = Model.BankConfirmation
        };
    }
}
