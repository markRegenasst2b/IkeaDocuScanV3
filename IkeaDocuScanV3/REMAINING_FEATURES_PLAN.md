# Document Properties Migration - Remaining Features Plan

**Date:** 2025-01-24
**Current Progress:** 70% Complete
**Remaining Work:** 30% (Estimated 12-15 hours)

---

## 📊 Remaining Features Breakdown

### High Priority Features (Must Have for MVP)

**Total Estimated Time:** 8-10 hours

#### **1. Load DocumentNames Service (2 hours)**

**Status:** ⏳ Pending
**Priority:** High
**Impact:** Medium

**Current State:**
- DocumentName dropdown in AdditionalInfoFields is empty
- Hardcoded placeholder: `@* TODO: Load DocumentNames filtered by DocumentTypeId *@`

**Implementation:**

1. **Check if service exists:**
   ```bash
   # Search for IDocumentNameService
   grep -r "IDocumentNameService" IkeaDocuScan.Shared/Interfaces/
   ```

2. **If missing, create interface:**
   ```csharp
   // File: /IkeaDocuScan.Shared/Interfaces/IDocumentNameService.cs
   public interface IDocumentNameService
   {
       Task<List<DocumentNameDto>> GetAllAsync();
       Task<List<DocumentNameDto>> GetByDocumentTypeIdAsync(int documentTypeId);
       Task<DocumentNameDto?> GetByIdAsync(int id);
   }
   ```

3. **Create DTO if missing:**
   ```csharp
   // File: /IkeaDocuScan.Shared/DTOs/DocumentNames/DocumentNameDto.cs
   public class DocumentNameDto
   {
       public int Id { get; set; }
       public string Name { get; set; } = string.Empty;
       public int? DocumentTypeId { get; set; }
       public string? DocumentTypeName { get; set; }
   }
   ```

4. **Create server service:**
   ```csharp
   // File: /IkeaDocuScan-Web/Services/DocumentNameService.cs
   public class DocumentNameService : IDocumentNameService
   {
       private readonly AppDbContext _context;

       public async Task<List<DocumentNameDto>> GetByDocumentTypeIdAsync(int documentTypeId)
       {
           return await _context.DocumentNames
               .Where(dn => dn.DocumentTypeId == documentTypeId)
               .Select(dn => new DocumentNameDto
               {
                   Id = dn.Id,
                   Name = dn.Name,
                   DocumentTypeId = dn.DocumentTypeId
               })
               .OrderBy(dn => dn.Name)
               .ToListAsync();
       }
   }
   ```

5. **Create endpoints:**
   ```csharp
   // File: /IkeaDocuScan-Web/Endpoints/DocumentNameEndpoints.cs
   public static class DocumentNameEndpoints
   {
       public static void MapDocumentNameEndpoints(this IEndpointRouteBuilder routes)
       {
           var group = routes.MapGroup("/api/documentnames")
               .RequireAuthorization()
               .WithTags("DocumentNames");

           group.MapGet("/", async (IDocumentNameService service) =>
               Results.Ok(await service.GetAllAsync()));

           group.MapGet("/bytype/{documentTypeId}", async (int documentTypeId, IDocumentNameService service) =>
               Results.Ok(await service.GetByDocumentTypeIdAsync(documentTypeId)));
       }
   }
   ```

6. **Create HTTP service:**
   ```csharp
   // File: /IkeaDocuScan-Web.Client/Services/DocumentNameHttpService.cs
   public class DocumentNameHttpService : IDocumentNameService
   {
       private readonly HttpClient _http;

       public async Task<List<DocumentNameDto>> GetByDocumentTypeIdAsync(int documentTypeId)
       {
           return await _http.GetFromJsonAsync<List<DocumentNameDto>>(
               $"/api/documentnames/bytype/{documentTypeId}") ?? new();
       }
   }
   ```

7. **Register in Program.cs:**
   ```csharp
   // Server Program.cs
   builder.Services.AddScoped<IDocumentNameService, DocumentNameService>();
   app.MapDocumentNameEndpoints();

   // Client Program.cs
   builder.Services.AddScoped<IDocumentNameService, DocumentNameHttpService>();
   ```

