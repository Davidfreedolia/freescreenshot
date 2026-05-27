using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FreeScreenshot.Core.Localization;
using Brushes = System.Windows.Media.Brushes;
using Clipboard = System.Windows.Clipboard;
using Color = System.Windows.Media.Color;
using Line = System.Windows.Shapes.Line;
using MouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Windows.Point;
using Polygon = System.Windows.Shapes.Polygon;
using Rectangle = System.Windows.Shapes.Rectangle;
using Size = System.Windows.Size;

namespace FreeScreenshot.Capture;

public partial class EditorWindow : Window
{
    private enum Tool { Arrow, Rectangle, Text }

    private readonly byte[] _originalPng;
    private readonly int _widthPx;
    private readonly int _heightPx;
    private readonly Stack<UIElement> _shapes = new();
    private readonly SolidColorBrush _lime = new(Color.FromRgb(0xA3, 0xE6, 0x35));

    private Point _start;
    private bool _drawing;
    private UIElement? _preview;

    /// <summary>The PNG bytes after editing (set when user clicks Save or Copy).</summary>
    public byte[]? EditedPng { get; private set; }

    /// <summary>True if user clicked Save and we should write to disk.</summary>
    public bool ShouldSave { get; private set; }

    public EditorWindow(byte[] pngBytes)
    {
        InitializeComponent();
        _originalPng = pngBytes;
        _lime.Freeze();

        using var ms = new MemoryStream(pngBytes);
        var decoder = BitmapDecoder.Create(ms, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
        var frame = decoder.Frames[0];
        frame.Freeze();
        ShotImage.Source = frame;
        _widthPx = frame.PixelWidth;
        _heightPx = frame.PixelHeight;
        OverlayCanvas.Width = ShotImage.Width = _widthPx;
        OverlayCanvas.Height = ShotImage.Height = _heightPx;

        // Localized labels
        Title = Strings.T("editor.title");
        LblArrow.Text = Strings.T("editor.tool.arrow");
        LblRect.Text  = Strings.T("editor.tool.rect");
        LblText.Text  = Strings.T("editor.tool.text");
        LblUndo.Text  = Strings.T("editor.undo");
        CancelBtn.Content = Strings.T("editor.cancel");
        CopyBtn.Content   = Strings.T("editor.copy");
        SaveBtn.Content   = Strings.T("editor.save");
        DimensionsText.Text = $"{_widthPx} × {_heightPx}";
    }

    private Tool CurrentTool =>
        ToolRect.IsChecked == true ? Tool.Rectangle :
        ToolText.IsChecked == true ? Tool.Text :
        Tool.Arrow;

    private void OnCanvasDown(object sender, MouseButtonEventArgs e)
    {
        _start = e.GetPosition(OverlayCanvas);
        _drawing = true;
        OverlayCanvas.CaptureMouse();
    }

    private void OnCanvasMove(object sender, MouseEventArgs e)
    {
        if (!_drawing) return;
        var p = e.GetPosition(OverlayCanvas);
        if (_preview is not null) OverlayCanvas.Children.Remove(_preview);
        _preview = BuildShape(_start, p);
        if (_preview is not null) OverlayCanvas.Children.Add(_preview);
    }

    private void OnCanvasUp(object sender, MouseButtonEventArgs e)
    {
        if (!_drawing) return;
        _drawing = false;
        OverlayCanvas.ReleaseMouseCapture();
        var p = e.GetPosition(OverlayCanvas);
        if (_preview is not null) OverlayCanvas.Children.Remove(_preview);
        _preview = null;
        var shape = BuildShape(_start, p);
        if (shape is not null)
        {
            OverlayCanvas.Children.Add(shape);
            _shapes.Push(shape);
        }
    }

    private UIElement? BuildShape(Point a, Point b)
    {
        var thickness = Math.Max(2.0, _widthPx * 0.0035);
        switch (CurrentTool)
        {
            case Tool.Rectangle:
            {
                var r = new Rectangle
                {
                    Stroke = _lime,
                    StrokeThickness = thickness,
                    Width = Math.Abs(b.X - a.X),
                    Height = Math.Abs(b.Y - a.Y),
                };
                System.Windows.Controls.Canvas.SetLeft(r, Math.Min(a.X, b.X));
                System.Windows.Controls.Canvas.SetTop(r,  Math.Min(a.Y, b.Y));
                return r;
            }
            case Tool.Arrow:
            {
                var g = new System.Windows.Controls.Canvas();
                var line = new Line
                {
                    X1 = a.X, Y1 = a.Y, X2 = b.X, Y2 = b.Y,
                    Stroke = _lime, StrokeThickness = thickness,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round,
                };
                g.Children.Add(line);
                // Arrow head
                var dx = b.X - a.X;
                var dy = b.Y - a.Y;
                var len = Math.Sqrt(dx * dx + dy * dy);
                if (len > 6)
                {
                    var ux = dx / len;
                    var uy = dy / len;
                    var head = Math.Min(20.0, len * 0.3);
                    // perpendicular
                    var px = -uy * head * 0.55;
                    var py =  ux * head * 0.55;
                    var p1 = new Point(b.X - ux * head + px, b.Y - uy * head + py);
                    var p2 = new Point(b.X - ux * head - px, b.Y - uy * head - py);
                    var poly = new Polygon
                    {
                        Points = new System.Windows.Media.PointCollection { new(b.X, b.Y), p1, p2 },
                        Fill = _lime,
                    };
                    g.Children.Add(poly);
                }
                return g;
            }
            case Tool.Text:
            {
                if (Math.Abs(b.X - a.X) < 30 || Math.Abs(b.Y - a.Y) < 16)
                    return null;
                var tb = new System.Windows.Controls.TextBox
                {
                    Width = Math.Abs(b.X - a.X),
                    MinWidth = 60,
                    Background = System.Windows.Media.Brushes.Transparent,
                    Foreground = _lime,
                    BorderThickness = new Thickness(0),
                    FontWeight = FontWeights.SemiBold,
                    FontSize = Math.Max(16, _widthPx * 0.018),
                    Text = Strings.T("editor.text.placeholder"),
                };
                System.Windows.Controls.Canvas.SetLeft(tb, Math.Min(a.X, b.X));
                System.Windows.Controls.Canvas.SetTop(tb,  Math.Min(a.Y, b.Y));
                tb.Loaded += (_, _) => { tb.Focus(); tb.SelectAll(); };
                return tb;
            }
        }
        return null;
    }

    private void OnUndo(object sender, RoutedEventArgs e)
    {
        if (_shapes.Count == 0) return;
        var last = _shapes.Pop();
        OverlayCanvas.Children.Remove(last);
    }

    private byte[] Render()
    {
        // Render Image + OverlayCanvas to a single PNG at native pixel size.
        var grid = (FrameworkElement)CanvasHost;
        grid.Measure(new Size(_widthPx, _heightPx));
        grid.Arrange(new Rect(new Size(_widthPx, _heightPx)));
        var rtb = new RenderTargetBitmap(_widthPx, _heightPx, 96, 96, PixelFormats.Pbgra32);
        rtb.Render(grid);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(rtb));
        using var ms = new MemoryStream();
        encoder.Save(ms);
        return ms.ToArray();
    }

    private void OnSave(object sender, RoutedEventArgs e)
    {
        EditedPng = Render();
        ShouldSave = true;
        try { Clipboard.SetImage(BitmapFrame.Create(new MemoryStream(EditedPng), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad)); } catch { }
        DialogResult = true;
        Close();
    }

    private void OnCopy(object sender, RoutedEventArgs e)
    {
        EditedPng = Render();
        ShouldSave = false;
        try { Clipboard.SetImage(BitmapFrame.Create(new MemoryStream(EditedPng), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad)); } catch { }
        DialogResult = true;
        Close();
    }

    private void OnCancel(object sender, RoutedEventArgs e)
    {
        EditedPng = null;
        ShouldSave = false;
        DialogResult = false;
        Close();
    }
}
