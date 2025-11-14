# Blazorise Setup Instructions

## Step 1: Add NuGet Packages

Add the following packages to `IkeaDocuScan-Web.Client.csproj`:

```bash
cd IkeaDocuScan-Web.Client
dotnet add package Blazorise --version 1.6.1
dotnet add package Blazorise.Bootstrap5 --version 1.6.1
dotnet add package Blazorise.Icons.FontAwesome --version 1.6.1
dotnet add package Blazorise.DataGrid --version 1.6.1
```

## Step 2: Register Blazorise Services

Update `IkeaDocuScan-Web.Client/Program.cs`:

```csharp
using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Add Blazorise
builder.Services
    .AddBlazorise(options =>
    {
        options.Immediate = true;
    })
    .AddBootstrap5Providers()
    .AddFontAwesomeIcons();

await builder.Build().RunAsync();
```

## Step 3: Add CSS and JS References

Update `IkeaDocuScan-Web/Components/App.razor`:

Add to `<head>`:

```html
<!-- Bootstrap 5 CSS -->
<link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet">

<!-- Font Awesome -->
<link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css">

<!-- Blazorise CSS -->
<link href="_content/Blazorise/blazorise.css" rel="stylesheet" />
<link href="_content/Blazorise.Bootstrap5/blazorise.bootstrap5.css" rel="stylesheet" />
```

Add before `</body>`:

```html
<!-- Bootstrap 5 JS -->
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>

<!-- Blazorise JS -->
<script src="_content/Blazorise/blazorise.js"></script>
<script src="_content/Blazorise.Bootstrap5/blazorise.bootstrap5.js"></script>
```

## Step 4: Update _Imports.razor

Add to `IkeaDocuScan-Web.Client/_Imports.razor`:

```razor
@using Blazorise
@using Blazorise.DataGrid
```

## Verification

After setup, build the project:

```bash
dotnet build
```

You should see no errors related to Blazorise components.
