using System.Windows.Documents;
using VistaControls.UsersManagement;
using Foerster.Models.Managers;
using Foerster.Models.System;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Wpf.Ui.Abstractions.Controls;

namespace Foerster.ViewModels.Pages
{
    public partial class SettingsViewModel : ObservableObject, INavigationAware
    {
        // Fields 
        private SystemConfiguration _systemConfiguration;
        private UserManager _userManager;
        private ISnackbarService _snackbarService;
        private IContentDialogService _contentDialogService;
        private INavigationService _navigationService;
        private int _snackbarDuration = 5;

        // Properties 
        [ObservableProperty] private string _appVersion = String.Empty;
        [ObservableProperty] private ApplicationTheme _currentTheme = ApplicationTheme.Unknown;
        [ObservableProperty] private bool _isOnline = false;
        [ObservableProperty] private bool _isOffline = false;
        [ObservableProperty] private bool _isSemiauto = false;
        [ObservableProperty] private bool _debugEnabled = false;
        [ObservableProperty] private bool _captureOnlyMode = false;
        [ObservableProperty] private Visibility _progressRingVisibility = Visibility.Collapsed;
        [ObservableProperty] private bool _switchRunModeEnable = true;
        [ObservableProperty] private bool _executionControlsEnable = true;
        [ObservableProperty] private bool _advancedControlsEnable = true;
        public async Task OnNavigatedToAsync()
        {

            UpdateDisplay();
        }
        public async Task OnNavigatedFromAsync()
        {

        }

        public SettingsViewModel(ISnackbarService snackbarService, IContentDialogService contentDialogService, INavigationService navigationService)
        {
            // Initialize services
            _contentDialogService = contentDialogService;
            _snackbarService = snackbarService;
            _navigationService = navigationService;

            // Get managers instances
            _systemConfiguration = SystemConfiguration.Instance;
            _systemConfiguration.RunModeUpdateCompletedEvent += UpdateRunMode;
            UpdateDisplay();
            _userManager = App.GetService<UserManager>();
            _userManager.UserLevelUpdatedEvent += OnAccessLevelChanged;
            UpdateVisibilityByAccessLevel(_userManager.CurrentUserRole.ToString());

            // Set app version
            AppVersion = $"Foerster Inspection GUI - {GetAssemblyVersion()}";
        }

        private string GetAssemblyVersion()
        {
            string fVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? String.Empty;
            if (fVersion == string.Empty) { return fVersion; }

            //Remove last position of the default version format of x.x.x.x to match Vista Major.Minor.Patch convention
            return fVersion.Substring(0, fVersion.LastIndexOf('.'));
        }
        private void UpdateDisplay()
        {
            DebugEnabled = _systemConfiguration.DebugEnable;
            CaptureOnlyMode = _systemConfiguration.CaptureOnlyEnable;
            CurrentTheme = ApplicationThemeManager.GetAppTheme();
            RunMode currentMode = _systemConfiguration.RunMode;
            UpdateRunMode(this, new RunModeUpdateEventArgs(currentMode, currentMode));
        }
        private void OnAccessLevelChanged(object? sender, EventArgs e)
        {
            UserLevelEventArgs? args = e as UserLevelEventArgs;
            if (args == null) { return; }
            UpdateVisibilityByAccessLevel(args.UserLevel);
        }
        private void UpdateVisibilityByAccessLevel(string userLevel)
        {
            switch (userLevel)
            {
                case "Operator":
                    AdvancedControlsEnable = false;
                    ExecutionControlsEnable = false;
                    break;
                case "Supervisor":
                    ExecutionControlsEnable = true;
                    AdvancedControlsEnable = false;
                    break;
                case "Administrator":
                    ExecutionControlsEnable = true;
                    AdvancedControlsEnable = true;
                    break;
            }
        }

        private void ShowSnackBarMessage(string header, string message, bool isError)
        {
            ControlAppearance controlAppearance = ControlAppearance.Success;
            SymbolIcon symbolIcon = new SymbolIcon(SymbolRegular.CheckmarkCircle24);
            if (isError)
            {
                controlAppearance = ControlAppearance.Danger;
                symbolIcon = new SymbolIcon(SymbolRegular.ErrorCircle24);
            }
            _snackbarService.Show(header, message, controlAppearance, symbolIcon, TimeSpan.FromSeconds(_snackbarDuration));
        }
        // -- switch execution (Run) mode --
        private void UpdateRunMode(object? sender, EventArgs e)
        {
            RunModeUpdateEventArgs? args = e as RunModeUpdateEventArgs;
            if (args == null) { return; }
            RunMode previousRunMode = args.PrevRunMode;
            RunMode newRunMode = args.NewRunMode;
            switch (newRunMode)
            {
                case RunMode.online:
                    {
                        IsOnline = true;
                        IsSemiauto = false;
                        IsOffline = false;
                        break;
                    }
                case RunMode.semiauto:
                    {
                        IsOnline = false;
                        IsSemiauto = true;
                        IsOffline = false;
                        break;
                    }
                case RunMode.offline:
                    {
                        IsOffline = true;
                        IsOnline = false;
                        IsSemiauto = false;
                        break;
                    }
            }
            ProgressRingVisibility = Visibility.Collapsed;
            SwitchRunModeEnable = true;
            if (previousRunMode != newRunMode)
            {

                ShowSnackBarMessage("Run Mode Update", $"Run mode switchted to: {newRunMode.ToString()}", false);
            }
        }
        private void SetRunMode(RunMode runMode)
        {
            ProgressRingVisibility = Visibility.Visible;
            SwitchRunModeEnable = false;
            _systemConfiguration.SwitchRunMode(runMode);
        }
        partial void OnIsOnlineChanged(bool value)
        {
            if (value) { SetRunMode(RunMode.online); }
        }
        partial void OnIsSemiautoChanged(bool value)
        {
            if (value) { SetRunMode(RunMode.semiauto); }
        }
        partial void OnIsOfflineChanged(bool value)
        {
            if (value) { SetRunMode(RunMode.offline); }
        }

        // -- switch debug mode --
        partial void OnDebugEnabledChanged(bool value)
        {
            _systemConfiguration.DebugEnable = value;
        }

        // -- switch capture only mode --
        partial void OnCaptureOnlyModeChanged(bool value)
        {
            _systemConfiguration.CaptureOnlyEnable = value;
        }


        [RelayCommand]
        private void OnChangeTheme(string parameter)
        {
            switch (parameter)
            {
                case "theme_light":
                    if (CurrentTheme == ApplicationTheme.Light)
                        break;

                    ApplicationThemeManager.Apply(ApplicationTheme.Light);
                    CurrentTheme = ApplicationTheme.Light;

                    break;

                default:
                    if (CurrentTheme == ApplicationTheme.Dark)
                        break;

                    ApplicationThemeManager.Apply(ApplicationTheme.Dark);
                    CurrentTheme = ApplicationTheme.Dark;

                    break;
            }
        }
    }
}
