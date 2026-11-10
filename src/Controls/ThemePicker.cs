using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media.Animation;

using JPDFs.Services;
using JPDFs.Shell;

namespace JPDFs.Controls
{
    /// <summary>
    /// J-PDF's theme picker: a radio-row ContextMenu with the two Acrobat palettes (light and
    /// dark) and the Film Grain switch. The palettes are fixed, so there is no accent strip.
    ///
    /// App hooks (set after constructing; leave them null to keep the default behavior):
    ///   ThemeChangeTransition wraps the apply in the app's own transition. The default
    ///     cross-fades through ThemeTransition; pass <c>apply => apply()</c> for no fade.
    ///   ThemeApplied runs after a theme is applied, for app frame work such as
    ///     DwmChrome.SetRoundedCorners.
    /// </summary>
    internal sealed class ThemePicker
    {
        // Picker order: the light reading room first, the graphite alternative second.
        private static readonly Theme[] Themes = [Theme.AcrobatLight, Theme.AcrobatDark];

        private readonly Window _window;
        private readonly ContextMenu _menu;
        private readonly FrameworkElement _button;
        private readonly FrameworkElement _pane;
        private CheckBox? _filmGrainCheck;
        // True while this picker is the one changing the theme, so ThemeChanged does not rebuild
        // the menu out from under the click.
        private bool _applying;

        /// <summary>Wraps a theme change; null cross-fades through ThemeTransition.</summary>
        internal Action<Action>? ThemeChangeTransition { get; set; }

        /// <summary>Runs after the picker applies a theme.</summary>
        internal Action? ThemeApplied { get; set; }

        private void RunTransition(Action<Action>? transition, Action apply)
        {
            if (transition != null) transition(apply);
            else ThemeTransition.CrossFade((FrameworkElement)_window.Content, apply);
        }

        internal ThemePicker(Window window, ContextMenu menu, FrameworkElement button,
                             FrameworkElement contentPane)
        {
            _window = window;
            _menu = menu;
            _button = button;
            _pane = contentPane;
            if (_button is FrameworkElement themeButton)
                themeButton.ContextMenuOpening += (_, e) => { e.Handled = true; Toggle(); };
        }

        internal void Toggle()
        {
            if (_menu.IsOpen) { _menu.IsOpen = false; return; }
            Build();
            FlyoutPlacement.UsePane(_pane);
            FlyoutPlacement.Attach(_menu, null);   // a small menu: always the pane corner
            _menu.IsOpen = true;
            _menu.BeginAnimation(UIElement.OpacityProperty,
                new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(150))
                { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } });
        }

        internal void Refresh()
        {
            if (_menu.IsOpen && !_applying) Build();
        }

        private void Build()
        {
            _menu.Items.Clear();
            var panel = new StackPanel { Margin = new Thickness(12, 10, 12, 10) };

            foreach (var theme in Themes)
            {
                var radio = new RadioButton
                {
                    Content = ThemeLabel(theme), Tag = theme, GroupName = "ThemeGroup",
                    Style = (Style)_window.FindResource("ThemeRadio"),
                    IsChecked = ThemeManager.Current == theme
                };
                AutomationProperties.SetName(radio, ThemeLabel(theme));
                radio.Checked += (_, _) => Theme_Checked(theme);
                panel.Children.Add(radio);
            }

            // Film Grain: one switch for the grain texture on every surface
            // (Services/FilmGrain.cs). Alt+Shift+G runs the same toggle, so the tooltip
            // carries the chord.
            var grainDivider = new Border
            {
                Height = 1,
                Margin = new Thickness(0, 8, 9, 8),
                Opacity = 0.18,
                IsHitTestVisible = false,
                OpacityMask = new System.Windows.Media.LinearGradientBrush
                {
                    StartPoint = new Point(0, 0), EndPoint = new Point(1, 0),
                    GradientStops =
                    {
                        new System.Windows.Media.GradientStop(System.Windows.Media.Colors.Transparent, 0),
                        new System.Windows.Media.GradientStop(System.Windows.Media.Colors.Black, 0.15),
                        new System.Windows.Media.GradientStop(System.Windows.Media.Colors.Black, 0.85),
                        new System.Windows.Media.GradientStop(System.Windows.Media.Colors.Transparent, 1),
                    },
                },
            };
            grainDivider.SetResourceReference(Border.BackgroundProperty, "TextBrush");
            panel.Children.Add(grainDivider);

            var filmGrain = new CheckBox
            {
                Style = (Style)_window.FindResource("ThemeCheck"),
                IsChecked = FilmGrain.Enabled,
                Margin = new Thickness(0),
            };
            filmGrain.SetResourceReference(ContentControl.ContentProperty, "Str_Theme_FilmGrain");
            filmGrain.Click += (_, _) => FilmGrain.Set(filmGrain.IsChecked == true);
            filmGrain.ToolTip = (_window.TryFindResource("Str_Theme_FilmGrain") as string) + " (Alt+Shift+G)";
            _filmGrainCheck = filmGrain;
            panel.Children.Add(filmGrain);

            _menu.Items.Add(panel);
        }

        /// <summary>Alt+Shift+G: Film Grain on or off, kept in step with the flyout's check box.</summary>
        internal void ToggleFilmGrain()
        {
            FilmGrain.Set(!FilmGrain.Enabled);
            _filmGrainCheck?.IsChecked = FilmGrain.Enabled;
        }

        private void Theme_Checked(Theme theme)
        {
            if (theme == ThemeManager.Current) return;
            _applying = true;
            try
            {
                RunTransition(ThemeChangeTransition, () =>
                {
                    ThemeManager.Apply(theme);
                    DwmChrome.SetThemeBorder(_window);
                    ThemeApplied?.Invoke();
                });
            }
            finally { _applying = false; }
        }

        private string ThemeLabel(Theme theme) =>
            _window.TryFindResource("Str_Theme_" + theme) as string ?? theme.ToString();
    }
}
