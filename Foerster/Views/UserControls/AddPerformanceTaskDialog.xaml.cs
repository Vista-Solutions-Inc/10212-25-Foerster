using Foerster.ViewModels.Pages;
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

namespace Foerster.Views.UserControls {
    /// <summary>
    /// Interaction logic for AddPerformanceTaskDialog.xaml
    /// </summary>
    public partial class AddPerformanceTaskDialog : ContentDialog {
        public RuntimeViewModel ViewModel { get; }
        public AddPerformanceTaskDialog(ContentPresenter contentPresenter, RuntimeViewModel viewModel) : base(contentPresenter) {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        protected override void OnButtonClick(ContentDialogButton button) {
            if (ContentDialogButton.Primary == button) {
                if (AvailableTaskComboBox.SelectedValue != null) {
                    base.OnButtonClick(button);
                    return;
                }

                ErrorMessageTextBlock.Visibility = Visibility.Visible;
                return;
            }
            else {
                base.OnButtonClick(button);
            }
        }
        private void ContentDialog_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            e.Handled = (e.Key == Key.Enter);
        }
    }
}
