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
using Wpf.Ui.Controls;
using Foerster.ViewModels.Pages;

namespace Foerster.ViewModels.VisualObjects
{
    public partial class StepResultVisual : NavigationVisualItem
    {
        #region Events
        public event EventHandler StepExecutionEventOccurred;
        #endregion

        private JMStep _step;
        [ObservableProperty] ObservableCollection<StreamResultVisual> _streamResultVisuals;

        public int NumberOfTasks
        {
            get
            {
                int numberOfTasks = 0;
                foreach (var streamVisual in StreamResultVisuals)
                {
                    numberOfTasks += streamVisual.TaskResultVisuals.Count;
                }

                return numberOfTasks;
            }
        }

        #region Constructor
        public StepResultVisual(JMStep step) : base(step.StepID, step.Status, step.Result)
        {
            _step = step;
            SubscribeToStepExecutionEvents();
            CreateStreamsVisualCollection();
        }
        #endregion

        private void CreateStreamsVisualCollection()
        {
            StreamResultVisuals = [];
            List<JMStream> streams = _step.StreamList;
            foreach (JMStream stream in streams)
            {
                // Do not instantiate visual objects for initializer nor finalizer streams
                bool isEdgeStream = stream.Name == "Initializer" || stream.Name == "Finalizer";
                if (!isEdgeStream)
                {
                    var streamResultVisual = new StreamResultVisual(stream, _step.StepID);
                    StreamResultVisuals.Add(streamResultVisual);
                }
            }
        }

        /// <summary>
        /// Updates the visual data inside the step result visual object and its stream visual objects
        /// </summary>
        /// <returns><c>string.Empty</c> if successful. An error message otherwise.</returns>
        public string UpdateStepVisual()
        {
            // Update the status and result of the step
            //UpdateStatusAndResult(_step.Status, _step.Result);
            foreach (var streamResultVisual in StreamResultVisuals)
            {
                string error = streamResultVisual.UpdateStreamVisual();
                if (error != string.Empty)
                {
                    return error;
                }
            }

            return string.Empty;
        }

        #region Event Handling
        /// <summary>
        /// Subscribes the step result visual to its underlying JMStep events.
        /// </summary>
        public void SubscribeToStepExecutionEvents()
        {
            _step.StepExecutionCompletedEvent += OnStepExecutionCompletedEvent;
            _step.StepStatusChangedEvent += OnStepStatusChangedEvent;
            _step.StepExecutionErrorEvent += OnStepExecutionErrorEvent;
        }
        public void UnsubscribeToStepExecutionEvents() {
            _step.StepExecutionCompletedEvent -= OnStepExecutionCompletedEvent;
            _step.StepStatusChangedEvent -= OnStepStatusChangedEvent;
            _step.StepExecutionErrorEvent -= OnStepExecutionErrorEvent;
        }

        private void OnStepStatusChangedEvent(object? sender, EventArgs e)
        {
            UpdateStatusAndResult(_step.Status, _step.Result);
        }

        public void OnStepExecutionCompletedEvent(object sender, EventArgs e)
        {
            UpdateStepVisual();
        }
        public void OnStepExecutionErrorEvent(object sender, EventArgs e)
        {
            UpdateStepVisual();
        }
        #endregion
    }
}
