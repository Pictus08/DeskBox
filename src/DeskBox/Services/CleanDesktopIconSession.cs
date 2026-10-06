namespace DeskBox.Services;

internal interface IDesktopIconVisibilityBackend
{
    bool? GetVisible();
    bool SetVisible(bool visible);
    bool RestorePending { get; set; }
}

/// <summary>Restores only the icons that this session actually hid.</summary>
internal sealed class CleanDesktopIconSession(IDesktopIconVisibilityBackend backend)
{
    public bool IsActive { get; private set; }

    public bool Hide()
    {
        bool? visible = backend.GetVisible();
        if (!visible.HasValue)
        {
            return false;
        }

        if (visible.Value)
        {
            // Write before hiding so a subsequent launch can recover after a crash.
            backend.RestorePending = true;
            if (!backend.SetVisible(false))
            {
                return false;
            }
        }

        IsActive = true;
        return true;
    }

    public bool Restore()
    {
        if (backend.RestorePending)
        {
            if (!backend.SetVisible(true))
            {
                return false;
            }

            backend.RestorePending = false;
        }

        IsActive = false;
        return true;
    }
}
