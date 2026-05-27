using System.Windows;
using System.Windows.Media;
using FreeScreenshot.Core.Localization;
using Canvas = System.Windows.Controls.Canvas;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Key = System.Windows.Input.Key;
using MouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace FreeScreenshot.Capture;

public partial class SelectionOverlay : Window
{
    /// <summary>Selection in DIPs relative to the overlay window's own coordinate space.</summary>
    public Rect? Selection { get; private set; }

    /// <summary>Selection in physical pixels relative to the virtual desktop top-left.</summary>
    public (int X, int Y, int W, int H)? PhysicalSelection { get; private set; }

    private Point _start;
    private bool _dragging;

    public SelectionOverlay()
    {
        InitializeComponent();
        WindowState = WindowState.Normal;
        // Cover the entire virtual desktop (all monitors).
        Left   = SystemParameters.VirtualScreenLeft;
        Top    = SystemParameters.VirtualScreenTop;
        Width  = SystemParameters.VirtualScreenWidth;
        Height = SystemParameters.VirtualScreenHeight;

        Loaded += (_, _) =>
        {
            Activate();
            Focus();
            UpdateLayoutForRect(default);
            HintText.Text = Strings.T("capture.hint");
            HintText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(HintText, (Width - HintText.DesiredSize.Width) / 2);
            Canvas.SetTop(HintText, 28);
        };
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Selection = null;
            PhysicalSelection = null;
            DialogResult = false;
            Close();
        }
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        _start = e.GetPosition(Root);
        _dragging = true;
        HintText.Visibility = Visibility.Collapsed;
        SelBorder.Visibility = Visibility.Visible;
        SizeChip.Visibility = Visibility.Visible;
        CaptureMouse();
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (!_dragging) return;
        var p = e.GetPosition(Root);
        var rect = new Rect(
            Math.Min(_start.X, p.X),
            Math.Min(_start.Y, p.Y),
            Math.Abs(p.X - _start.X),
            Math.Abs(p.Y - _start.Y));
        UpdateLayoutForRect(rect);
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!_dragging) return;
        _dragging = false;
        ReleaseMouseCapture();

        var p = e.GetPosition(Root);
        var rect = new Rect(
            Math.Min(_start.X, p.X),
            Math.Min(_start.Y, p.Y),
            Math.Abs(p.X - _start.X),
            Math.Abs(p.Y - _start.Y));

        if (rect.Width < 5 || rect.Height < 5)
        {
            Selection = null;
            PhysicalSelection = null;
            DialogResult = false;
        }
        else
        {
            Selection = rect;
            // Convert to physical pixels relative to virtual desktop top-left.
            var dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;
            var virtLeft = SystemParameters.VirtualScreenLeft;
            var virtTop  = SystemParameters.VirtualScreenTop;
            PhysicalSelection = (
                (int)Math.Round((rect.X + virtLeft) * dpi),
                (int)Math.Round((rect.Y + virtTop)  * dpi),
                (int)Math.Round(rect.Width  * dpi),
                (int)Math.Round(rect.Height * dpi));
            DialogResult = true;
        }
        Close();
    }

    private void UpdateLayoutForRect(Rect r)
    {
        Canvas.SetLeft(SelBorder, r.X);
        Canvas.SetTop(SelBorder, r.Y);
        SelBorder.Width = r.Width;
        SelBorder.Height = r.Height;

        DimTop.Width = Width;               DimTop.Height = r.Y;
        Canvas.SetLeft(DimTop, 0);          Canvas.SetTop(DimTop, 0);

        DimBottom.Width = Width;            DimBottom.Height = Math.Max(0, Height - r.Bottom);
        Canvas.SetLeft(DimBottom, 0);       Canvas.SetTop(DimBottom, r.Bottom);

        DimLeft.Width = r.X;                DimLeft.Height = r.Height;
        Canvas.SetLeft(DimLeft, 0);         Canvas.SetTop(DimLeft, r.Y);

        DimRight.Width = Math.Max(0, Width - r.Right); DimRight.Height = r.Height;
        Canvas.SetLeft(DimRight, r.Right);  Canvas.SetTop(DimRight, r.Y);

        var dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;
        var px = (int)Math.Round(r.Width * dpi);
        var py = (int)Math.Round(r.Height * dpi);
        SizeText.Text = $"{px} × {py}";
        var chipX = Math.Min(r.X + r.Width + 6, Width - 80);
        var chipY = Math.Min(r.Y + r.Height + 6, Height - 28);
        Canvas.SetLeft(SizeChip, Math.Max(0, chipX));
        Canvas.SetTop(SizeChip, Math.Max(0, chipY));
    }
}
