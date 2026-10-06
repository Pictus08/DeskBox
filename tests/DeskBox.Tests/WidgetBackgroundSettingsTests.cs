using System.Text.Json;
using DeskBox.Models;
using DeskBox.Services;
using Windows.UI;

namespace DeskBox.Tests;

public sealed class WidgetBackgroundSettingsTests
{
    [Fact]
    public void ColorsAreIndependentAndSurviveSerialization()
    {
        var first = new WidgetConfig();
        var second = new WidgetConfig();
        WidgetBackgroundSettings.SetColor(first, Color.FromArgb(128, 255, 0, 0));
        WidgetBackgroundSettings.SetColor(second, Color.FromArgb(255, 0, 0, 255));

        var restored = JsonSerializer.Deserialize<WidgetConfig>(JsonSerializer.Serialize(first))!;
        Assert.Equal(Color.FromArgb(255, 255, 0, 0), WidgetBackgroundSettings.GetColor(restored));
        Assert.Equal(Color.FromArgb(255, 0, 0, 255), WidgetBackgroundSettings.GetColor(second));

        first.Metadata["Unrelated"] = "preserved";
        WidgetBackgroundSettings.SetColor(first, null);
        Assert.Null(WidgetBackgroundSettings.GetColor(first));
        Assert.Equal("preserved", first.Metadata["Unrelated"]);
        Assert.NotNull(WidgetBackgroundSettings.GetColor(second));
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("")]
    [InlineData("#ZZ0011")]
    public void InvalidSavedColorsFallBackToGlobalMaterial(string value)
    {
        var config = new WidgetConfig();
        config.Metadata[WidgetBackgroundSettings.ColorMetadataKey] = value;
        Assert.Null(WidgetBackgroundSettings.GetColor(config));
    }

    [Theory]
    [InlineData(255, 255, 255, true)]
    [InlineData(255, 255, 0, true)]
    [InlineData(0, 0, 0, false)]
    [InlineData(0, 0, 255, false)]
    public void AutomaticTextContrastFollowsBackground(byte r, byte g, byte b, bool darkText)
    {
        Assert.Equal(darkText, WidgetBackgroundSettings.UseDarkForeground(Color.FromArgb(255, r, g, b)));
    }

    [Fact]
    public void RecentColorsKeepTenUniqueEntriesInMostRecentlyUsedOrder()
    {
        var settings = new AppSettings();
        for (byte index = 0; index < 12; index++)
        {
            WidgetBackgroundSettings.RememberColor(settings, Color.FromArgb(255, index, 0, 0));
        }

        Assert.Equal(10, WidgetBackgroundSettings.GetRecentColors(settings).Count);
        Assert.Equal("#0B0000", WidgetBackgroundSettings.GetRecentColors(settings)[0]);
        Assert.Equal("#020000", WidgetBackgroundSettings.GetRecentColors(settings)[9]);
        WidgetBackgroundSettings.RememberColor(settings, Color.FromArgb(50, 5, 0, 0));
        var recent = WidgetBackgroundSettings.GetRecentColors(settings);
        Assert.Equal(10, recent.Count);
        Assert.Equal("#050000", recent[0]);
        Assert.Equal("#0B0000", recent[1]);
        Assert.Single(recent.Where(color => color == "#050000"));
        Assert.DoesNotContain("#010000", recent);
    }

    [Fact]
    public void RecentColorsNormalizeMalformedDuplicateAndMixedCaseEntries()
    {
        var settings = new AppSettings();
        settings.WidgetShell.RecentWidgetBackgroundColors =
            ["#aabbcc", "invalid", "#FFAABBCC", "", "#123456"];

        Assert.True(WidgetBackgroundSettings.NormalizeRecentColors(settings));
        Assert.Equal(new[] { "#AABBCC", "#123456" }, WidgetBackgroundSettings.GetRecentColors(settings));
        Assert.False(WidgetBackgroundSettings.NormalizeRecentColors(settings));
        Assert.Empty(WidgetBackgroundSettings.GetRecentColors(new AppSettings()));
    }

    [Fact]
    public void BackgroundOpacityIsPerWidgetAndSurvivesSerialization()
    {
        var first = new WidgetConfig();
        var second = new WidgetConfig();
        WidgetBackgroundSettings.SetColor(first, Color.FromArgb(255, 0x20, 0x38, 0x50));
        WidgetBackgroundSettings.SetColor(second, Color.FromArgb(255, 0x20, 0x38, 0x50));
        WidgetBackgroundSettings.SetOpacity(first, 0.4);

        var restored = JsonSerializer.Deserialize<WidgetConfig>(JsonSerializer.Serialize(first))!;
        Assert.Equal(Color.FromArgb(102, 0x20, 0x38, 0x50), WidgetBackgroundSettings.GetSurfaceColor(restored));
        Assert.Equal(255, WidgetBackgroundSettings.GetSurfaceColor(second)!.Value.A);
        Assert.Equal(255, WidgetBackgroundSettings.GetColor(restored)!.Value.A);
        WidgetBackgroundSettings.SetColor(first, null);
        Assert.Null(WidgetBackgroundSettings.GetSurfaceColor(first));
        Assert.False(first.Metadata.ContainsKey(WidgetBackgroundSettings.OpacityMetadataKey));
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("1", 1)]
    [InlineData("0.25", 0.25)]
    [InlineData("invalid", 1)]
    [InlineData("NaN", 1)]
    [InlineData("Infinity", 1)]
    [InlineData("-1", 0)]
    [InlineData("2", 1)]
    public void SavedOpacityIsFiniteAndBounded(string value, double expected)
    {
        var config = new WidgetConfig();
        config.Metadata[WidgetBackgroundSettings.OpacityMetadataKey] = value;
        Assert.Equal(expected, WidgetBackgroundSettings.GetOpacity(config));
    }
}
