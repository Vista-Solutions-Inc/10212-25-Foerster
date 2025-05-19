using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HalconDotNet;
using Jobs.BaseClasses.Execution;
using Jobs.BaseClasses;
using System.Windows.Media;
using VistaControls.ResultsNavigation;
using Jobs.ViewModels;
using Foerster.ViewModels.Pages;
using System.Windows.Threading;
using VistaHelpers.Log4Net;
using TaskToolBox.Tasks.Acquisition;
using TaskToolBox.BaseClases.Results;

namespace Foerster.ViewModels.VisualObjects
{
    public partial class StreamResultVisual : NavigationVisualItem
    {
        private JMStream _stream;
        private string _parentStepName;

        //-- Display control --//
        [ObservableProperty] HSmartWindowControlWPF _displayControl;
        [ObservableProperty] SolidColorBrush _borderBrush;
        [ObservableProperty] int _controlHeight = 135;
        [ObservableProperty] int _controlWidth = 180;
        //-- Visual objects containers --//
        [ObservableProperty] ObservableCollection<NavigationVisualItem> _taskResultVisuals;

        public string ParentStepName => _parentStepName;

        public StreamResultVisual(JMStream stream, string parentStepName) : base(stream.StreamID, stream.Status, stream.Result)
        {
            _stream = stream;
            _parentStepName = parentStepName;
            ItemName = _stream.StreamID;
            CreateDisplayControl();
            CreateTasksVisualCollection();
            SubscribeToStreamExecutionEvents();
        }

        #region Execution Event Handling
        /// <summary>
        /// Subscribes the step result visual to its underlying JMStep events.
        /// </summary>
        private void SubscribeToStreamExecutionEvents()
        {
            _stream.StreamExecutionErrorEvent += OnStreamExecutionError;
            _stream.StreamStatusChangedEvent += OnStreamStatusChanged;
        }
        private void OnStreamExecutionError(object? sender, EventArgs e)
        {
            StreamExecutionEventArgs streamEventArgs = e as StreamExecutionEventArgs;
        }
        private void OnStreamStatusChanged(object? sender, EventArgs e)
        {
            UpdateStatusAndResult(_stream.Status, _stream.Result);
        }
        #endregion

        #region Initialization
        private void CreateDisplayControl()
        {
            Application.Current.Dispatcher.BeginInvoke(
            DispatcherPriority.Background,
            new Action(() => {
                DisplayControl = new HSmartWindowControlWPF();
                DisplayControl.HZoomContent = HSmartWindowControlWPF.ZoomContent.Off;
                BorderBrush = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
            }));
        }
        private void CreateTasksVisualCollection()
        {
            TaskResultVisuals = [];
            List<BaseJMTask> tasks = _stream.TaskList;
            foreach (BaseJMTask task in tasks)
            {
                // Instantiate visual objects based on the task type
                TaskResultVisual taskResultVisual = CreateTaskResultVisual(task);
                TaskResultVisuals.Add(taskResultVisual);
            }
        }
        private TaskResultVisual CreateTaskResultVisual(BaseJMTask task)
        {
            // Calls the appropriate constructor based on the type of the 'task' parameter
            //Create A Task Result Visualizer
            return task switch
            {
                ImageCaptureTask => new ImageCaptureTaskResultVisual(task as ImageCaptureTask),
                _ => throw new ArgumentException("Stream visual creational error: Unknown type.")
            };
        }
        #endregion

        #region Updating
        public string UpdateStreamVisual()
        {
            // Update the status and result
            Application.Current.Dispatcher.BeginInvoke(
            DispatcherPriority.Background,
            new Action(() => {
                // Update status of the list of task visual objects
                foreach (var navigationItem in TaskResultVisuals)
                {
                    TaskResultVisual? taskVisual = navigationItem as TaskResultVisual;
                    if (taskVisual != null)
                    {
                        taskVisual.UpdateTaskResultVisual();
                    }
                }
            }));
            return "";            
        }
        #endregion

    }
}
