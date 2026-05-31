namespace NDB.Platform.Api.Hangfire;

/// <summary>Configuration options for the Hangfire dashboard and server.</summary>
public sealed class NdbHangfireOptions
{
    /// <summary>URL path for the Hangfire dashboard. Default: <c>/jobs</c>.</summary>
    public string DashboardUrl { get; set; } = "/jobs";

    /// <summary>Username for dashboard Basic Auth.</summary>
    public string BasicAuthUser { get; set; } = "admin";

    /// <summary>
    /// Password for dashboard Basic Auth.
    /// Must be set before deploying to production — an empty password is rejected by <see cref="Validate"/>.
    /// </summary>
    public string BasicAuthPassword { get; set; } = string.Empty;

    /// <summary>Title displayed in the Hangfire dashboard header.</summary>
    public string DashboardTitle { get; set; } = "Background Jobs";

    /// <summary>Number of concurrent background job workers. Default: <c>ProcessorCount × 5</c>, minimum 20.</summary>
    public int WorkerCount { get; set; } = Math.Max(Environment.ProcessorCount * 5, 20);

    /// <summary>Queue names to process. Default: <c>["default"]</c>.</summary>
    public string[] Queues { get; set; } = ["default"];

    /// <summary>
    /// Validates the Hangfire configuration. Called by <see cref="HangfireExtensions.UseNdbHangfireDashboard"/> before registering the dashboard.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if <see cref="BasicAuthUser"/> or <see cref="BasicAuthPassword"/> is empty.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(BasicAuthUser))
            throw new InvalidOperationException(
                "NdbHangfireOptions.BasicAuthUser is required to secure the Hangfire dashboard.");

        if (string.IsNullOrEmpty(BasicAuthPassword))
            throw new InvalidOperationException(
                "NdbHangfireOptions.BasicAuthPassword is required. " +
                "Do not expose the Hangfire dashboard without a password.");
    }
}
