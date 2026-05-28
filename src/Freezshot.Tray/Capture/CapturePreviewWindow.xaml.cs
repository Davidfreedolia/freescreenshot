using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace Freezshot.Capture;

/// <summary>
/// macOS-style preview shelf: a small thumbnail of the just-saved capture
/// appears in the bottom-right and auto-dismisses after a few seconds. Click
/// to open the file. Hover to pause the timer. Stacks vertically when
/// multiple previews are alive.
/// </summary>
public partial class CapturePreviewWindow : Window
{
    private const double ThumbWidth = 220;
    private const double Gap = 12;
    private const int LingerMs = 5000;

    private static readonly List<CapturePreviewWindow> _alive = new();

    private readonly string _filePath;
    private readonly DispatcherTimer _timer;

    public CapturePreviewWindow(string filePath, byte[] pngBytes)
    {
        InitializeComponent();
        _filePath = filePath;

        using var ms = new MemoryStream(pngBytes);
        var decoder = BitmapDecoder.Create(ms, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
        var frame = decoder.Frames[0];
        frame.Freeze();
        var aspect = (double)frame.PixelHeight / frame.PixelWidth;
        ShotImage.Source = frame;
        ShotImage.Width = ThumbWidth;
        ShotImage.Height = ThumbWidth * aspect;
        Caption.Text = Path.GetFileName(filePath);

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(LingerMs) };
        _timer.Tick += (_, _) => FadeOutAndClose();

        _alive.Add(this);
        Loaded += (_, _) => { Reflow(); _timer.Start(); };
        Closed += (_, _) => { _alive.Remove(this); Reflow(); };
    }

    private static void Reflow()
    {
        var work = SystemParameters.WorkArea;
        double y = work.Bottom - Gap;
        foreach (var w in System.Linq.Enumerable.Reverse(_alive))
        {
            if (!w.IsLoaded) continue;
            y -= w.ActualHeight;
            w.Left = work.Right - w.ActualWidth - Gap;
            w.Top  = y;
            y -= Gap;
        }
    }

    private void OnMouseEnter(object sender, MouseEventArgs e) => _timer.Stop();
    private void OnMouseLeave(object sender, MouseEventArgs e) => _timer.Start();

    private void OnClick(object sender, MouseButtonEventArgs e)
    {
        TryOpen(_filePath);
        FadeOutAndClose();
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) => FadeOutAndClose();

    private void FadeOutAndClose()
    {
        _timer.Stop();
        var anim = new DoubleAnimation
        {
            From = Opacity,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(180),
        };
        anim.Completed += (_, _) => Close();
        BeginAnimation(OpacityProperty, anim);
    }

    private static void TryOpen(string path)
    {
        // Always open in our own editor — never hand off to Paint / the OS viewer.
        EditorWindow.OpenForFile(path);
    }
}
