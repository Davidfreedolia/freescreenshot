using System.Windows;
using System.Windows.Media;
using FreeScreenshot.Core.Localization;
using Border = System.Windows.Controls.Border;
using Button = System.Windows.Controls.Button;
using Color = System.Windows.Media.Color;
using ContentPresenter = System.Windows.Controls.ContentPresenter;
using ControlTemplate = System.Windows.Controls.ControlTemplate;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Key = System.Windows.Input.Key;
using Rectangle = System.Windows.Shapes.Rectangle;
using Setter = System.Windows.Setter;
using StackPanel = System.Windows.Controls.StackPanel;
using Trigger = System.Windows.Trigger;
using VerticalAlignment = System.Windows.VerticalAlignment;

namespace FreeScreenshot.Capture;

/// <summary>
/// Pill-shaped action bar that appears under a finished selection — the
/// CleanShot moment. The user picks an action; the result is returned as
/// <see cref="ChosenAction"/>.
/// </summary>
public partial class FloatingToolbar : Window
{
    public enum Action { None, Copy, Save, Editor, Pin, Ocr, Cancel }

    public Action ChosenAction { get; private set; } = Action.None;

    public FloatingToolbar()
    {
        InitializeComponent();
        BuildButtons();
    }

    /// <summary>Position the toolbar near a selection rectangle (in screen DIPs).</summary>
    public void PositionNear(Rect selectionInScreenDips, double virtScreenLeft, double virtScreenTop)
    {
        Loaded += (_, _) =>
        {
            UpdateLayout();
            var preferredLeft = virtScreenLeft + selectionInScreenDips.X + selectionInScreenDips.Width - ActualWidth;
            var preferredTop  = virtScreenTop  + selectionInScreenDips.Y + selectionInScreenDips.Height + 12;

            // Clamp to virtual screen so the toolbar is always visible.
            var maxRight = virtScreenLeft + SystemParameters.VirtualScreenWidth;
            var maxBottom = virtScreenTop + SystemParameters.VirtualScreenHeight;
            if (preferredLeft + ActualWidth > maxRight)
                preferredLeft = maxRight - ActualWidth - 8;
            if (preferredLeft < virtScreenLeft + 8) preferredLeft = virtScreenLeft + 8;
            if (preferredTop + ActualHeight > maxBottom)
                // No room below — place above the selection.
                preferredTop = virtScreenTop + selectionInScreenDips.Y - ActualHeight - 12;

            Left = preferredLeft;
            Top  = preferredTop;
        };
    }

    private void BuildButtons()
    {
        AddBtn(BuildEditorIcon(),  Strings.T("toolbar.editor"),  Action.Editor);
        AddBtn(BuildPinIcon(),     Strings.T("toolbar.pin"),     Action.Pin);
        AddBtn(BuildOcrIcon(),     Strings.T("toolbar.ocr"),     Action.Ocr);
        AddDivider();
        AddBtn(BuildSaveIcon(),    Strings.T("toolbar.save"),    Action.Save);
        AddPrimary(BuildCopyIcon(),Strings.T("toolbar.copy"),    Action.Copy);
    }

    private void AddBtn(UIElement icon, string tooltip, Action action)
    {
        var btn = MakeIconButton(icon, tooltip);
        btn.Click += (_, _) => Choose(action);
        ButtonsHost.Children.Add(btn);
    }

    private void AddPrimary(UIElement icon, string tooltip, Action action)
    {
        var btn = MakeIconButton(icon, tooltip, primary: true);
        btn.Click += (_, _) => Choose(action);
        ButtonsHost.Children.Add(btn);
    }

    private void AddDivider()
    {
        var d = new Rectangle
        {
            Width = 1, Margin = new Thickness(8, 7, 8, 7),
            Fill = new SolidColorBrush(Color.FromRgb(0x1F, 0x3A, 0x3D)),
        };
        ButtonsHost.Children.Add(d);
    }

