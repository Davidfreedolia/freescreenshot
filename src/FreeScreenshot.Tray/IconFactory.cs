using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FreeScreenshot;

/// <summary>
/// Loads the embedded app icon for the tray. We use the real .ico
/// (Resources\app.ico, generated from the Freedolia brand mark) because
/// Windows' tray needs a proper HICON — an arbitrary RenderTargetBitmap
/// converts unreliably and frequently renders as a blank/invisible icon.
/// </summary>
internal static class IconFactory
{
    private static readonly Uri IconUri =
        new("pack://application:,,,/FreeScreenshot;component/Resources/app.ico", UriKind.Absolute);

    public static ImageSource CreateTrayIcon()
    {
        // BitmapDecoder picks the best size from the multi-resolution .ico.
        var stream = Application.GetResourceStream(IconUri)?.Stream
                     ?? throw new InvalidOperationException("Embedded app.ico not found.");
        var decoder = BitmapDecoder.Create(
            stream,
            BitmapCreateOptions.PreservePixelFormat,
            BitmapCacheOption.OnLoad);
        var frame = decoder.Frames[0];
        frame.Freeze();
        return frame;
    }
}
