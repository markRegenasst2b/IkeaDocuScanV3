using IkeaDocuScan.Shared.Interfaces;
using IkeaDocuScan.Shared.DTOs.ScannedFiles;

namespace IkeaDocuScan_Web.Endpoints;

public static class ScannedFileEndpoints
{
    public static void MapScannedFileEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/scannedfiles")
            .RequireAuthorization("HasAccess")
            .WithTags("ScannedFiles");

        group.MapGet("/", async (IScannedFileService service) =>
        {
            var files = await service.GetScannedFilesAsync();
            return Results.Ok(files);
        })
        .WithName("GetAllScannedFiles")
        .Produces<List<ScannedFileDto>>(200);

        group.MapGet("/{fileName}", async (string fileName, IScannedFileService service) =>
        {
            var file = await service.GetFileByNameAsync(fileName);
            if (file == null)
                return Results.NotFound(new { error = $"File '{fileName}' not found" });

            return Results.Ok(file);
        })
        .WithName("GetScannedFileByName")
        .Produces<ScannedFileDto>(200)
        .Produces(404);

        group.MapGet("/{fileName}/content", async (string fileName, IScannedFileService service) =>
        {
            var content = await service.GetFileContentAsync(fileName);
            if (content == null)
                return Results.NotFound(new { error = $"File content for '{fileName}' not found" });

            // Determine content type based on extension
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            var contentType = extension switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".tif" or ".tiff" => "image/tiff",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => "application/octet-stream"
            };

            return Results.File(content, contentType, fileName);
        })
        .WithName("GetScannedFileContent")
        .Produces(200)
        .Produces(404);

        group.MapGet("/{fileName}/exists", async (string fileName, IScannedFileService service) =>
        {
            var exists = await service.FileExistsAsync(fileName);
            return Results.Ok(exists);
        })
        .WithName("CheckScannedFileExists")
        .Produces<bool>(200);

        group.MapGet("/{fileName}/stream", async (string fileName, IScannedFileService service) =>
        {
            var stream = await service.GetFileStreamAsync(fileName);
            if (stream == null)
                return Results.NotFound(new { error = $"File stream for '{fileName}' not found" });

            // Determine content type based on extension
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            var contentType = extension switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".tif" or ".tiff" => "image/tiff",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => "application/octet-stream"
            };

            // Don't pass fileName to display inline instead of triggering download
            return Results.Stream(stream, contentType, fileDownloadName: null, enableRangeProcessing: true);
        })
        .WithName("GetScannedFileStream")
        .Produces(200)
        .Produces(404);

        group.MapDelete("/{fileName}", async (string fileName, IScannedFileService service) =>
        {
            try
            {
                var deleted = await service.DeleteFileAsync(fileName);
                if (deleted)
                {
                    return Results.Ok(new { message = $"File '{fileName}' deleted successfully" });
                }

                return Results.NotFound(new { error = $"File '{fileName}' not found" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Access Denied",
                    detail: ex.Message);
            }
            catch (IOException ex)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "File Deletion Error",
                    detail: ex.Message);
            }
        })
        .WithName("DeleteScannedFile")
        .Produces(200)
        .Produces(404)
        .Produces(403)
        .Produces(500);
    }
}
