using DeskBox.Services;

namespace DeskBox.Tests;

public sealed class CleanDesktopIconSessionTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void HideAndRestorePreserveOriginalVisibility(bool visible)
    {
        var backend = new FakeBackend { Visible = visible };
        var session = new CleanDesktopIconSession(backend);
        Assert.True(session.Hide());
        Assert.True(session.Hide());
        Assert.False(backend.Visible);
        Assert.True(session.IsActive);
        Assert.True(session.Restore());
        Assert.Equal(visible, backend.Visible);
        Assert.False(backend.RestorePending);
        Assert.False(session.IsActive);
    }

    [Fact]
    public void NextInstanceRecoversIconsAfterInterruptedSession()
    {
        var backend = new FakeBackend { Visible = true };
        Assert.True(new CleanDesktopIconSession(backend).Hide());
        Assert.True(backend.RestorePending);
        Assert.True(new CleanDesktopIconSession(backend).Restore());
        Assert.True(backend.Visible);
        Assert.False(backend.RestorePending);
    }

    [Fact]
    public void MissingExplorerAndFailedRestoreKeepRecoveryPending()
    {
        var backend = new FakeBackend { Visible = true };
        var session = new CleanDesktopIconSession(backend);
        Assert.True(session.Hide());
        backend.Available = false;
        Assert.False(session.Restore());
        Assert.True(backend.RestorePending);
        Assert.True(session.IsActive);
        backend.Available = true;
        Assert.True(session.Restore());
        Assert.True(backend.Visible);
    }

    [Fact]
    public void UnavailableIconListDoesNotEnterCleanMode()
    {
        var backend = new FakeBackend { Available = false };
        var session = new CleanDesktopIconSession(backend);
        Assert.False(session.Hide());
        Assert.False(session.IsActive);
        Assert.False(backend.RestorePending);
    }

    [Fact]
    public void ExplorerRecreationIsHiddenAndRemainsRestorable()
    {
        var backend = new FakeBackend { Visible = true };
        var session = new CleanDesktopIconSession(backend);
        Assert.True(session.Hide());
        backend.Visible = true;
        Assert.True(session.Hide());
        Assert.False(backend.Visible);
        Assert.True(session.Restore());
        Assert.True(backend.Visible);
    }

    private sealed class FakeBackend : IDesktopIconVisibilityBackend
    {
        public bool Available { get; set; } = true;
        public bool Visible { get; set; }
        public bool RestorePending { get; set; }
        public bool? GetVisible() => Available ? Visible : null;
        public bool SetVisible(bool visible)
        {
            if (!Available) return false;
            Visible = visible;
            return true;
        }
    }
}
