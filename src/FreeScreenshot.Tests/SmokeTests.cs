using FreeScreenshot.Core.Capture;
using Xunit;

namespace FreeScreenshot.Tests;

public class SmokeTests
{
    [Fact]
    public void Rectangle_StoresDimensions()
    {
        var r = new Rectangle(10, 20, 300, 200);
        Assert.Equal(10, r.X);
        Assert.Equal(20, r.Y);
        Assert.Equal(300, r.Width);
        Assert.Equal(200, r.Height);
    }

    [Fact]
    public void CaptureResult_RoundTrip()
    {
        var bytes = new byte[] { 1, 2, 3 };
        var t = DateTime.UtcNow;
        var c = new CaptureResult(bytes, 100, 50, t);

        Assert.Equal(bytes, c.PngBytes);
        Assert.Equal(100, c.Width);
        Assert.Equal(50, c.Height);
        Assert.Equal(t, c.CapturedAtUtc);
    }
}
