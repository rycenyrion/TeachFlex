using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TeachFlex.ViewModels
{
    public partial class EClassRecordViewModel
    {
        private string _selectedRecordMode =
            "Single Record";

        private string _componentOneName =
            "Component 1";

        private string _componentTwoName =
            "Component 2";

        private string _selectedComponent =
            "General";

        public IReadOnlyList<string>
            RecordModeOptions
        {
            get;
        } = new[]
        {
            "Single Record",
            "Per Component"
        };

        public ObservableCollection<string>
            ComponentOptions
        {
            get;
        } = new ObservableCollection<string>();

        public string SelectedRecordMode
        {
            get => _selectedRecordMode;

            set
            {
                string safeValue =
                    string.IsNullOrWhiteSpace(
                        value)
                        ? "Single Record"
                        : value.Trim();

                if (SetProperty(
                        ref _selectedRecordMode,
                        safeValue))
                {
                    UpdateComponentOptions();
                    ClearRecord();

                    OnPropertyChanged(
                        nameof(
                            CanEditComponentNames));

                    OnPropertyChanged(
                        nameof(
                            UsesComponentRecords));

                    OnPropertyChanged(
                        nameof(
                            ActiveComponentName));

                    NotifyCommandStates();
                }
            }
        }

        public string ComponentOneName
        {
            get => _componentOneName;

            set
            {
                string safeValue =
                    value?.Trim()
                    ?? string.Empty;

                if (SetProperty(
                        ref _componentOneName,
                        safeValue))
                {
                    UpdateComponentOptions();
                    ClearRecord();

                    OnPropertyChanged(
                        nameof(
                            ActiveComponentName));

                    NotifyCommandStates();
                }
            }
        }

        public string ComponentTwoName
        {
            get => _componentTwoName;

            set
            {
                string safeValue =
                    value?.Trim()
                    ?? string.Empty;

                if (SetProperty(
                        ref _componentTwoName,
                        safeValue))
                {
                    UpdateComponentOptions();
                    ClearRecord();

                    OnPropertyChanged(
                        nameof(
                            ActiveComponentName));

                    NotifyCommandStates();
                }
            }
        }

        public string SelectedComponent
        {
            get => _selectedComponent;

            set
            {
                string safeValue =
                    string.IsNullOrWhiteSpace(
                        value)
                        ? "General"
                        : value.Trim();

                if (SetProperty(
                        ref _selectedComponent,
                        safeValue))
                {
                    ClearRecord();

                    OnPropertyChanged(
                        nameof(
                            ActiveComponentName));

                    NotifyCommandStates();
                }
            }
        }

        public bool ShowsComponentSetup =>
            CurrentPolicy?
                .UsesMapehComponents == true ||
            CurrentPolicy?
                .SupportsEppTleComponents == true;

        public bool CanSelectRecordMode =>
            CurrentPolicy?
                .SupportsEppTleComponents == true;

        public bool CanEditComponentNames =>
            CurrentPolicy?
                .SupportsEppTleComponents == true &&
            SelectedRecordMode ==
                "Per Component";

        public bool UsesComponentRecords =>
            CurrentPolicy?
                .UsesMapehComponents == true ||
            (
                CurrentPolicy?
                    .SupportsEppTleComponents == true &&
                SelectedRecordMode ==
                    "Per Component"
            );

        public string ActiveComponentName =>
            UsesComponentRecords
                ? SelectedComponent
                : "General";

        private void ConfigureComponentPolicy()
        {
            if (CurrentPolicy?
                .UsesMapehComponents == true)
            {
                _selectedRecordMode =
                    "Per Component";

                _componentOneName =
                    "Music and Arts";

                _componentTwoName =
                    "PE and Health";
            }
            else if (CurrentPolicy?
                .SupportsEppTleComponents == true)
            {
                if (SelectedRecordMode !=
                        "Single Record" &&
                    SelectedRecordMode !=
                        "Per Component")
                {
                    _selectedRecordMode =
                        "Single Record";
                }

                if (string.IsNullOrWhiteSpace(
                        ComponentOneName) ||
                    ComponentOneName ==
                        "Music and Arts")
                {
                    _componentOneName =
                        "Component 1";
                }

                if (string.IsNullOrWhiteSpace(
                        ComponentTwoName) ||
                    ComponentTwoName ==
                        "PE and Health")
                {
                    _componentTwoName =
                        "Component 2";
                }
            }
            else
            {
                _selectedRecordMode =
                    "Single Record";

                _componentOneName =
                    "Component 1";

                _componentTwoName =
                    "Component 2";
            }

            UpdateComponentOptions();

            OnPropertyChanged(
                nameof(
                    SelectedRecordMode));

            OnPropertyChanged(
                nameof(
                    ComponentOneName));

            OnPropertyChanged(
                nameof(
                    ComponentTwoName));

            OnPropertyChanged(
                nameof(
                    SelectedComponent));

            OnPropertyChanged(
                nameof(
                    ShowsComponentSetup));

            OnPropertyChanged(
                nameof(
                    CanSelectRecordMode));

            OnPropertyChanged(
                nameof(
                    CanEditComponentNames));

            OnPropertyChanged(
                nameof(
                    UsesComponentRecords));

            OnPropertyChanged(
                nameof(
                    ActiveComponentName));

            NotifyCommandStates();
        }

        private void UpdateComponentOptions()
        {
            string previousSelection =
                SelectedComponent;

            ComponentOptions.Clear();

            if (CurrentPolicy?
                .UsesMapehComponents == true)
            {
                ComponentOptions.Add(
                    "Music and Arts");

                ComponentOptions.Add(
                    "PE and Health");
            }
            else if (CurrentPolicy?
                         .SupportsEppTleComponents ==
                     true &&
                     SelectedRecordMode ==
                         "Per Component")
            {
                if (!string.IsNullOrWhiteSpace(
                        ComponentOneName))
                {
                    ComponentOptions.Add(
                        ComponentOneName.Trim());
                }

                if (!string.IsNullOrWhiteSpace(
                        ComponentTwoName) &&
                    !ComponentOptions.Contains(
                        ComponentTwoName.Trim(),
                        StringComparer
                            .OrdinalIgnoreCase))
                {
                    ComponentOptions.Add(
                        ComponentTwoName.Trim());
                }
            }
            else
            {
                ComponentOptions.Add(
                    "General");
            }

            string nextSelection =
                ComponentOptions.FirstOrDefault(
                    component =>
                        component.Equals(
                            previousSelection,
                            StringComparison
                                .OrdinalIgnoreCase))
                ?? ComponentOptions.FirstOrDefault()
                ?? "General";

            _selectedComponent =
                nextSelection;

            OnPropertyChanged(
                nameof(
                    SelectedComponent));

            OnPropertyChanged(
                nameof(
                    ActiveComponentName));
        }
    }
}