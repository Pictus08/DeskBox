using DeskBox.Models;
using DeskBox.Helpers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace DeskBox.Services;

internal static class WidgetBackgroundMenuBuilder
{
    public static MenuFlyoutSubItem Create(
        WidgetConfig config,
        LocalizationService localization,
        Action reset,
        Action chooseColor,
        AppSettings settings,
        Action<Color> applyColor)
    {
        var menu = new MenuFlyoutSubItem
        {
            Text = localization.T("Widget.Background.Menu"),
            Icon = new FontIcon { Glyph = "\uE790" }
        };
        var useGlobal = new ToggleMenuFlyoutItem
        {
            Text = localization.T("Widget.Foreground.UseGlobal"),
            IsChecked = WidgetBackgroundSettings.GetColor(config) is null
        };
        useGlobal.Click += (_, _) => reset();
        var custom = new ToggleMenuFlyoutItem
        {
            Text = localization.T("Widget.Foreground.CustomColor"),
            IsChecked = WidgetBackgroundSettings.GetColor(config) is not null
        };
        custom.Click += (_, _) => chooseColor();
        menu.Items.Add(useGlobal);
        menu.Items.Add(custom);
        var recentColors = WidgetBackgroundSettings.GetRecentColors(settings);
        if (recentColors.Count > 0)
        {
            var recent = new MenuFlyoutSubItem
            {
                Text = localization.T("Widget.Background.Recent")
            };
            foreach (string hex in recentColors)
            {
                Color color = AccentColorHelper.FromHex(hex);
                var item = new MenuFlyoutItem
                {
                    Text = hex,
                    Icon = new FontIcon { Glyph = "\uE91F", Foreground = new SolidColorBrush(color) }
                };
                item.Click += (_, _) => applyColor(color);
                recent.Items.Add(item);
            }
            menu.Items.Add(recent);
        }
        return menu;
    }
}
