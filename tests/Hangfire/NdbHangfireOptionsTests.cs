using FluentAssertions;
using NDB.Platform.Api.Hangfire;
using Xunit;

namespace NDB.Platform.Api.Tests.Hangfire;

public class NdbHangfireOptionsTests
{
    [Fact]
    public void Defaults_Should_Have_Correct_Dashboard_Url()
    {
        var opts = new NdbHangfireOptions();
        opts.DashboardUrl.Should().Be("/jobs");
    }

    [Fact]
    public void Defaults_Should_Have_Admin_User()
    {
        var opts = new NdbHangfireOptions();
        opts.BasicAuthUser.Should().Be("admin");
    }

    [Fact]
    public void Defaults_Should_Have_Min_20_Workers()
    {
        var opts = new NdbHangfireOptions();
        opts.WorkerCount.Should().BeGreaterThanOrEqualTo(20);
    }

    [Fact]
    public void Defaults_Should_Have_Default_Queue()
    {
        var opts = new NdbHangfireOptions();
        opts.Queues.Should().ContainSingle().Which.Should().Be("default");
    }

    [Fact]
    public void Should_Allow_Custom_Worker_Count()
    {
        var opts = new NdbHangfireOptions { WorkerCount = 50 };
        opts.WorkerCount.Should().Be(50);
    }

    [Fact]
    public void Should_Allow_Custom_Queues()
    {
        var opts = new NdbHangfireOptions { Queues = ["high", "low"] };
        opts.Queues.Should().HaveCount(2);
        opts.Queues.Should().Contain("high").And.Contain("low");
    }
}