8. **Update AdditionalInfoFields.razor:**
   ```razor
   @inject IDocumentNameService DocumentNameService

   @code {
       private List<DocumentNameDto> documentNames = new();

       protected override async Task OnInitializedAsync()
       {
           await LoadDocumentNames();
       }

       protected override async Task OnParametersSetAsync()
       {
           if (Model.DocumentTypeId.HasValue)
               await LoadDocumentNames();
       }

       private async Task LoadDocumentNames()
       {
           if (Model.DocumentTypeId.HasValue)
           {
               documentNames = await DocumentNameService
                   .GetByDocumentTypeIdAsync(Model.DocumentTypeId.Value);
           }
           else
           {
               documentNames = new();
           }
       }
   }

   @* In the markup *@
   <Select TValue="int?" @bind-SelectedValue="@Model.DocumentNameId" Disabled="@IsReadOnly">
       <SelectItem Value="@((int?)null)">-- Select Document Name --</SelectItem>
       @foreach (var dn in documentNames)
       {
           <SelectItem Value="@dn.Id">@dn.Name</SelectItem>
       }
   </Select>
   ```

**Testing:**
- Document Type selected → DocumentName dropdown filters
- Correct names appear for selected type
- Clears when Document Type changes

---

#### **2. Load Currencies Service (1.5 hours)**

**Status:** ⏳ Pending
**Priority:** High
**Impact:** Low

**Current State:**
- Only 3 hardcoded currencies: USD, EUR, GBP
- Hardcoded in AdditionalInfoFields.razor line 99-101

**Implementation:**

1. **Check if service exists:**
   ```bash
   grep -r "ICurrencyService" IkeaDocuScan.Shared/Interfaces/
   ```

2. **Create interface if missing:**
   ```csharp
   // File: /IkeaDocuScan.Shared/Interfaces/ICurrencyService.cs
   public interface ICurrencyService
   {
       Task<List<CurrencyDto>> GetAllAsync();
       Task<CurrencyDto?> GetByCodeAsync(string code);
   }
   ```

3. **Create DTO if missing:**
   ```csharp
   // File: /IkeaDocuScan.Shared/DTOs/Currencies/CurrencyDto.cs
   public class CurrencyDto
   {
       public string CurrencyCode { get; set; } = string.Empty;
       public string Name { get; set; } = string.Empty;
       public int DecimalPlaces { get; set; }
   }
   ```

4. **Create server service:**
   ```csharp
   // File: /IkeaDocuScan-Web/Services/CurrencyService.cs
   public class CurrencyService : ICurrencyService
   {
       private readonly AppDbContext _context;

       public async Task<List<CurrencyDto>> GetAllAsync()
       {
           return await _context.Currencies
               .Select(c => new CurrencyDto
               {
                   CurrencyCode = c.CurrencyCode,
                   Name = c.Name,
                   DecimalPlaces = c.DecimalPlaces
               })
               .OrderBy(c => c.CurrencyCode)
               .ToListAsync();
       }
   }
   ```

5. **Create endpoints:**
   ```csharp
   // File: /IkeaDocuScan-Web/Endpoints/CurrencyEndpoints.cs
   public static class CurrencyEndpoints
   {
       public static void MapCurrencyEndpoints(this IEndpointRouteBuilder routes)
       {
           var group = routes.MapGroup("/api/currencies")
               .RequireAuthorization()
               .WithTags("Currencies");

           group.MapGet("/", async (ICurrencyService service) =>
               Results.Ok(await service.GetAllAsync()));
       }
   }
   ```

6. **Create HTTP service and register**

