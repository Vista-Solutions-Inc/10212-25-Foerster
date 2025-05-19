using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foerster.Models.System;
using Wpf.Ui;
using Foerster.Models.Managers;
using VistaControls.UsersManagement.Authenticator;
using VistaControls.UsersManagement;

namespace Foerster.ViewModels.Pages {
    public partial class UserViewModel : ObservableObject {
        // Fields
        private readonly ISnackbarService _snackbarService;
        private UserManager _userManager;
 
        // Properties
        public UserManager UserManager { get { return _userManager; } }
        public ISnackbarService SnackbarService { get { return _snackbarService; } }

        [ObservableProperty]
        string _path;

        [ObservableProperty]
        UserRole _currentUserRole;

        [ObservableProperty]
        bool _userLevelChangeSuccessful;

        [ObservableProperty]
        bool _passwordChangeSuccessful;

        // Constructor
        public UserViewModel(ISnackbarService snackbarService) {
            _userManager = App.GetService<UserManager>();
            Path = SystemConfiguration.Instance.AuthenticationPath;
            _snackbarService = snackbarService;
            _currentUserRole = UserRole.Operator;
            UserManager.OnUserLevelUpdated(this, new UserLevelEventArgs("Operator"));
        }

    }

}
