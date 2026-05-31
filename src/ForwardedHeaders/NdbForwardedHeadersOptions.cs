using System.Net;
using AspNetIPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;

namespace NDB.Platform.Api.ForwardedHeaders;

/// <summary>
/// Configuration options for forwarded header processing in NDB Platform.
/// In production, you should always register known proxies or networks to prevent
/// IP spoofing via the <c>X-Forwarded-For</c> header.
/// </summary>
public sealed class NdbForwardedHeadersOptions
{
    /// <summary>
    /// List of trusted proxy IP addresses.
    /// Default: empty (ASP.NET Core defaults apply — loopback only unless cleared).
    /// </summary>
    public IList<IPAddress> KnownProxies { get; set; } = new List<IPAddress>();

    /// <summary>
    /// List of trusted CIDR networks (<see cref="AspNetIPNetwork"/>).
    /// Default: empty (ASP.NET Core defaults apply).
    /// </summary>
#pragma warning disable ASPDEPR005 // KnownNetworks is obsolete in net10 but still needed for net8 compatibility
    public IList<AspNetIPNetwork> KnownNetworks { get; set; } = new List<AspNetIPNetwork>();
#pragma warning restore ASPDEPR005

    /// <summary>
    /// Clears existing <c>KnownProxies</c> and <c>KnownNetworks</c> before applying the values from these options.
    /// Default: <c>false</c> — ASP.NET Core defaults (loopback) are preserved.
    /// Set to <c>true</c> only when you have fully populated <see cref="KnownProxies"/> and <see cref="KnownNetworks"/>.
    /// </summary>
    public bool ClearExistingKnownHosts { get; set; }
}