7. **Update AdditionalInfoFields.razor:**
   ```razor
   @inject ICurrencyService CurrencyService

   @code {
       private List<CurrencyDto> currencies = new();

       protected override async Task OnInitializedAsync()
       {
           currencies = await CurrencyService.GetAllAsync();
       }
   }

   @* Replace hardcoded currencies *@
   <Select TValue="string?" @bind-SelectedValue="@Model.CurrencyCode" Disabled="@IsReadOnly">
       <SelectItem Value="@((string?)null)">-- Select Currency --</SelectItem>
       @foreach (var currency in currencies)
       {
           <SelectItem Value="@currency.CurrencyCode">
               @currency.CurrencyCode - @currency.Name
           </SelectItem>
       }
   </Select>
   ```

**Testing:**
- All currencies from database appear in dropdown
- Display format: "USD - United States Dollar"

---

#### **3. Implement Duplicate Detection Backend (2 hours)**

**Status:** ⏳ Pending (UI exists, backend missing)
**Priority:** High
**Impact:** High

**Current State:**
- Modal dialog exists in DocumentPropertiesPage.razor
- Commented out in code: `// TODO: Implement GetSimilarDocuments`
- Lines 266-274 in DocumentPropertiesPage.razor.cs

**Implementation:**

1. **Add method to IDocumentService:**
   ```csharp
   // File: /IkeaDocuScan.Shared/Interfaces/IDocumentService.cs
   Task<List<int>> GetSimilarDocumentsAsync(int? documentTypeId, string documentNo, string versionNo);
   ```

2. **Implement in DocumentService:**
   ```csharp
   // File: /IkeaDocuScan-Web/Services/DocumentService.cs
   public async Task<List<int>> GetSimilarDocumentsAsync(
       int? documentTypeId,
       string documentNo,
       string versionNo)
   {
       var query = _context.Documents.AsQueryable();

       if (documentTypeId.HasValue)
           query = query.Where(d => d.DocumentTypeId == documentTypeId.Value);

       return await query
           .Where(d => d.DocumentNo == documentNo && d.VersionNo == versionNo)
           .Select(d => d.BarCode ?? 0)
           .Take(5) // Max 5 as per spec
           .ToListAsync();
   }
   ```

3. **Add endpoint:**
   ```csharp
   // File: /IkeaDocuScan-Web/Endpoints/DocumentEndpoints.cs
   group.MapGet("/similar", async (
       int? documentTypeId,
       string documentNo,
       string versionNo,
       IDocumentService service) =>
   {
       var barcodes = await service.GetSimilarDocumentsAsync(documentTypeId, documentNo, versionNo);
       return Results.Ok(barcodes);
   })
   .WithName("GetSimilarDocuments");
   ```

4. **Implement in HTTP service:**
   ```csharp
   // File: /IkeaDocuScan-Web.Client/Services/DocumentHttpService.cs
   public async Task<List<int>> GetSimilarDocumentsAsync(
       int? documentTypeId,
       string documentNo,
       string versionNo)
   {
       var query = $"/api/documents/similar?documentNo={documentNo}&versionNo={versionNo}";
       if (documentTypeId.HasValue)
           query += $"&documentTypeId={documentTypeId}";

       return await _http.GetFromJsonAsync<List<int>>(query) ?? new();
   }
   ```

5. **Uncomment in DocumentPropertiesPage.razor.cs:**
   ```csharp
   private async Task<bool> CheckForDuplicates()
   {
       if (string.IsNullOrEmpty(Model.DocumentNo) || string.IsNullOrEmpty(Model.VersionNo))
           return false;

       var similar = await DocumentService.GetSimilarDocumentsAsync(
           Model.DocumentTypeId, Model.DocumentNo, Model.VersionNo);

       if (similar.Any())
       {
           similarDocumentBarcodes = similar;
           await duplicateModal.Show();
           return true; // Wait for user confirmation
       }

       return false; // No duplicates
   }
   ```

**Testing:**
- Create document with DocumentNo="TEST001", VersionNo="1.0"
- Try to create another with same DocumentNo and VersionNo
- Modal appears showing existing barcode
- User can confirm to proceed or cancel

---

#### **4. Add File Upload to Check-in Mode (3 hours)**

**Status:** ⏳ Pending
**Priority:** High (for Check-in mode)
**Impact:** High

