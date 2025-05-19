using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VistaControls.UsersManagement.Authenticator;
using VistaControls.UsersManagement;
using VistaHelpers.CustomExtensions;

namespace Foerster.Models.Managers {
    public class UserManager {

        // Events
        public event EventHandler UserLevelUpdatedEvent;

        // Fields 
        private UserRole _currentUserRole;
        private AuthenticatorServiceTypes _currentAuthenticationServiceType;

        //Properties
        public UserRole CurrentUserRole { get { return _currentUserRole; } set { _currentUserRole = value; } }
        public AuthenticatorServiceTypes CurrentAuthenticationServiceType { get { return _currentAuthenticationServiceType; } }

        // Constructor
        public UserManager() {
            _currentUserRole = UserRole.Operator;
        }

        public void OnUserLevelUpdated(object sender, UserLevelEventArgs e) {
            _currentUserRole = e.UserLevel.GetEnumFromString<UserRole>();
            UserLevelUpdatedEvent?.Invoke(this, e);
        }
    }
}
