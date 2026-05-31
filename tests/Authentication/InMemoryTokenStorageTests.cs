using FluentAssertions;
using NDB.Platform.Api.Authentication;
using Xunit;

namespace NDB.Platform.Api.Tests.Authentication;

public sealed class InMemoryTokenStorageTests
{
    [Fact]
    public void GetAccessToken_InitialState_ShouldReturnNull()
    {
        var storage = new InMemoryTokenStorage();
        storage.GetAccessToken().Should().BeNull();
    }

    [Fact]
    public void GetRefreshToken_InitialState_ShouldReturnNull()
    {
        var storage = new InMemoryTokenStorage();
        storage.GetRefreshToken().Should().BeNull();
    }

    [Fact]
    public void SetTokens_ShouldPersistBothTokens()
    {
        var storage = new InMemoryTokenStorage();
        storage.SetTokens("access-123", "refresh-456");

        storage.GetAccessToken().Should().Be("access-123");
        storage.GetRefreshToken().Should().Be("refresh-456");
    }

    [Fact]
    public void ClearTokens_ShouldResetBothTokensToNull()
    {
        var storage = new InMemoryTokenStorage();
        storage.SetTokens("access-123", "refresh-456");
        storage.ClearTokens();

        storage.GetAccessToken().Should().BeNull();
        storage.GetRefreshToken().Should().BeNull();
    }

    [Fact]
    public void SetTokens_Overwrite_ShouldUpdateBothTokens()
    {
        var storage = new InMemoryTokenStorage();
        storage.SetTokens("old-access", "old-refresh");
        storage.SetTokens("new-access", "new-refresh");

        storage.GetAccessToken().Should().Be("new-access");
        storage.GetRefreshToken().Should().Be("new-refresh");
    }

    [Fact]
    public void InMemoryTokenStorage_ShouldImplementITokenStorage()
    {
        typeof(InMemoryTokenStorage)
            .Should().Implement<NDB.Platform.Http.Resilience.ITokenStorage>();
    }

    [Fact]
    public async Task SetTokens_ConcurrentAccess_ShouldBeThreadSafe()
    {
        var storage = new InMemoryTokenStorage();
        var tasks = Enumerable.Range(0, 100).Select(i =>
            Task.Run(() => storage.SetTokens($"access-{i}", $"refresh-{i}")));

        await Task.WhenAll(tasks);

        // After all writes, should have consistent state (not null)
        storage.GetAccessToken().Should().NotBeNull();
        storage.GetRefreshToken().Should().NotBeNull();
    }
}
