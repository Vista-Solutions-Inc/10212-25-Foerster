using System.Collections.ObjectModel;
using VistaControls.UsersManagement;
using VistaHelpers.Log4Net;
using Foerster.Models.Managers;
using Foerster.Views.Pages;
using Wpf.Ui;
using Wpf.Ui.Controls;
using VistaHelpers.Log4Net;
using static VistaHelpers.Log4Net.NotifyAppender;
using Foerster.ViewModels.VisualObjects;
using Foerster.Models.System;


namespace Foerster.ViewModels.Windows
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private UserManager _userManager;
        private SystemConfiguration _sysConfig;

        [ObservableProperty]
        private string _applicationTitle = "Foerster";

        [ObservableProperty]
        private ObservableCollection<object> _menuItems = new()
        {
            new NavigationViewItem()
            {
                Content = "Jobs",
                Icon = new SymbolIcon { Symbol = SymbolRegular.ClipboardBulletListLtr20 },
                TargetPageType = typeof(Jobs.Views.Pages.JobsPage),
            },
            new NavigationViewItem()
            {
                Content = "Run",
                Icon = new SymbolIcon { Symbol = SymbolRegular.PlayCircle24 },
                TargetPageType = typeof(Views.Pages.RuntimePage)
            },
            new NavigationViewItem()
            {
                Content = "System",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Scan24 },
                TargetPageType = typeof(Views.Pages.SystemPage)
            },
             new NavigationViewItem()
            {
                Content = "Users",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Person24 },
                TargetPageType = typeof(Views.Pages.UserPage)
            },
        };


        [ObservableProperty]
        private ObservableCollection<object> _footerMenuItems = new()
        {
            new NavigationViewItem()
            {
                Content = "Settings",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Settings24 },
                TargetPageType = typeof(Views.Pages.SettingsPage)
            }
        };
        [ObservableProperty] private string _currentUserRole = string.Empty;

        [ObservableProperty]
        private ObservableCollection<MenuItem> _trayMenuItems = new()
        {
            new MenuItem { Header = "Home", Tag = "tray_home" }
        };

        // Constructor 
        public MainWindowViewModel() {
            _userManager = App.GetService<UserManager>();
            CurrentUserRole = _userManager.CurrentUserRole.ToString();
            _userManager.UserLevelUpdatedEvent += OnUserRoleChanged;
            NotifyAppender.LogEventOccurred += NotifyAppender_LogEventOccurred;

            _sysConfig = SystemConfiguration.Instance;
            _sysConfig.DebugModeUpdatedEvent += _sysConfig_DebugModeUpdatedEvent;
            _sysConfig_DebugModeUpdatedEvent(this, new EventArgs());

        }



        // Methods 
        private void OnUserRoleChanged(object sender, EventArgs e) {
            UserLevelEventArgs args = (UserLevelEventArgs)e;
            CurrentUserRole = args.UserLevel;
        }
        [RelayCommand]
        private void OnNavigateToUserPage(object content)
        {
            INavigationWindow _navigationWindow = App.GetService<INavigationWindow>();
            _navigationWindow.Navigate(typeof(UserPage));
        }


        private void _sysConfig_DebugModeUpdatedEvent(object? sender, EventArgs e)
        {
            DebugEnable = _sysConfig.DebugEnable;
        }


        [ObservableProperty] bool _debugEnable;
        [ObservableProperty] private ObservableCollection<LogEntryVisual> _systemMessages = new ObservableCollection<LogEntryVisual>();
        private void NotifyAppender_LogEventOccurred(object? sender, NotifyAppender.LogEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
            {
                // get data form event args 
                LogEventArgs? eventArgs = e as LogEventArgs;
                if (eventArgs == null) { return; }
                SystemMessages.Insert(0, new LogEntryVisual(eventArgs));
            });
        }
    }
}