**Current State:**
- FileUploadButton component exists but not integrated
- Lines 138-146 in DocumentPropertiesPage.razor.cs: `// TODO: Load scanned file from CheckinDirectory`

**Implementation:**

1. **Add to DocumentPropertiesPage.razor header section:**
   ```razor
   @* In Check-in and Register modes, show file upload *@
   @if (Model.Mode == DocumentPropertyMode.CheckIn || Model.Mode == DocumentPropertyMode.Register)
   {
       <Row MarginBottom="Margin.Is3">
           <Column>
               <Card>
                   <CardHeader>
                       <CardTitle>File Attachment</CardTitle>
                   </CardHeader>
                   <CardBody>
                       <FileUploadButton Label="Upload PDF Document"
                                         @bind-FileBytes="@Model.FileBytes"
                                         @bind-FileName="@uploadedFileName"
                                         AllowedExtensions=".pdf"
                                         MaxFileSizeBytes="52428800"
                                         Disabled="@(Model.Mode == DocumentPropertyMode.CheckIn && Model.HasFile)" />

                       @if (Model.Mode == DocumentPropertyMode.CheckIn)
                       {
                           <Small TextColor="TextColor.Info">
                               File will be loaded from: @Model.SourceFilePath
                           </Small>
                       }
                   </CardBody>
               </Card>
           </Column>
       </Row>
   }
   ```

2. **Update code-behind to load file from CheckinDirectory:**
   ```csharp
   // In DocumentPropertiesPage.razor.cs

   private string? uploadedFileName;

   private async Task LoadCheckInModeAsync(string fileName)
   {
       // Extract barcode from filename
       Model.BarCode = ExtractBarcodeFromFileName(fileName);
       Model.FileName = fileName;
       Model.Mode = DocumentPropertyMode.CheckIn;
       Model.PropertySetNumber = 2;

       // Load file bytes from CheckinDirectory via API
       try
       {
           var fileResponse = await Http.GetAsync($"/api/scannedfiles/content/{fileName}");
           if (fileResponse.IsSuccessStatusCode)
           {
               Model.FileBytes = await fileResponse.Content.ReadAsByteArrayAsync();
               Model.SourceFilePath = fileName; // For deletion after save
               Logger.LogInformation("Loaded file from CheckinDirectory: {FileName}, {Size} bytes",
                   fileName, Model.FileBytes.Length);
           }
       }
       catch (Exception ex)
       {
           Logger.LogError(ex, "Error loading file from CheckinDirectory: {FileName}", fileName);
           errorMessage = $"Could not load file: {ex.Message}";
       }
   }
   ```

3. **Update SaveDocument to attach file:**
   ```csharp
   private async Task SaveRegisterOrCheckInModeAsync()
   {
       var createDto = MapToCreateDto();

       // Attach file if present
       if (Model.FileBytes != null && Model.FileBytes.Length > 0)
       {
           // Create DocumentFile record
           createDto.FileId = await CreateDocumentFile(Model.FileBytes, Model.FileName ?? $"{Model.BarCode}.pdf");
       }

       var result = await DocumentService.CreateAsync(createDto);
       Model.Id = result.Id;

       // Delete file from CheckinDirectory if check-in mode
       if (Model.Mode == DocumentPropertyMode.CheckIn && !string.IsNullOrEmpty(Model.SourceFilePath))
       {
           try
           {
               await Http.DeleteAsync($"/api/scannedfiles/{Model.SourceFilePath}");
               Logger.LogInformation("Deleted file from CheckinDirectory: {FileName}", Model.SourceFilePath);
           }
           catch (Exception ex)
           {
               Logger.LogWarning(ex, "Could not delete file from CheckinDirectory: {FileName}", Model.SourceFilePath);
           }
       }

       successMessage = Model.Mode == DocumentPropertyMode.Register
           ? "Successfully registered document!"
           : "Successfully checked in document!";
   }

   private async Task<int?> CreateDocumentFile(byte[] bytes, string fileName)
   {
       // TODO: Implement DocumentFile creation endpoint
       // For now, return null and let service handle it
       return null;
   }
   ```

