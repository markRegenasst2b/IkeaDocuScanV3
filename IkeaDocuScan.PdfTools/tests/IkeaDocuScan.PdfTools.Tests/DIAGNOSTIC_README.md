# Diagnostic Tool for ExtractTextFromBytes Issue

## Overview

This diagnostic tool helps debug the issue where `ExtractTextFromBytes(byte[])` returns an empty string while `ExtractText(string filePath)` works correctly.

## Files

- **DiagnosticTests.cs** - Contains the diagnostic test that compares file path vs byte array extraction
- **DebugLogger.cs** - Simple file logger that writes detailed logs to disk

## Running the Diagnostic Test

### Option 1: Using Command Line

Navigate to the solution directory and run:

```powershell
cd IkeaDocuScan.PdfTools
dotnet test --filter "FullyQualifiedName~DiagnosticTests" --logger "console;verbosity=detailed"
```

### Option 2: Using Visual Studio Test Explorer

1. Open **Test Explorer** (Test → Test Explorer)
2. Build the solution
3. Find `DiagnosticTests` → `Diagnostic_CompareFilePathVsByteArray`
4. Right-click the test → **Run**
5. After the test completes, click on the test to view output

### Option 3: Using Rider

1. Open the **Unit Tests** window
2. Find `DiagnosticTests.Diagnostic_CompareFilePathVsByteArray`
3. Right-click → **Run**
4. Check the test output panel

## Finding the Log File

The diagnostic test creates a detailed log file in the test output directory:

**Location:**
```
IkeaDocuScan.PdfTools\tests\IkeaDocuScan.PdfTools.Tests\bin\Debug\net9.0\test-debug-YYYYMMDD-HHmmss.log
```

**Quick way to find it:**
1. The test output will print: `Test Complete - Check log file: C:\path\to\test-debug-....log`
2. Copy that path and open the file in any text editor

**Or navigate manually:**
```powershell
cd tests\IkeaDocuScan.PdfTools.Tests\bin\Debug\net9.0
dir test-debug-*.log | sort LastWriteTime -Descending | select -First 1
notepad (dir test-debug-*.log | sort LastWriteTime -Descending | select -First 1).Name
```

## Understanding the Log Output

### Successful Execution (Both Methods Work)

```
[12:34:56.789] === Test Debug Log Started at 2025-01-27 12:34:56 ===
[12:34:56.790] Log file: C:\...\test-debug-20250127-123456.log
[12:34:56.801] === Starting Diagnostic Test ===
[12:34:56.802] PDF Path: C:\...\Data\SmallPdf.pdf
[12:34:56.802] File exists: True
[12:34:56.850] Loaded 6783816 bytes from file
[12:34:56.851] PDF bytes: 6783816 bytes, first 20: 25 50 44 46 2D 31 2E 37 0D 25 E2 E3 CF D3 0D 0A 32 39 39 36
[12:34:56.852] --- Extracting from file path ---
[12:34:58.123] Result from path: 1234 characters
[12:34:58.124] First 100 chars: This is the content of the PDF document...
[12:34:58.125] --- Extracting from byte array ---
[12:34:59.456] Result from bytes: 1234 characters
[12:34:59.457] First 100 chars: This is the content of the PDF document...
[12:34:59.458] --- Comparison ---
[12:34:59.459] Are they equal? True
[12:34:59.460] Path length: 1234, Bytes length: 1234
[12:34:59.461] === Test Complete - Check log file: C:\...\test-debug-20250127-123456.log ===
```

### Failed Execution (Byte Array Returns Empty)

```
[12:34:56.789] === Test Debug Log Started at 2025-01-27 12:34:56 ===
[12:34:56.790] Log file: C:\...\test-debug-20250127-123456.log
[12:34:56.801] === Starting Diagnostic Test ===
[12:34:56.802] PDF Path: C:\...\Data\SmallPdf.pdf
[12:34:56.802] File exists: True
[12:34:56.850] Loaded 6783816 bytes from file
[12:34:56.851] PDF bytes: 6783816 bytes, first 20: 25 50 44 46 2D 31 2E 37 0D 25 E2 E3 CF D3 0D 0A 32 39 39 36
[12:34:56.852] --- Extracting from file path ---
[12:34:58.123] Result from path: 1234 characters
[12:34:58.124] First 100 chars: This is the content of the PDF document...
[12:34:58.125] --- Extracting from byte array ---
[12:34:59.456] Result from bytes: 0 characters        ⚠️ PROBLEM!
[12:34:59.457] First 100 chars:
[12:34:59.458] --- Comparison ---
[12:34:59.459] Are they equal? False
[12:34:59.460] Path length: 1234, Bytes length: 0     ⚠️ PROBLEM!
[12:34:59.461] ERROR: ExtractTextFromBytes returned empty string!
[12:34:59.462] === Test Complete - Check log file: C:\...\test-debug-20250127-123456.log ===
```

