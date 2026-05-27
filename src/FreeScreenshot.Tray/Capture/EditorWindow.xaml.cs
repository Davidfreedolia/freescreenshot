using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FreeScreenshot.Core.Localization;
using Brushes = System.Windows.Media.Brushes;
using Clipboard = System.Windows.Clipboard;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
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

    private Point _start;
    private bool _drawing;
    private UIElement? _preview;

    public byte[]? EditedPng { get; private set; }
    public bool ShouldSave { get; private set; }

    public EditorWindow(byte[] pngBytes)
    {
        InitializeComponent();
        _originalPng = pngBytes;

        using var ms = new MemoryStream(pngBytes);
        var decoder = BitmapDecoder.Create(ms, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
        var frame = decoder.Frames[0];
        frame.Freeze();
        ShotImage.Source = frame;
        _widthPx = frame.PixelWidth;
        _heightPx = frame.PixelHeight;
        OverlayCanvas.Width = ShotImage.Width = _widthPx;
        OverlayCanvas.Height = ShotImage.Height = _heightPx;

        Title             = Strings.T("editor.title");
        UndoBtn.ToolTip   = Strings.T("editor.undo");
        CancelBtn.Content = Strings.T("editor.cancel");
        CopyBtn.Content   = Strings.T("editor.copy");
        SaveBtn.Content   = Strings.T("editor.save");
        DimensionsText.Text = $"{_widthPx} × {_heightPx}";
    }

    private Tool CurrentTool =>
        ToolRect.IsChecked == true ? Tool.Rectangle :
        ToolText.IsChecked == true ? Tool.Text :
        Tool.Arrow;

    private SolidColorBrush CurrentColor
    {
        get
        {
            string hex =
                ColorRed.IsChecked    == true ? "#EF4444" :
                ColorYellow.IsChecked == true ? "#FBBF24" :
                ColorWhite.IsChecked  == true ? "#F5F2EC" :
                ColorDark.IsChecked   == true ? "#1A1814" :
                                                "#A3E635";
            var c = (Color)ColorConverter.ConvertFromString(hex);
            var b = new SolidColorBrush(c);
            b.Freeze();
            return b;
        }
    }

    private double CurrentStroke
    {
        get
        {
            // Stored in Tag as string ("2"/"4"/"7"). Pick up via the scale we want at native pixel size.
            var tag =
                StrokeThin.IsChecked == true ? StrokeThin.Tag :
                StrokeThick.IsChecked == true ? StrokeThick.Tag :
                StrokeMed.Tag;
            return double.TryParse(tag?.ToString(), out var v) ? v : 4.0;
        }
    }

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
        var brush = CurrentColor;
        // Convert stroke setting to a thickness that respects image scale.
        var thickness = Math.Max(1.0, _widthPx * 0.0008 * CurrentStroke);

        switch (CurrentTool)
        {
            case Tool.Rectangle:
            {
                var r = new Rectangle
                {
                    Stroke = brush,
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
                    Stroke = brush, StrokeThickness = thickness,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap   = PenLineCap.Round,
                };
                g.Children.Add(line);
                var dx = b.X - a.X;
                var dy = b.Y - a.Y;
                var len = Math.Sqrt(dx * dx + dy * dy);
                if (len > 6)
                {
                    var ux = dx / len;
                    var uy = dy / len;
                    var head = Math.Min(thickness * 5.0, len * 0.35);
                    var px = -uy * head * 0.55;
                    var py =  ux * head * 0.55;
                    var p1 = new Point(b.X - ux * head + px, b.Y - uy * head + py);
                    var p2 = new Point(b.X - ux * head - px, b.Y - uy * head - py);
                    var poly = new Polygon
                    {
                        Points = new System.Windows.Media.PointCollection { new(b.X, b.Y), p1, p2 },
                        Fill = brush,
                    };
                    g.Children.Add(poly);
                }
                return g;
            }
            case Tool.Text:
            {
                if (Math.Abs(b.X - a.X) < 24 || Math.Abs(b.Y - a.Y) < 14)
                    return null;
                var tb = new System.Windows.Controls.TextBox
                {
                    Width = Math.Abs(b.X - a.X),
                    MinWidth = 60,
                    Background = Brushes.Transparent,
                    Foreground = brush,
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(0),
                    FontWeight = FontWeights.SemiBold,
                    FontSize = Math.Max(16, _widthPx * 0.022),
                    Text = Strings.T("editor.text.placeholder"),
                    AcceptsReturn = false,
                    SnapsToDevicePixels = true,
                    UseLayoutRounding = true,
                };
                // Subtle dark shadow so light text reads on any background.
                tb.Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Color.FromRgb(0, 0, 0),
                    BlurRadius = 6,
                    ShadowDepth = 0,
                    Opacity = 0.6,
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