4. **Update ScannedFileEndpoints to support content retrieval:**
   ```csharp
   // File: /IkeaDocuScan-Web/Endpoints/ScannedFileEndpoints.cs

   group.MapGet("/content/{fileName}", async (string fileName, IScannedFileService service) =>
   {
       var bytes = await service.GetFileContentAsync(fileName);
       if (bytes == null)
           return Results.NotFound();

       return Results.File(bytes, "application/pdf", fileName);
   });

   group.MapDelete("/{fileName}", async (string fileName, IScannedFileService service) =>
   {
       await service.DeleteFileAsync(fileName);
       return Results.NoContent();
   });
   ```

**Testing:**
- Navigate to `/documents/checkin/12345.pdf`
- File loads automatically from CheckinDirectory
- Can see file size indicator
- Save creates document
- File deleted from CheckinDirectory after save
- Manual upload works in Register mode

---

### Medium Priority Features (Nice to Have)

**Total Estimated Time:** 4-5 hours

#### **5. Dynamic Field Visibility (4-5 hours)**

**Status:** ⏳ Pending
**Priority:** Medium
**Impact:** High (but can work without it)

**Current State:**
- All fields always visible
- DocumentType entity has field configuration (M/O/N codes)
- No service to read and apply configuration

**Implementation:**

1. **Create FieldVisibilityService:**
   ```csharp
   // File: /IkeaDocuScan-Web.Client/Services/FieldVisibilityService.cs
   public class FieldVisibilityService
   {
       public Dictionary<string, FieldVisibility> GetFieldConfig(DocumentTypeDto docType)
       {
           // Map DocumentType properties to FieldVisibility
           // This would require DocumentType to include field config properties
           // which might not be in the DTO yet

           var config = new Dictionary<string, FieldVisibility>();

           // Example (would need actual properties from DocumentType):
           // config["SendingOutDate"] = MapToFieldVisibility(docType.SendingOutDateConfig);
           // config["ForwardedToSignatoriesDate"] = MapToFieldVisibility(docType.ForwardedToSignatoriesDateConfig);
           // etc...

           return config;
       }

       private FieldVisibility MapToFieldVisibility(string? code)
       {
           return code switch
           {
               "M" => FieldVisibility.Mandatory,
               "O" => FieldVisibility.Optional,
               "N" => FieldVisibility.NotApplicable,
               _ => FieldVisibility.Optional
           };
       }
   }
   ```

2. **Update DocumentTypeDto to include field configs:**
   ```csharp
   // File: /IkeaDocuScan.Shared/DTOs/DocumentTypes/DocumentTypeDto.cs
   public class DocumentTypeDto
   {
       public int DtId { get; set; }
       public string DtName { get; set; } = string.Empty;
       public bool? IsEnabled { get; set; }

       // Add field configuration properties
       public string? SendingOutDateConfig { get; set; }
       public string? ForwardedToSignatoriesDateConfig { get; set; }
       public string? DispatchDateConfig { get; set; }
       // ... all other fields
   }
   ```

3. **Update DocumentSectionFields to apply visibility:**
   ```csharp
   @inject FieldVisibilityService VisibilityService

   @code {
       private async Task OnDocumentTypeChanged()
       {
           if (!Model.DocumentTypeId.HasValue)
           {
               Model.FieldConfig.Clear();
               return;
           }

           var docType = documentTypes.FirstOrDefault(dt => dt.DtId == Model.DocumentTypeId);
           if (docType != null)
           {
               Model.FieldConfig = VisibilityService.GetFieldConfig(docType);
               ApplyFieldVisibility();
           }
       }

       private void ApplyFieldVisibility()
       {
           // Clear values for NA fields
           foreach (var kvp in Model.FieldConfig.Where(c => c.Value == FieldVisibility.NotApplicable))
           {
               // Clear field value based on field name
               // This would need reflection or a switch statement
           }
       }
   }
   ```