    private Button MakeIconButton(UIElement icon, string tooltip, bool primary = false)
    {
        var btn = new Button
        {
            Width = 38, Height = 38,
            Margin = new Thickness(3, 0, 3, 0),
            BorderThickness = new Thickness(0),
            Background = primary
                ? new SolidColorBrush(Color.FromRgb(0x2D, 0xD4, 0xBF))     // teal accent
                : System.Windows.Media.Brushes.Transparent,
            Foreground = primary
                ? new SolidColorBrush(Color.FromRgb(0x06, 0x30, 0x2C))     // on-accent text
                : new SolidColorBrush(Color.FromRgb(0xF0, 0xFA, 0xF7)),
            ToolTip = tooltip,
            Cursor = System.Windows.Input.Cursors.Hand,
            Content = icon,
            FocusVisualStyle = null,
        };
        btn.Template = BuildButtonTemplate(primary);
        return btn;
    }

    private static ControlTemplate BuildButtonTemplate(bool primary)
    {
        var tmpl = new ControlTemplate(typeof(Button));
        var border = new FrameworkElementFactory(typeof(Border));
        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(19));
        border.SetBinding(Border.BackgroundProperty, new System.Windows.Data.Binding("Background") { RelativeSource = System.Windows.Data.RelativeSource.TemplatedParent });
        var content = new FrameworkElementFactory(typeof(ContentPresenter));
        content.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        content.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
        border.AppendChild(content);
        tmpl.VisualTree = border;

        if (primary)
        {
            var hover = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
            hover.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush(Color.FromRgb(0x5E, 0xEA, 0xD4))));
            tmpl.Triggers.Add(hover);
        }
        else
        {
            var hover = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
            hover.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush(Color.FromRgb(0x22, 0x3D, 0x40))));
            tmpl.Triggers.Add(hover);
        }
        return tmpl;
    }

    // ---- Lucide-style icons rendered as Path geometry ----

    private static UIElement BuildEditorIcon()
    {
        // Pencil/edit
        return MakePath("M12 20h9 M16.5 3.5a2.121 2.121 0 0 1 3 3L7 19l-4 1 1-4Z");
    }

    private static UIElement BuildPinIcon()
    {
        // Pin
        return MakePath("M12 17 L12 22 M5 17h14l-1.5-2.5V10a5.5 5.5 0 0 0-11 0v4.5L5 17z");
    }

    private static UIElement BuildOcrIcon()
    {
        // Type (T)
        return MakePath("M4 7 V4 H20 V7 M9 20 H15 M12 4 V20");
    }

    private static UIElement BuildSaveIcon()
    {
        // Download
        return MakePath("M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4 M7 10 L12 15 L17 10 M12 15 V3");
    }

    private static UIElement BuildCopyIcon()
    {
        // Copy
        return MakePath("M9 9 H22 V22 H9 Z M5 15 H4 a2 2 0 0 1-2-2 V4 a2 2 0 0 1 2-2 H13 a2 2 0 0 1 2 2 V5");
    }

    private static UIElement MakePath(string data)
    {
        var path = new System.Windows.Shapes.Path
        {
            Stroke = new SolidColorBrush(Color.FromRgb(0xF0, 0xFA, 0xF7)),
            StrokeThickness = 1.6,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round,
            StrokeLineJoin = PenLineJoin.Round,
            Fill = null,
            Data = System.Windows.Media.Geometry.Parse(data),
            Width = 18, Height = 18,
            Stretch = Stretch.Uniform,
            UseLayoutRounding = true,
        };
        // Inherit colour from the containing button's Foreground.
        path.SetBinding(System.Windows.Shapes.Shape.StrokeProperty,
            new System.Windows.Data.Binding("Foreground")
            {
                RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.FindAncestor, typeof(Button), 1),
            });
        return path;
    }

    private void Choose(Action a)
    {
        ChosenAction = a;
        DialogResult = true;
        Close();
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            ChosenAction = Action.Cancel;
            DialogResult = false;
            Close();
        }
        else if (e.Key == Key.Enter)
        {
            ChosenAction = Action.Copy;
            DialogResult = true;
            Close();
        }
    }
}
