using Foerster.ViewModels.Pages;
using Wpf.Ui.Controls;
using Wpf.Ui.Abstractions.Controls;

namespace Foerster.Views.Pages
{
    public partial class SystemPage : INavigableView<SystemViewModel>
    {
        public SystemViewModel ViewModel { get; }
        public SystemPage(SystemViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();

        }
    }
}
