using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Controls;
using Wpf.Ui.Abstractions.Controls;
using Foerster.ViewModels.Pages;
using VistaControls.UsersManagement;

namespace Foerster.Views.Pages {
    /// <summary>
    /// Interaction logic for UserPage.xaml
    /// </summary>
    public partial class UserPage : INavigableView<UserViewModel> {
        public UserViewModel ViewModel { get; }

        public UserPage(UserViewModel viewModel) {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        private void UserManagement_UserRoleChanged(object sender, RoutedEventArgs e) {
            if (UserControl.UserLevelChangeSuccessful) {
                ViewModel.SnackbarService.Show("User Role Changed", $"Active User Role changed to:{UserControl.CurrentUserRole.ToString()}", Wpf.Ui.Controls.ControlAppearance.Success, new SymbolIcon(SymbolRegular.PersonAvailable24), TimeSpan.FromSeconds(3));
            }
            else {
                ViewModel.SnackbarService.Show("User Role Change Failed", $"Incorrect User/Password combination", Wpf.Ui.Controls.ControlAppearance.Danger, new SymbolIcon(SymbolRegular.PersonDelete24), TimeSpan.FromSeconds(3));
            }
            ViewModel.UserManager.OnUserLevelUpdated(this, new UserLevelEventArgs(UserControl.CurrentUserRole.ToString()));
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            UserControl.Path = ViewModel.Path;
        }

        private void UserControl_PasswordChanged(object sender, RoutedEventArgs e) {
            if (UserControl.PasswordChangeSuccessful) {
                ViewModel.SnackbarService.Show("Password Change", "Password Changed Successfully", Wpf.Ui.Controls.ControlAppearance.Success, new SymbolIcon(SymbolRegular.PersonAvailable24), TimeSpan.FromSeconds(3));
            }
            else {
                ViewModel.SnackbarService.Show("Password Change", "Password Change failed", Wpf.Ui.Controls.ControlAppearance.Danger, new SymbolIcon(SymbolRegular.PersonDelete24), TimeSpan.FromSeconds(3));
            }
        }
    }
}
