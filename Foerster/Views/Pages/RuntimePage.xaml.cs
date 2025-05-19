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
using System.Windows.Threading;
using Foerster.ViewModels.Pages;
using Foerster.ViewModels.VisualObjects;
using Foerster.Views.Pages;
using HalconDotNet;
using VistaControls.PerformanceMonitor;
using Wpf.Ui.Abstractions.Controls;

namespace Foerster.Views.Pages
{
    public enum GridDisplayMode
    {
        Small,
        Medium,
        Large
    }
    /// <summary>
    /// Interaction logic for RuntimePage.xaml
    /// </summary>
  
    public partial class RuntimePage : Page, INavigableView<RuntimeViewModel>
    {
        public RuntimeViewModel ViewModel { get; }
        public RuntimePage(RuntimeViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        /// <summary>
        /// This event handler recovers the content control data context, casts it to a <c>StreamResultVisual</c> object and
        /// updates the stream visual object. This function is required to make sure the <c>HSmartWindowControlWPF</c> has already
        /// been loaded and avoid null references to the <c>HSmartWindowControlWPF.HalconWindow</c> property.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private void ContentControl_Loaded(object sender, RoutedEventArgs e)
        {
            var contentControl = sender as ContentControl;
            var streamResultVisual = contentControl.DataContext as StreamResultVisual;
            streamResultVisual.UpdateStreamVisual();
        }


        private async void PerformanceMonitor_AddTask(object sender, RoutedEventArgs e) {
            int result = await ViewModel.AddPerformanceTaskDialog();
        }

        private void PerformanceMonitor_DeleteTask(object sender, RoutedEventArgs e) {
            DeleteTaskRoutedEventArgs args = (DeleteTaskRoutedEventArgs)e;
            ViewModel.NamePerformanceTaskDict[args.TaskName].Task.TaskExecutionTimeUpdatedEvent -= ViewModel.OnTaskExecutionTimeUpdated;
            ViewModel.AvailableTaskNameList.Add(args.TaskName);
            for (int i = 0; i < ViewModel.TaskList.Count; i++) {
                if (ViewModel.TaskList[i].TaskName == args.TaskName) {
                    ViewModel.TaskList.Remove(ViewModel.TaskList[i]);
                }
            }
        }

        private void PerformanceMonitor_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            ViewModel.SelectedTask = null;
        }
    }
}
