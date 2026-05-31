using Microsoft.AspNetCore.Mvc;

namespace NDB.Platform.Api.Infrastructure;

/// <summary>Helper for returning a PDF file response from a controller action.</summary>
public static class PdfService
{
    /// <summary>
    /// Wraps the given PDF bytes in a <see cref="FileContentResult"/> with content-type <c>application/pdf</c>.
    /// </summary>
    /// <param name="pdfBytes">Raw PDF bytes from the renderer.</param>
    /// <param name="fileName">Download file name without extension — <c>.pdf</c> is appended automatically.</param>
    public static FileContentResult ToPdfResult(byte[] pdfBytes, string fileName)
        => new(pdfBytes, "application/pdf") { FileDownloadName = $"{fileName}.pdf" };
}
