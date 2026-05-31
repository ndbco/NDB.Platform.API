namespace NDB.Platform.Api.Infrastructure;

/// <summary>
/// PDF renderer abstraction — converts HTML to PDF bytes.
/// The implementation is provided by the consuming project (e.g. Playwright, PuppeteerSharp).
/// </summary>
public interface IPdfRenderer
{
    /// <summary>Renders the given HTML to PDF bytes (A4, print background).</summary>
    Task<byte[]> RenderAsync(string html, CancellationToken ct = default);
}
