using System.Net;
using System.Net.Http;
using System.Text;
using FreeScreenshot.Core.Config;
using FreeScreenshot.Core.Telemetry;
using Xunit;

namespace FreeScreenshot.Tests;

public class TelemetryTests
{
    [Fact]
    public void AppConfig_EnsureInstallId_GeneratesAValidGuid()
    {
        var cfg = new AppConfig();
        var id = cfg.EnsureInstallId();
        Assert.False(string.IsNullOrWhiteSpace(id));
        Assert.True(Guid.TryParse(id, out _));
        // Calling again returns the same value.
        Assert.Equal(id, cfg.EnsureInstallId());
    }

    [Fact]
    public async Task TelemetryClient_DoesNotCallNetwork_WhenOptedOut()
    {
        var cfg = new AppConfig { TrackingOptedIn = false, InstallId = Guid.NewGuid().ToString() };
        var handler = new ThrowingHandler();
        var http = new HttpClient(handler);
        var sut = new TelemetryClient(cfg, http);

        // Should not throw even though the handler would.
        await sut.TryReportInstallAsync("0.0.1", "ca", "Windows 11");
        Assert.Equal(0, handler.Calls);
    }

    [Fact]
    public async Task TelemetryClient_NeverThrows_OnNetworkErrors()
    {
        var cfg = new AppConfig { TrackingOptedIn = true, InstallId = Guid.NewGuid().ToString() };
        var http = new HttpClient(new ThrowingHandler());
        var sut = new TelemetryClient(cfg, http);

        // Each call must swallow exceptions.
        await sut.TryReportInstallAsync("0.0.1", "ca", "Windows 11");
        var latest = await sut.TryGetLatestAsync();
        Assert.Null(latest);
        await sut.TryReportUninstallAsync(new[] { "bugs" }, "test", "0.0.1", TimeSpan.FromMilliseconds(50));
    }

    private sealed class ThrowingHandler : HttpMessageHandler
    {
        public int Calls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref Calls);
            throw new HttpRequestException("simulated network failure");
        }
    }
}
