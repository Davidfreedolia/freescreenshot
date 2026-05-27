using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FreeScreenshot;

/// <summary>
/// Renders the Freedolia brand mark (two viewfinder corner brackets + a centred dot)
/// directly to an <see cref="ImageSource"/> so we don't need a .ico file in the repo.
/// </summary>
internal static class IconFactory
{
    public static ImageSource CreateTrayIcon(int size = 32)
    {
        var brushAccent = new SolidColorBrush(Color.FromRgb(0xA3, 0xE6, 0x35));
        brushAccent.Freeze();

        var pen = new Pen(brushAccent, size * 0.10) { StartLineCap = PenLineCap.Square, EndLineCap = PenLineCap.Square };
        pen.Freeze();

        var drawing = new DrawingGroup();
        using (var ctx = drawing.Open())
        {
            var inset = size * 0.18;
            var armLen = size * 0.22;

            // top-left bracket
            ctx.DrawLine(pen, new Point(inset, inset + armLen), new Point(inset, inset));
            ctx.DrawLine(pen, new Point(inset, inset), new Point(inset + armLen, inset));

            // bottom-right bracket
            ctx.DrawLine(pen, new Point(size - inset, size - inset - armLen), new Point(size - inset, size - inset));
            ctx.DrawLine(pen, new Point(size - inset, size - inset), new Point(size - inset - armLen, size - inset));

            // centred dot
            ctx.DrawEllipse(brushAccent, null, new Point(size / 2.0, size / 2.0), size * 0.07, size * 0.07);
        }

        var visual = new DrawingVisual();
        using (var ctx = visual.RenderOpen())
        {
            ctx.DrawDrawing(drawing);
        }

        var rtb = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
        rtb.Render(visual);
        rtb.Freeze();
        return rtb;
    }
}
