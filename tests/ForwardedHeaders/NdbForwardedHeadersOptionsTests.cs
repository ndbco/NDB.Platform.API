using FluentAssertions;
using NDB.Platform.Api.ForwardedHeaders;
using System.Net;
using Xunit;

namespace NDB.Platform.Api.Tests.ForwardedHeaders;

// ── FIX 7 (C-12): NdbForwardedHeadersOptions tests ──

public sealed class NdbForwardedHeadersOptionsTests
{
    [Fact]
    public void Default_KnownProxies_Should_Be_Empty()
    {
        var opts = new NdbForwardedHeadersOptions();
        opts.KnownProxies.Should().BeEmpty();
    }

    [Fact]
    public void Default_KnownNetworks_Should_Be_Empty()
    {
        var opts = new NdbForwardedHeadersOptions();
        opts.KnownNetworks.Should().BeEmpty();
    }

    [Fact]
    public void Default_ClearExistingKnownHosts_Should_Be_False()
    {
        // Default MUST NOT clear ASP.NET defaults (loopback)
        var opts = new NdbForwardedHeadersOptions();
        opts.ClearExistingKnownHosts.Should().BeFalse();
    }

    [Fact]
    public void Should_Accept_KnownProxy_IPAddress()
    {
        var opts = new NdbForwardedHeadersOptions();
        opts.KnownProxies.Add(IPAddress.Parse("10.0.0.1"));
        opts.KnownProxies.Should().ContainSingle()
            .Which.Should().Be(IPAddress.Parse("10.0.0.1"));
    }

    [Fact]
    public void Should_Accept_Multiple_KnownProxies()
    {
        var opts = new NdbForwardedHeadersOptions
        {
            KnownProxies = new List<IPAddress>
            {
                IPAddress.Parse("10.0.0.1"),
                IPAddress.Parse("10.0.0.2"),
                IPAddress.Parse("192.168.1.1")
            }
        };

        opts.KnownProxies.Should().HaveCount(3);
    }
}
