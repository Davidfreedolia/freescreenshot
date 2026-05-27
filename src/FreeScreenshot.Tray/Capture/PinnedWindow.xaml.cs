using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Key = System.Windows.Input.Key;
using MouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;

namespace FreeScreenshot.Capture;

/// <summary>Borderless Topmost window that floats a captured image on screen.</summary>
public partial class PinnedWindow : Window
{
    public PinnedWindow(byte[] pngBytes, double placementX, double placementY)
    {
        InitializeComponent();
        using var ms = new MemoryStream(pngBytes);
        var decoder = BitmapDecoder.Create(ms, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
        var frame = decoder.Frames[0];
        frame.Freeze();
        ShotImage.Source = frame;
        Left = placementX;
        Top  = placementY;
    }

    private void OnDragStart(object sender, MouseButtonEventArgs e)
    {
        try { DragMove(); } catch { }
    }

    private void OnClose(object sender, RoutedEventArgs e) => Close();

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape) Close();
    }
}
