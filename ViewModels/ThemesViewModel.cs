using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;

namespace TeachFlex.ViewModels
{
    public sealed class ThemesViewModel : ViewModelBase
    {
        private readonly string _settingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TeachFlex", "theme-settings.json");

        private ThemeOption _selectedTheme;
        private string _status = "TeachFlex Green is the recommended theme for consistent readability.";

        public ThemesViewModel()
        {
            ThemeOptions = new ObservableCollection<ThemeOption>
            {
                new ThemeOption(
                    "TeachFlex Green",
                    "Recommended for everyday school work. Uses the established TeachFlex green sidebar, light workspace, white cards, and blue actions.",
                    "#E8F5E9", "#2563EB", "#F4F7FB", "#FFFFFF", "#111827", "#64748B")
            };

            _selectedTheme = ThemeOptions[0];
            LoadSavedTheme();
            ApplyThemeCommand = new RelayCommand(ApplyTheme);
            SelectThemeCommand = new RelayCommand<ThemeOption>(SelectTheme);
        }

        public ObservableCollection<ThemeOption> ThemeOptions { get; }

        public ThemeOption SelectedTheme
        {
            get => _selectedTheme;
            set => SetProperty(ref _selectedTheme, value);
        }

        public string Status
        {
            get => _status;
            private set => SetProperty(ref _status, value);
        }

        public IRelayCommand ApplyThemeCommand { get; }
        public IRelayCommand<ThemeOption> SelectThemeCommand { get; }

        private void SelectTheme(ThemeOption? option)
        {
            if (option != null)
                SelectedTheme = option;
        }

        private void LoadSavedTheme()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var saved = JsonSerializer.Deserialize<ThemeSetting>(File.ReadAllText(_settingsPath));
                    if (saved != null &&
                        !string.Equals(saved.Name, "TeachFlex Green", StringComparison.OrdinalIgnoreCase))
                    {
                        Status = "Your previous theme is no longer applied. TeachFlex Green is selected to restore readable colors.";
                    }
                }

                ApplyPalette(SelectedTheme);
                SaveThemeSetting();
            }
            catch
            {
                ApplyPalette(SelectedTheme);
                Status = "TeachFlex Green is active. Theme settings could not be read.";
            }
        }

        private void ApplyTheme()
        {
            try
            {
                ApplyPalette(SelectedTheme);
                SaveThemeSetting();
                Status = "TeachFlex Green applied. This is the supported theme while other pages are being prepared for full theme compatibility.";
            }
            catch (Exception ex)
            {
                Status = "Could not save theme settings: " + ex.Message;
            }
        }

        private void SaveThemeSetting()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
            File.WriteAllText(
                _settingsPath,
                JsonSerializer.Serialize(new ThemeSetting { Name = SelectedTheme.Name }));
        }

        private static void ApplyPalette(ThemeOption theme)
        {
            var resources = Application.Current.Resources;
            SetBrush(resources, "PrimaryBrush", theme.Primary);
            SetBrush(resources, "PrimaryDarkBrush", "#1D4ED8");
            SetBrush(resources, "PrimaryLightBrush", "#DBEAFE");
            SetBrush(resources, "BackgroundBrush", theme.Background);
            SetBrush(resources, "SurfaceBrush", theme.Surface);
            SetBrush(resources, "SidebarBrush", theme.Sidebar);
            SetBrush(resources, "SidebarHoverBrush", "#D1EBD6");
            SetBrush(resources, "TextPrimaryBrush", theme.TextPrimary);
            SetBrush(resources, "TextSecondaryBrush", theme.TextSecondary);
            SetBrush(resources, "BorderBrush", "#E2E8F0");
            SetBrush(resources, "SuccessBrush", "#16A34A");
            SetBrush(resources, "WarningBrush", "#D97706");
            SetBrush(resources, "DangerBrush", "#DC2626");

            SetColor(resources, "PrimaryColor", theme.Primary);
            SetColor(resources, "PrimaryDarkColor", "#1D4ED8");
            SetColor(resources, "PrimaryLightColor", "#DBEAFE");
            SetColor(resources, "BackgroundColor", theme.Background);
            SetColor(resources, "SurfaceColor", theme.Surface);
            SetColor(resources, "SidebarColor", theme.Sidebar);
            SetColor(resources, "SidebarHoverColor", "#D1EBD6");
            SetColor(resources, "TextPrimaryColor", theme.TextPrimary);
            SetColor(resources, "TextSecondaryColor", theme.TextSecondary);
            SetColor(resources, "BorderColor", "#E2E8F0");
        }

        private static System.Windows.ResourceDictionary? FindDictionary(
            System.Windows.ResourceDictionary dictionary, string key)
        {
            if (dictionary.Contains(key))
                return dictionary;

            foreach (var merged in dictionary.MergedDictionaries)
            {
                var found = FindDictionary(merged, key);
                if (found != null)
                    return found;
            }

            return null;
        }

        private static void SetBrush(System.Windows.ResourceDictionary resources, string key, string hex)
        {
            var dictionary = FindDictionary(resources, key);
            if (dictionary == null) return;

            var color = (Color)ColorConverter.ConvertFromString(hex);
            if (dictionary[key] is SolidColorBrush brush && !brush.IsFrozen)
                brush.Color = color;
            else
                dictionary[key] = new SolidColorBrush(color);
        }

        private static void SetColor(System.Windows.ResourceDictionary resources, string key, string hex)
        {
            var dictionary = FindDictionary(resources, key);
            if (dictionary != null)
                dictionary[key] = (Color)ColorConverter.ConvertFromString(hex);
        }

        public sealed class ThemeOption
        {
            public ThemeOption(string name, string description, string sidebar, string primary, string background, string surface, string textPrimary, string textSecondary)
            {
                Name = name;
                Description = description;
                Sidebar = sidebar;
                Primary = primary;
                Background = background;
                Surface = surface;
                TextPrimary = textPrimary;
                TextSecondary = textSecondary;
                Border = "#E2E8F0";
            }

            public string Name { get; }
            public string Description { get; }
            public string Sidebar { get; }
            public string Primary { get; }
            public string Background { get; }
            public string Surface { get; }
            public string TextPrimary { get; }
            public string TextSecondary { get; }
            public string Border { get; }
        }

        private sealed class ThemeSetting
        {
            public string Name { get; set; } = "TeachFlex Green";
        }
    }
}