4. **Update field rendering to check visibility:**
   ```razor
   @{
       var sendingOutVisibility = Model.FieldConfig.GetValueOrDefault("SendingOutDate", FieldVisibility.Mandatory);
       var isRequired = sendingOutVisibility == FieldVisibility.Mandatory;
       var isDisabled = sendingOutVisibility == FieldVisibility.NotApplicable || IsReadOnly;
   }

   <Field>
       <FieldLabel>
           Sending Out Date
           @if (isRequired) { <span class="required">*</span> }
       </FieldLabel>
       <FieldBody>
           <DocumentDatePicker @bind-Value="@Model.SendingOutDate"
                              Disabled="@isDisabled"
                              Label="Sending Out Date" />
       </FieldBody>
   </Field>
   ```

**Note:** This is complex and requires significant refactoring. Can be deferred to post-MVP.

---

### Low Priority Features (Can Be Deferred)

**Total Estimated Time:** 3-4 hours

#### **6. Comprehensive Styling (3-4 hours)**

**Status:** ⏳ Pending
**Priority:** Low
**Impact:** Medium (cosmetic only)

**Tasks:**
- Create `/wwwroot/css/document-properties.css`
- Match original Tahoma 11px font
- Match gray borders and light gray backgrounds
- Match field widths (280px standard)
- Match button styling
- Match validation error styling (red text)
- Add responsive breakpoints

**Can use existing Blazorise styling for now.**

---

#### **7. SignalR Real-Time Updates (1 hour)**

**Status:** ⏳ Pending
**Priority:** Low
**Impact:** Low (nice to have)

**Implementation:**
```csharp
// In DocumentPropertiesPage.razor.cs OnInitializedAsync

protected override async Task OnInitializedAsync()
{
    await LoadPageAsync();

    // Subscribe to document updates
    if (_hubConnection != null)
    {
        _hubConnection.On<DocumentDto>("DocumentUpdated", async (updatedDoc) =>
        {
            if (updatedDoc.BarCode == Model.BarCode)
            {
                await ShowNotification("This document was updated by another user. Refresh to see changes.");
            }
        });
    }
}
```

**Note:** HubConnection needs to be injected. Check if already configured.

---

## 🎯 Recommended Implementation Order

### Week 1 (8-10 hours)
1. **Day 1-2:** Load DocumentNames and Currencies (3.5 hours)
2. **Day 3:** Implement Duplicate Detection backend (2 hours)
3. **Day 4-5:** Add File Upload functionality (3 hours)
4. **Testing:** All high-priority features (0.5 hours)

### Week 2 (Optional - 4-5 hours)
5. **Day 1-3:** Dynamic Field Visibility (4-5 hours) if time permits

### Post-MVP (Optional - 4-5 hours)
6. Comprehensive Styling (3-4 hours)
7. SignalR Real-Time Updates (1 hour)

---

## 📊 Feature Priority Matrix

| Feature | Priority | Impact | Effort | Status |
|---------|----------|--------|--------|--------|
| DocumentNames Loading | High | Medium | 2h | ⏳ Pending |
| Currencies Loading | High | Low | 1.5h | ⏳ Pending |
| Duplicate Detection | High | High | 2h | ⏳ Pending |
| File Upload | High | High | 3h | ⏳ Pending |
| Dynamic Visibility | Medium | High | 4-5h | ⏳ Deferred |
| Styling | Low | Medium | 3-4h | ⏳ Deferred |
| SignalR Updates | Low | Low | 1h | ⏳ Deferred |

---

## 💡 Quick Wins (Can Do Today)

### 1. DocumentNames (2 hours)
- Highest value-to-effort ratio
- Completes the AdditionalInfo section
- Users can select proper document names

### 2. Currencies (1.5 hours)
- Quick implementation
- Completes currency dropdown
- Better than hardcoded list

### 3. Duplicate Detection (2 hours)
- Critical for data integrity
- Prevents duplicate entries
- Modal already built, just needs backend

**Total Quick Wins: 5.5 hours = MVP Feature Complete!**

---

## 🚀 After These Features: 100% MVP

With the high-priority features complete, you'll have:

