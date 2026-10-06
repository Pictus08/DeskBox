using DeskBox.Services;
using DeskBox.Helpers;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace DeskBox.Views;

public abstract partial class WidgetWindowBase
{
    protected Color? CustomWidgetBackground => WindowsCompatibilityService.IsHighContrast
        ? null
        : WidgetBackgroundSettings.GetSurfaceColor(Config);

    protected string EffectiveWidgetMaterialType => CustomWidgetBackground.HasValue
        ? SettingsService.WidgetMaterialTypeSolid
        : WindowsCompatibilityService.ResolveWidgetMaterialType(SettingsService.Settings.WidgetShell.WidgetMaterialType);

    protected void ResetWidgetBackground() => SetWidgetBackground(null);

    private void SetWidgetBackground(Color? color, double? opacity = null)
    {
        WidgetBackgroundSettings.SetColor(Config, color);
        if (color is { } selected)
        {
            if (opacity.HasValue) WidgetBackgroundSettings.SetOpacity(Config, opacity.Value);
            WidgetBackgroundSettings.RememberColor(SettingsService.Settings, selected);
        }
        SettingsService.UpdateWidget(Config);
        ApplyBackdropPreference();
        ApplyWidgetForegroundAppearance();
    }

    protected MenuFlyoutSubItem CreateWidgetBackgroundMenu(Action chooseColor) =>
        WidgetBackgroundMenuBuilder.Create(
            Config, App.Current.LocalizationService, ResetWidgetBackground,
            chooseColor, SettingsService.Settings, color => SetWidgetBackground(color));

    protected Flyout BuildWidgetBackgroundColorPickerFlyout()
    {
        var localization = App.Current.LocalizationService;
        var picker = new ColorPicker
        {
            Color = WidgetBackgroundSettings.GetColor(Config) ??
                BuildNativeBackdropTintColor(RootElement.ActualTheme == ElementTheme.Dark),
            IsAlphaEnabled = false,
            MinWidth = 256
        };
        var save = new Button { Content = localization.T("Common.Save"), MinWidth = 96 };
        var cancel = new Button { Content = localization.T("Common.Cancel"), MinWidth = 96 };
        AutomationProperties.SetAutomationId(picker, "WidgetBackgroundColorPicker");
        AutomationProperties.SetAutomationId(save, "SaveWidgetBackground");
        AutomationProperties.SetAutomationId(cancel, "CancelWidgetBackground");

        var recentPalette = new Grid { ColumnSpacing = 6, RowSpacing = 6 };
        for (int column = 0; column < 5; column++) recentPalette.ColumnDefinitions.Add(new ColumnDefinition());
        recentPalette.RowDefinitions.Add(new RowDefinition());
        recentPalette.RowDefinitions.Add(new RowDefinition());
        var recentColors = WidgetBackgroundSettings.GetRecentColors(SettingsService.Settings);
        for (int index = 0; index < recentColors.Count; index++)
        {
            string hex = recentColors[index];
            Color color = AccentColorHelper.FromHex(hex);
            var button = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Height = 26,
                Padding = new Thickness(0),
                Content = new Border
                {
                    Background = new SolidColorBrush(color),
                    BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Gray),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(3),
                    Height = 20,
                    Width = 30
                }
            };
            AutomationProperties.SetName(button, hex);
            AutomationProperties.SetAutomationId(button, $"RecentBackgroundColor{index}");
            ToolTipService.SetToolTip(button, hex);
            button.Click += (_, _) => picker.Color = color;
            Grid.SetColumn(button, index % 5);
            Grid.SetRow(button, index / 5);
            recentPalette.Children.Add(button);
        }

        var transparency = new Slider
        {
            Minimum = 0,
            Maximum = 100,
            StepFrequency = 1,
            Value = (1 - WidgetBackgroundSettings.GetOpacity(Config)) * 100
        };
        AutomationProperties.SetName(transparency, localization.T("Settings.Opacity.Title"));
        AutomationProperties.SetAutomationId(transparency, "WidgetBackgroundTransparency");
        var transparencyValue = new TextBlock { HorizontalAlignment = HorizontalAlignment.Right };
        void RefreshTransparencyLabel() => transparencyValue.Text = $"{transparency.Value:0}%";
        transparency.ValueChanged += (_, _) => RefreshTransparencyLabel();
        RefreshTransparencyLabel();

        var content = new StackPanel
        {
            Spacing = 10,
            Children =
            {
                new TextBlock { Text = localization.T("Widget.Background.Menu"), FontWeight = FontWeights.SemiBold },
                new TextBlock
                {
                    Text = localization.T("Widget.Background.Recent"),
                    Visibility = recentColors.Count == 0 ? Visibility.Collapsed : Visibility.Visible
                },
                recentPalette,
                picker,
                new TextBlock { Text = localization.T("Settings.Opacity.Title") },
                transparencyValue,
                transparency,
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Spacing = 8,
                    Children = { cancel, save }
                }
            }
        };
        recentPalette.Visibility = recentColors.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        var flyout = new Flyout
        {
            ShouldConstrainToRootBounds = false,
            Content = new ScrollViewer
            {
                MaxHeight = 640,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = content
            }
        };
        cancel.Click += (_, _) => flyout.Hide();
        save.Click += (_, _) =>
        {
            SetWidgetBackground(picker.Color, 1 - transparency.Value / 100);
            flyout.Hide();
        };
        return flyout;
    }
}
