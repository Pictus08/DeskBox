using DeskBox.Helpers;
using DeskBox.Models;
using System.Globalization;
using Windows.UI;

namespace DeskBox.Services;

/// <summary>Independent solid backgrounds, stored with each widget's metadata.</summary>
public static class WidgetBackgroundSettings
{
    public const string ColorMetadataKey = "WidgetBackgroundColor";
    public const string OpacityMetadataKey = "WidgetBackgroundOpacity";
    public const int RecentColorLimit = 10;

    public static Color? GetColor(WidgetConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        return config.Metadata is not null &&
            config.Metadata.TryGetValue(ColorMetadataKey, out string? value) &&
            AccentColorHelper.TryParseHex(value, out Color color)
                ? Color.FromArgb(255, color.R, color.G, color.B)
                : null;
    }

    public static void SetColor(WidgetConfig config, Color? color)
    {
        ArgumentNullException.ThrowIfNull(config);
        config.Metadata ??= [];
        if (color is { } value)
        {
            config.Metadata[ColorMetadataKey] = AccentColorHelper.ToHex(
                Color.FromArgb(255, value.R, value.G, value.B));
        }
        else
        {
            config.Metadata.Remove(ColorMetadataKey);
            config.Metadata.Remove(OpacityMetadataKey);
        }
    }

    public static double GetOpacity(WidgetConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        return config.Metadata is not null &&
            config.Metadata.TryGetValue(OpacityMetadataKey, out string? value) &&
            double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double opacity) &&
            double.IsFinite(opacity)
                ? Math.Clamp(opacity, 0, 1)
                : 1;
    }

    public static void SetOpacity(WidgetConfig config, double opacity)
    {
        ArgumentNullException.ThrowIfNull(config);
        config.Metadata ??= [];
        config.Metadata[OpacityMetadataKey] =
            (double.IsFinite(opacity) ? Math.Clamp(opacity, 0, 1) : 1)
            .ToString("0.####", CultureInfo.InvariantCulture);
    }

    public static Color? GetSurfaceColor(WidgetConfig config) => GetColor(config) is { } color
        ? Color.FromArgb((byte)Math.Round(GetOpacity(config) * 255), color.R, color.G, color.B)
        : null;

    public static IReadOnlyList<string> GetRecentColors(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        return NormalizeColors(settings.WidgetShell.RecentWidgetBackgroundColors);
    }

    public static void RememberColor(AppSettings settings, Color color)
    {
        ArgumentNullException.ThrowIfNull(settings);
        string hex = AccentColorHelper.ToHex(Color.FromArgb(255, color.R, color.G, color.B));
        settings.WidgetShell.RecentWidgetBackgroundColors = NormalizeColors(
            new[] { hex }.Concat(settings.WidgetShell.RecentWidgetBackgroundColors ?? []));
    }

    public static bool NormalizeRecentColors(AppSettings settings)
    {
        if (settings.WidgetShell.RecentWidgetBackgroundColors is not { } current)
        {
            return false;
        }
        var normalized = NormalizeColors(current);
        if (current.SequenceEqual(normalized))
        {
            return false;
        }
        settings.WidgetShell.RecentWidgetBackgroundColors = normalized;
        return true;
    }

    private static List<string> NormalizeColors(IEnumerable<string>? colors)
    {
        var result = new List<string>(RecentColorLimit);
        foreach (string value in colors ?? [])
        {
            if (AccentColorHelper.TryParseHex(value, out Color color))
            {
                string hex = AccentColorHelper.ToHex(Color.FromArgb(255, color.R, color.G, color.B));
                if (!result.Contains(hex)) result.Add(hex);
                if (result.Count == RecentColorLimit) break;
            }
        }
        return result;
    }

    public static bool UseDarkForeground(Color background)
    {
        static double Linear(byte channel)
        {
            double value = channel / 255.0;
            return value <= 0.04045 ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4);
        }

        double luminance = 0.2126 * Linear(background.R) +
            0.7152 * Linear(background.G) + 0.0722 * Linear(background.B);
        return luminance > 0.179;
    }
}
