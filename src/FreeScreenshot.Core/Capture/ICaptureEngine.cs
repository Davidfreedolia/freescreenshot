namespace FreeScreenshot.Core.Capture;

/// <summary>
/// Abstraction over Windows screen-capture APIs.
/// Implementations live in the UI/Tray projects so Core stays UI-free.
/// </summary>
public interface ICaptureEngine
{
    /// <summary>Capture a rectangular region of the virtual desktop.</summary>
    Task<CaptureResult> CaptureRegionAsync(Rectangle region, CancellationToken ct = default);

    /// <summary>Capture an entire monitor.</summary>
    Task<CaptureResult> CaptureMonitorAsync(int monitorIndex, CancellationToken ct = default);

    /// <summary>Capture a specific window by its native handle (HWND).</summary>
    Task<CaptureResult> CaptureWindowAsync(IntPtr hWnd, CancellationToken ct = default);
}

public readonly record struct Rectangle(int X, int Y, int Width, int Height);

public sealed record CaptureResult(
    byte[] PngBytes,
    int Width,
    int Height,
    DateTime CapturedAtUtc);
