using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Key = System.Windows.Input.Key;
using MouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;

namespace FreeScreenshot.Capture;

/// <summary>
/// Floating Topmost image. Multiple instances auto-arrange in a row at the
/// bottom-right of the primary work area, all rendered at the same
/// thumbnail width (CleanShot-style).
/// </summary>
public partial class PinnedWindow : Window
{
    private const double ThumbWidth = 220;
    private const double Gap = 12;

    private static readonly List<PinnedWindow> _alive = new();
    private bool _moved; // becomes true once the user drags the window — stops auto-flow for this one

    public PinnedWindow(byte[] pngBytes, double placementXHint = double.NaN, double placementYHint = double.NaN)
    {
        InitializeComponent();
        using var ms = new MemoryStream(pngBytes);
        var decoder = BitmapDecoder.Create(ms, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
        var frame = decoder.Frames[0];
        frame.Freeze();
        ShotImage.Source = frame;

        // Render at fixed thumbnail width so all pinned windows share a row.
        var aspect = (double)frame.PixelHeight / frame.PixelWidth;
        ShotImage.Width  = ThumbWidth;
        ShotImage.Height = ThumbWidth * aspect;

        _alive.Add(this);
        ReflowAll();
    }

    /// <summary>Recompute positions so non-moved pinned windows form a row at bottom-right.</summary>
    private static void ReflowAll()
    {
        var work = SystemParameters.WorkArea;
        double x = work.Right - Gap;
        double y = 0; // computed per item from height
        foreach (var w in System.Linq.Enumerable.Reverse(_alive))
        {
            if (w._moved) continue;
            // Wait until size is known.
            if (double.IsNaN(w.ActualWidth) || w.ActualWidth < 1)
            {
                w.SizeChanged += OnFirstSize;
                continue;
            }
            x -= w.ActualWidth + Gap;
            y  = work.Bottom - w.ActualHeight - Gap;
            if (x < work.Left + Gap)
            {
                // Wrap to a new row above.
                x = work.Right - Gap - w.ActualWidth;
                y -= w.ActualHeight + Gap;
            }
            w.Left = x;
            w.Top  = y;
        }
    }

    private static void OnFirstSize(object? sender, SizeChangedEventArgs e)
    {
        if (sender is PinnedWindow w)
        {
            w.SizeChanged -= OnFirstSize;
            ReflowAll();
        }
    }

    private void OnDragStart(object sender, MouseButtonEventArgs e)
    {
        _moved = true;
        try { DragMove(); } catch { }
    }

    private void OnClose(object sender, RoutedEventArgs e) => Close();

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape) Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        _alive.Remove(this);
        ReflowAll();
        base.OnClosed(e);
    }
}
