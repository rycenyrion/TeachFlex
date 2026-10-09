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
        private string _status = "Choose a theme, then select Apply Theme.";

        public ThemesViewModel()
        {
            ThemeOptions = new ObservableCollection<ThemeOption>
            {
                new ThemeOption("TeachFlex Green", "The familiar green sidebar and blue accents.", "#E8F5E9", "#2563EB", "#F4F7FB", "#FFFFFF", "#111827", "#64748B"),
                new ThemeOption("Ocean Blue", "A calm blue workspace with blue-gray surfaces.", "#E6F0FF", "#1D4ED8", "#F3F7FD", "#FFFFFF", "#152238", "#60718A"),
                new ThemeOption("Dark", "A darker workspace with comfortable contrast.", "#202A36", "#60A5FA", "#111827", "#1F2937", "#F3F4F6", "#B6C2D2")
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
                if (!File.Exists(_settingsPath)) return;
                var saved = JsonSerializer.Deserialize<ThemeSetting>(File.ReadAllText(_settingsPath));
                if (saved == null) return;
                foreach (var option in ThemeOptions)
                    if (string.Equals(option.Name, saved.Name, StringComparison.OrdinalIgnoreCase))
                        SelectedTheme = option;
                ApplyPalette(SelectedTheme);
                Status = "Saved theme loaded: " + SelectedTheme.Name;
            }
            catch
            {
                Status = "Default theme is active.";
            }
        }

        private void ApplyTheme()
        {
            try
            {
                ApplyPalette(SelectedTheme);
                Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
                File.WriteAllText(_settingsPath, JsonSerializer.Serialize(new ThemeSetting { Name = SelectedTheme.Name }));
                Status = "Theme applied: " + SelectedTheme.Name;
            }
            catch (Exception ex)
            {
                Status = "Could not save theme settings: " + ex.Message;
            }
        }

        private static void ApplyPalette(ThemeOption theme)
        {
            var resources = Application.Current.Resources;
            SetBrush(resources, "PrimaryBrush", theme.Primary);
            SetBrush(resources, "PrimaryDarkBrush", theme.PrimaryDark);
            SetBrush(resources, "PrimaryLightBrush", theme.PrimaryLight);
            SetBrush(resources, "BackgroundBrush", theme.Background);
            SetBrush(resources, "SurfaceBrush", theme.Surface);
            SetBrush(resources, "SidebarBrush", theme.Sidebar);
            SetBrush(resources, "SidebarHoverBrush", theme.SidebarHover);
            SetBrush(resources, "TextPrimaryBrush", theme.TextPrimary);
            SetBrush(resources, "TextSecondaryBrush", theme.TextSecondary);
            SetBrush(resources, "BorderBrush", theme.Border);
            SetBrush(resources, "SuccessBrush", theme.Success);
            SetBrush(resources, "WarningBrush", theme.Warning);
            SetBrush(resources, "DangerBrush", theme.Danger);

            SetColor(resources, "PrimaryColor", theme.Primary);
            SetColor(resources, "PrimaryDarkColor", theme.PrimaryDark);
            SetColor(resources, "PrimaryLightColor", theme.PrimaryLight);
            SetColor(resources, "BackgroundColor", theme.Background);
            SetColor(resources, "SurfaceColor", theme.Surface);
            SetColor(resources, "SidebarColor", theme.Sidebar);
            SetColor(resources, "SidebarHoverColor", theme.SidebarHover);
            SetColor(resources, "TextPrimaryColor", theme.TextPrimary);
            SetColor(resources, "TextSecondaryColor", theme.TextSecondary);
            SetColor(resources, "BorderColor", theme.Border);
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
                SidebarHover = name == "Dark" ? "#303C4B" : name == "Ocean Blue" ? "#D2E2FF" : "#D1EBD6";
                PrimaryDark = name == "Dark" ? "#3B82F6" : name == "Ocean Blue" ? "#1E40AF" : "#1D4ED8";
                PrimaryLight = name == "Dark" ? "#263B55" : name == "Ocean Blue" ? "#D7E6FF" : "#DBEAFE";
                Border = name == "Dark" ? "#374151" : name == "Ocean Blue" ? "#D5E0F0" : "#E2E8F0";
                Success = "#16A34A";
                Warning = "#D97706";
                Danger = "#DC2626";
            }
            public string Name { get; }
            public string Description { get; }
            public string Sidebar { get; }
            public string SidebarHover { get; }
            public string Primary { get; }
            public string PrimaryDark { get; }
            public string PrimaryLight { get; }
            public string Background { get; }
            public string Surface { get; }
            public string TextPrimary { get; }
            public string TextSecondary { get; }
            public string Border { get; }
            public string Success { get; }
            public string Warning { get; }
            public string Danger { get; }
        }

        private sealed class ThemeSetting
        {
            public string Name { get; set; } = "TeachFlex Green";
        }
    }
}