✅ All 40+ fields functional
✅ All 3 modes working (Edit/Register/Check-in)
✅ All dropdowns populated from database
✅ ThirdPartySelector dual-listbox
✅ File upload/check-in
✅ Duplicate detection
✅ Data saves/loads correctly
✅ Validation complete
✅ NEW date fields integrated

**Feature Complete: 100%**
**Production Ready: 95%** (styling deferred)

---

## 📝 Implementation Checklist

### DocumentNames Service
- [ ] Create IDocumentNameService interface
- [ ] Create DocumentNameDto
- [ ] Create DocumentNameService
- [ ] Create DocumentNameEndpoints
- [ ] Create DocumentNameHttpService
- [ ] Register services in Program.cs
- [ ] Update AdditionalInfoFields component
- [ ] Test dropdown population
- [ ] Test filtering by DocumentType

### Currencies Service
- [ ] Create ICurrencyService interface (if missing)
- [ ] Create CurrencyDto (if missing)
- [ ] Create CurrencyService
- [ ] Create CurrencyEndpoints
- [ ] Create CurrencyHttpService
- [ ] Register services in Program.cs
- [ ] Update AdditionalInfoFields component
- [ ] Test dropdown population
- [ ] Verify decimal places validation

### Duplicate Detection
- [ ] Add GetSimilarDocumentsAsync to IDocumentService
- [ ] Implement in DocumentService
- [ ] Add endpoint to DocumentEndpoints
- [ ] Implement in DocumentHttpService
- [ ] Uncomment CheckForDuplicates in page
- [ ] Test duplicate detection flow
- [ ] Test confirmation modal
- [ ] Test cancel/proceed actions

### File Upload
- [ ] Add FileUploadButton to page header
- [ ] Implement LoadCheckInModeAsync file loading
- [ ] Update SaveRegisterOrCheckInModeAsync
- [ ] Add ScannedFile content endpoint
- [ ] Add ScannedFile delete endpoint
- [ ] Test manual upload in Register mode
- [ ] Test auto-load in Check-in mode
- [ ] Test file deletion after check-in

---

## 🎯 Success Metrics

After completing high-priority features:

**Functionality:**
- 100% of spec features implemented
- All CRUD operations working
- All 3 modes functional

**Quality:**
- Zero critical bugs
- < 2 second page load
- < 1 second save operation
- All validation working

**User Experience:**
- All dropdowns populated
- Intuitive workflow
- Clear error messages
- Responsive layout

---

## 📅 Estimated Timeline

**Conservative Estimate:** 12-15 hours total

**Breakdown:**
- High Priority (Must Have): 8-10 hours
- Medium Priority (Nice to Have): 4-5 hours
- Low Priority (Can Defer): 3-4 hours

**Aggressive Timeline:**
- High Priority Only: 1-2 days (full-time work)
- All Features: 2-3 days (full-time work)

**Part-Time Schedule:**
- Week 1: High Priority features
- Week 2: Medium Priority if time
- Post-MVP: Low Priority polish

---

## 🎉 The Home Stretch!

You're **70% complete** with a solid, working foundation.

The remaining **30%** is mostly:
- Creating a few more services (DocumentNames, Currencies)
- Implementing duplicate detection backend
- Adding file upload UI integration
- Optional polish features

**You've built the hard parts:**
- Complete form architecture ✅
- All 40+ fields ✅
- ThirdPartySelector dual-listbox ✅
- Counterparty auto-cascade ✅
- Copy/Paste ✅
- Validation framework ✅
- Three operational modes ✅

**The remaining work is straightforward CRUD operations!**

---

**Ready to implement? Start with the Quick Wins!**

**Next Steps:**
1. Build and test current implementation
2. Fix any bugs found
3. Implement DocumentNames service (2 hours)
4. Implement Currencies service (1.5 hours)
5. Implement Duplicate Detection (2 hours)
6. **MVP Complete!** 🎉

---

**Document Status:** Ready for Final Push
**Last Updated:** 2025-01-24
**Estimated Completion:** 2-3 days (part-time) or 1-2 days (full-time)
