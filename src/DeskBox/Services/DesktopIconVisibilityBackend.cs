using DeskBox.Platform;
using Microsoft.Win32;

namespace DeskBox.Services;

/// <summary>
/// Hides only Explorer's icon list, leaving the desktop host, files, positions,
/// and Windows' own "Show desktop icons" preference intact.
/// </summary>
internal sealed class DesktopIconVisibilityBackend : IDesktopIconVisibilityBackend
{
    private readonly string _recoveryKey =
        @"Software\DeskBox\CleanDesktop\" + DeskBoxDataPathService.Current.InstanceScope;

    public bool RestorePending
    {
        get
        {
            using var key = Registry.CurrentUser.OpenSubKey(_recoveryKey);
            return key?.GetValue("RestoreIcons") is int value && value == 1;
        }
        set
        {
            using var key = Registry.CurrentUser.CreateSubKey(_recoveryKey);
            if (value)
            {
                key.SetValue("RestoreIcons", 1, RegistryValueKind.DWord);
            }
            else
            {
                key.DeleteValue("RestoreIcons", throwOnMissingValue: false);
            }
        }
    }

    public bool? GetVisible()
    {
        IntPtr list = FindIconList();
        return list == IntPtr.Zero ? null : Win32Helper.IsWindowVisible(list);
    }

    public bool SetVisible(bool visible)
    {
        // Respect a Windows-side choice to keep icons hidden made during the session.
        if (visible)
        {
            using var key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced");
            if (key?.GetValue("HideIcons") is int hidden && hidden == 1)
            {
                return true;
            }
        }

        IntPtr list = FindIconList();
        if (list == IntPtr.Zero)
        {
            return false;
        }

        Win32Helper.ShowWindow(list, visible ? Win32Helper.SW_SHOWNOACTIVATE : Win32Helper.SW_HIDE);
        return Win32Helper.IsWindowVisible(list) == visible;
    }

    private static IntPtr FindIconList()
    {
        IntPtr result = IntPtr.Zero;
        Win32Helper.EnumWindows((window, _) =>
        {
            IntPtr view = Win32Helper.FindWindowEx(window, IntPtr.Zero, "SHELLDLL_DefView", null);
            if (view != IntPtr.Zero)
            {
                result = Win32Helper.FindWindowEx(view, IntPtr.Zero, "SysListView32", null);
            }
            return result == IntPtr.Zero;
        }, IntPtr.Zero);
        return result;
    }
}
