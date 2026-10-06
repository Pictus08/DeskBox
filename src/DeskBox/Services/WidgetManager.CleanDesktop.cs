namespace DeskBox.Services;

public sealed partial class WidgetManager
{
    private readonly CleanDesktopIconSession _cleanDesktopIcons = new(new DesktopIconVisibilityBackend());

    public bool IsCleanDesktopActive => _cleanDesktopIcons.IsActive;

    private async Task ToggleCleanDesktopCoreAsync()
    {
        if (IsCleanDesktopActive)
        {
            RestoreDesktopIcons();
            await SetAllWidgetsVisibleCoreAsync(true);
            App.Log("[CleanDesktop] Restored desktop widgets and original icon visibility.");
            return;
        }

        try
        {
            if (!_cleanDesktopIcons.Hide())
            {
                App.Log("[CleanDesktop] Explorer icon list unavailable; desktop was not hidden.");
                RestoreDesktopIcons();
                return;
            }
            await SetAllWidgetsVisibleCoreAsync(false);
            App.Log("[CleanDesktop] Hidden desktop widgets and icons.");
        }
        catch
        {
            RestoreDesktopIcons();
            throw;
        }
    }

    internal void RecoverCleanDesktopState()
    {
        if (!IsCleanDesktopActive)
        {
            RestoreDesktopIcons();
            return;
        }

        try
        {
            _cleanDesktopIcons.Hide();
        }
        catch (Exception ex)
        {
            App.Log($"[CleanDesktop] Explorer recovery failed: {ex.Message}");
        }
    }

    internal void RestoreDesktopIcons()
    {
        try
        {
            if (!_cleanDesktopIcons.Restore())
            {
                App.Log("[CleanDesktop] Icon restore pending until Explorer is available.");
            }
        }
        catch (Exception ex)
        {
            App.Log($"[CleanDesktop] Icon restore failed: {ex.Message}");
        }
    }
}
