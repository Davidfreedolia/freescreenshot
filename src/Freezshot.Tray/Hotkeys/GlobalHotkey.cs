using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Freezshot.Hotkeys;

/// <summary>
/// Win32 RegisterHotKey wrapper backed by a hidden message-only window.
/// Required because system-wide hotkeys deliver WM_HOTKEY to a specific HWND.
/// </summary>
internal sealed class GlobalHotkey : NativeWindow, IDisposable
{
    private const int WM_HOTKEY = 0x0312;

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [Flags]
    public enum Modifiers : uint
    {
        None = 0,
        Alt = 0x0001,
        Ctrl = 0x0002,
        Shift = 0x0004,
        Win = 0x0008,
        NoRepeat = 0x4000,
    }

    private readonly Dictionary<int, Action> _handlers = new();
    private int _nextId = 1;
    private bool _disposed;

    public GlobalHotkey()
    {
        CreateHandle(new CreateParams
        {
            Caption = "FreezshotHotkeyWindow",
            // Message-only window — invisible, off-screen.
            Parent = new IntPtr(-3),
        });
    }

    /// <summary>Register a system-wide hotkey. Returns the id, or 0 if registration failed.</summary>
    public int Register(Modifiers mods, uint virtualKey, Action onPressed)
    {
        var id = _nextId++;
        var flags = (uint)(mods | Modifiers.NoRepeat);
        if (!RegisterHotKey(Handle, id, flags, virtualKey)) return 0;
        _handlers[id] = onPressed;
        return id;
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_HOTKEY)
        {
            var id = m.WParam.ToInt32();
            if (_handlers.TryGetValue(id, out var handler))
            {
                try { handler(); }
                catch { /* swallow — hotkey handlers must not propagate */ }
            }
        }
        base.WndProc(ref m);
    }

    public void Dispose()
    {
        if (_disposed) return;
        foreach (var id in _handlers.Keys) UnregisterHotKey(Handle, id);
        _handlers.Clear();
        DestroyHandle();
        _disposed = true;
    }
}