## What to Look For

### 1. Check File Loading
```
Loaded 6783816 bytes from file
PDF bytes: 6783816 bytes, first 20: 25 50 44 46 ...
```
- Bytes loaded should be > 0
- First bytes should be `25 50 44 46` (PDF magic number: %PDF)
- If 0 bytes: File loading failed

### 2. Check File Path Extraction
```
Result from path: 1234 characters
```
- Should be > 0 characters
- If 0: PDF has no extractable text or Python environment issue

### 3. Check Byte Array Extraction
```
Result from bytes: 0 characters    ⚠️ If this is 0, there's the bug!
```
- Should match the file path result
- If 0: CSnakes marshaling issue or Python function problem

### 4. Check Equality
```
Are they equal? False              ⚠️ Should be True
Path length: 1234, Bytes length: 0 ⚠️ Should match
```

## Common Issues and Solutions

### Issue 1: "SmallPdf.pdf not found"

**Symptoms:**
```
SKIP: SmallPdf.pdf not found
```

**Solution:**
Ensure SmallPdf.pdf exists in `tests\IkeaDocuScan.PdfTools.Tests\Data\SmallPdf.pdf`

### Issue 2: Both Methods Return 0 Characters

**Symptoms:**
```
Result from path: 0 characters
Result from bytes: 0 characters
```

**Cause:** Python environment or PyPDF2 issue

**Solution:**
1. Delete the .venv folder: `bin\Debug\net9.0\.venv`
2. Rebuild: `dotnet clean && dotnet build`

### Issue 3: File Path Works, Byte Array Returns 0

**Symptoms:**
```
Result from path: 1234 characters   ✓
Result from bytes: 0 characters     ✗
```

**Cause:** CSnakes byte array marshaling issue

**Possible solutions:**
1. Check CSnakes version compatibility
2. Python function may not be receiving bytes correctly
3. Check Python function signature: `def extract_text_from_bytes(pdf_bytes: bytes) -> str`

### Issue 4: "Cannot access a disposed object"

**Symptoms:**
Test fails with ObjectDisposedException

**Solution:**
1. Ensure `xunit.runner.json` is configured for sequential execution
2. Check that the host is kept alive (not disposed)
3. Run tests one at a time to confirm

## Next Steps After Running Diagnostic

1. **Run the test** using one of the methods above
2. **Locate the log file** in the bin\Debug\net9.0 directory
3. **Open the log file** in a text editor
4. **Compare** the output to the examples above
5. **Identify** which step is failing:
   - File loading?
   - File path extraction?
   - Byte array extraction?
6. **Share the log file** if you need help debugging

## Cleaning Up

To remove old log files:

```powershell
cd tests\IkeaDocuScan.PdfTools.Tests\bin\Debug\net9.0
Remove-Item test-debug-*.log
```

## Additional Debugging

If you need even more detail, you can:

1. **Add breakpoints** in DiagnosticTests.cs and step through
2. **Check Python output** by looking at the console during test execution
3. **Modify DebugLogger.Log** calls to add more diagnostic information
4. **Test with different PDFs** by changing the test to use different files

## Technical Details

### What the Test Does

1. Loads `SmallPdf.pdf` from the Data folder
2. Reads the file into a byte array
3. Logs the first 20 bytes in hexadecimal
4. Calls `PdfTextExtractor.ExtractText(filePath)`
5. Calls `PdfTextExtractor.ExtractText(pdfBytes)`
6. Compares the results
7. Asserts they should be equal

### Thread Safety

The test runs with xUnit configuration:
- `parallelizeAssembly: false`
- `parallelizeTestCollections: false`
- `maxParallelThreads: 1`

This ensures no race conditions affect the results.

### Python Marshaling

CSnakes marshals the C# `byte[]` to Python `bytes`:
- C#: `byte[] pdfBytes` (managed array)
- Python: `pdf_bytes: bytes` (Python bytes object)

The diagnostic test verifies this marshaling works correctly.

---

**Last Updated:** 2025-01-27
**Status:** Ready to use for debugging ExtractTextFromBytes issue
