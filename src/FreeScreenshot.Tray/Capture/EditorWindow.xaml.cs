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
    private enum Tool { Arrow, Rectangle, Text, Blur, Crop }

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
        _widthField = _widthPx;
        _heightField = _heightPx;
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
        ToolBlur.IsChecked == true ? Tool.Blur :
        ToolCrop.IsChecked == true ? Tool.Crop :
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

        // Crop is special — re-render with the cropped region as the new image.
        if (CurrentTool == Tool.Crop)
        {
            var x = Math.Min(_start.X, p.X);
            var y = Math.Min(_start.Y, p.Y);
            var w = Math.Abs(p.X - _start.X);
            var h = Math.Abs(p.Y - _start.Y);
            if (w >= 10 && h >= 10) DoCrop(new Rect(x, y, w, h));
            return;
        }

        var shape = BuildShape(_start, p);
        if (shape is not null)
        {
            OverlayCanvas.Children.Add(shape);
            _shapes.Push(shape);
        }
    }

    private void DoCrop(Rect rect)
    {
        // Render current state to a PNG, take the cropped sub-rectangle, and
        // reload it as the new source image. All existing annotations bake in.
        var flat = Render();
        using var ms = new MemoryStream(flat);
        var decoder = BitmapDecoder.Create(ms, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
        var src = decoder.Frames[0];
        var ix = (int)Math.Round(rect.X);
        var iy = (int)Math.Round(rect.Y);
        var iw = (int)Math.Round(rect.Width);
        var ih = (int)Math.Round(rect.Height);
        ix = Math.Max(0, Math.Min(ix, src.PixelWidth - 1));
        iy = Math.Max(0, Math.Min(iy, src.PixelHeight - 1));
        iw = Math.Min(iw, src.PixelWidth - ix);
        ih = Math.Min(ih, src.PixelHeight - iy);

        var cropped = new CroppedBitmap(src, new System.Windows.Int32Rect(ix, iy, iw, ih));
        cropped.Freeze();
        ShotImage.Source = cropped;
        _widthField = iw; _heightField = ih;
        OverlayCanvas.Width = ShotImage.Width = iw;
        OverlayCanvas.Height = ShotImage.Height = ih;
        OverlayCanvas.Children.Clear();
        _shapes.Clear();
        DimensionsText.Text = $"{iw} × {ih}";
        // Switch back to arrow tool after a crop.
        ToolArrow.IsChecked = true;
    }

    // Mutable backing fields so crop can update the canvas size at runtime.
    private int _widthField;
    private int _heightField;

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
            case Tool.Blur:
            {
                var w = Math.Abs(b.X - a.X);
                var h = Math.Abs(b.Y - a.Y);
                if (w < 6 || h < 6) return null;
                var sx = Math.Min(a.X, b.X);
                var sy = Math.Min(a.Y, b.Y);
                // A clipping container with the original image offset inside — when blurred,
                // it shows the underlying area in-place but blurred.
                var container = new System.Windows.Controls.Canvas
                {
                    Width = w, Height = h, ClipToBounds = true,
                };
                var img = new System.Windows.Controls.Image
                {
                    Source = ShotImage.Source,
                    Stretch = Stretch.None,
                    Effect = new System.Windows.Media.Effects.BlurEffect { Radius = Math.Max(12, _widthPx * 0.012) },
                };
                System.Windows.Controls.Canvas.SetLeft(img, -sx);
                System.Windows.Controls.Canvas.SetTop(img,  -sy);
                container.Children.Add(img);
                System.Windows.Controls.Canvas.SetLeft(container, sx);
                System.Windows.Controls.Canvas.SetTop(container,  sy);
                return container;
            }
            case Tool.Crop:
            {
                // Crop is applied directly on mouseUp via DoCrop(); we draw a dashed
                // preview rectangle while dragging.
                var pen = new SolidColorBrush(Color.FromRgb(0xA3, 0xE6, 0x35));
                var dashed = new Rectangle
                {
                    Stroke = pen,
                    StrokeThickness = 1.5,
                    StrokeDashArray = new System.Windows.Media.DoubleCollection { 4, 3 },
                    Width = Math.Abs(b.X - a.X),
                    Height = Math.Abs(b.Y - a.Y),
                };
                System.Windows.Controls.Canvas.SetLeft(dashed, Math.Min(a.X, b.X));
                System.Windows.Controls.Canvas.SetTop(dashed,  Math.Min(a.Y, b.Y));
                return dashed;
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
        grid.Measure(new Size(_widthField, _heightField));
        grid.Arrange(new Rect(new Size(_widthField, _heightField)));
        var rtb = new RenderTargetBitmap(_widthField, _heightField, 96, 96, PixelFormats.Pbgra32);
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
