using Jobs.BaseClasses;
using Jobs.BaseClasses.Execution;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using VistaControls.PerformanceMonitor;
using VistaControls.ResultsNavigation;
using Foerster.Models.Managers;
using Foerster.Models.System;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Wpf.Ui.Abstractions.Controls;
using Foerster.ViewModels.VisualObjects;
using Foerster.Views.UserControls;


namespace Foerster.ViewModels.Pages
{
    public enum EmulationStatus
    {
        Start,
        Run,
        Halt,
        Complete,
        Disable
    }


    public partial class RuntimeViewModel : ObservableObject, INavigationAware
    {
        //-- Events --//


        // Fields
        private bool _showPerformanceExpanded = false;

        //-- Services --//
        private readonly IContentDialogService _contentDialogService;
        private readonly ISnackbarService _snackBarService;
        private readonly INavigationService _navigationService;
        private int _snackBarDuration = 4;
        public INavigationService NavigationService => _navigationService;

        //-- System --//
        private SystemManager _systemManager;
        private SystemConfiguration _sysConfig;
        private JobManager _jobManager;

        //-- Reference to the inspections page's view model --//


        //-- Execution mode indicators --//
        [ObservableProperty] bool _debugEnable;
        [ObservableProperty] bool _showEmulationControls;

        //-- Upper status bar --//
        [ObservableProperty] BaseJMJob _currentJob;
        [ObservableProperty] bool _progressBarEnable;
        [ObservableProperty] int _stepExecutionIndex;
        [ObservableProperty] ExecutionStatus _jobStatus;

        //-- Emulation controls --//
        [ObservableProperty] bool _startEnable;
        [ObservableProperty] bool _nextStepEnable;
        [ObservableProperty] bool _stopEnable;
        [ObservableProperty] bool _resetEnable;

        //-- Collections of visual objects --//
        [ObservableProperty] ObservableCollection<NavigationVisualItem> _stepResultVisuals;
        [ObservableProperty] NavigationVisualItem _selectedStepResult;
        [ObservableProperty] ObservableCollection<StreamResultVisual> _allStreamResultVisuals;

        //-- Performance Monitor --//
        [ObservableProperty] private Visibility _performanceGridVisibility = Visibility.Collapsed;
        [ObservableProperty] private Thickness _performanceMargin = new Thickness(5, 0, -30, 0);
        [ObservableProperty] private string _lastJob;
        [ObservableProperty] private string _avgJob;
        [ObservableProperty] private string _lastStep;
        [ObservableProperty] private string _avgStep;
        [ObservableProperty] List<PerformanceMonitorTask> _taskList = new List<PerformanceMonitorTask>();
        [ObservableProperty] PerformanceMonitorTask _selectedTask;
        [ObservableProperty] ObservableCollection<string> _availableTaskNameList = new ObservableCollection<string>();
        private Dictionary<string, PerformanceMonitorTask> _namePerformanceTaskDict = new Dictionary<string, PerformanceMonitorTask>();
        [ObservableProperty] string _selectedAvailableTaskName;
        private double _jobExecutionTimeSum; // Needs to be reset when job changes
        private long _jobExecutionCount; // Needs to be reset when job changes
        private double _stepExecutionTimeSum; // Needs to be reset when job changes
        private long _stepExecutionCount; // Needs to be reset when job changes




        public Dictionary<string, PerformanceMonitorTask> NamePerformanceTaskDict => _namePerformanceTaskDict;

        #region Constructor
        public RuntimeViewModel(ISnackbarService snackBarService, IContentDialogService contentDialogService, INavigationService navigationService)
        {
            // Initialize services
            _contentDialogService = contentDialogService;
            _snackBarService = snackBarService;
            _navigationService = navigationService;
            // Initialize system variables
            _systemManager = App.GetService<SystemManager>();
            _jobManager = _systemManager.JobManager;
            CurrentJob = _jobManager.CurrentJob;
            // Initialize the list of step result visuals
            _stepResultVisuals = [];
            _allStreamResultVisuals = [];
            // Subscribe to job-manager-related events
            SubscribeToJobManagerEvents();
            // Subscribe to system config changes
            _sysConfig = SystemConfiguration.Instance;
            EnableDisableDebugHelpers(_sysConfig.DebugEnable, _sysConfig.RunMode);
            SubscribeToSystemConfigEvents();
            if (_jobManager.CurrentJob == null) { return; }
            UpdateTaskList();
            _jobExecutionTimeSum = 0;
            _jobExecutionCount = 0;
            _stepExecutionTimeSum = 0;
            _stepExecutionCount = 0;


            //_taskList.Add(new PerformanceMonitorTask("Image Capture", "10.562", "10.242"));
            //_taskList.Add(new PerformanceMonitorTask("OD", "11.452", "12.562"));
        }

        #endregion

        public void UpdateTaskList()
        {
            TaskList = new List<PerformanceMonitorTask>();
            _namePerformanceTaskDict.Clear();
            AvailableTaskNameList.Clear();
            for (int i = 0; i < _jobManager.CurrentJob.StepList.Count; i++)
            {
                for (int j = 0; j < _jobManager.CurrentJob.StepList[i].StreamList.Count; j++)
                {
                    for (int k = 0; k < _jobManager.CurrentJob.StepList[i].StreamList[j].TaskList.Count; k++)
                    {
                        _namePerformanceTaskDict.Add(_jobManager.CurrentJob.StepList[i].StreamList[j].TaskList[k].Name, new PerformanceMonitorTask(_jobManager.CurrentJob.StepList[i].StreamList[j].TaskList[k].Name, "", "", _jobManager.CurrentJob.StepList[i].StreamList[j].TaskList[k]));
                        AvailableTaskNameList.Add(_jobManager.CurrentJob.StepList[i].StreamList[j].TaskList[k].Name);
                    }
                }
            }
        }

        #region Results Display
        partial void OnSelectedStepResultChanged(NavigationVisualItem value)
        {
            var stepVisualItem = value as StepResultVisual;
            if (stepVisualItem != null)
            {
                // Set the border brush for the stream visuals that match the step name
                string stepName = stepVisualItem.ItemName;
                foreach (var streamVisual in AllStreamResultVisuals)
                {
                    if (streamVisual.ParentStepName == stepName)
                    {
                        streamVisual.BorderBrush = new SolidColorBrush(Color.FromRgb(246, 92, 13));
                    }
                    else
                    {
                        streamVisual.BorderBrush = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
                    }
                }
            }
        }
        public void CreateStepsVisualCollection()
        {
            for (int i = 0; i < StepResultVisuals.Count; i++)
            {
                (StepResultVisuals[i] as StepResultVisual).UnsubscribeToStepExecutionEvents();
            }
            StepResultVisuals.Clear();
            if (CurrentJob == null) return;
            List<JMStep> steps = CurrentJob.StepList ?? [];
            foreach (JMStep step in steps)
            {
                // Instantiate visual objects
                var stepResultVisual = new StepResultVisual(step);
                // Subscribe to the step's execution events for non-empty steps
                if (stepResultVisual.StreamResultVisuals.Count > 0 && stepResultVisual.NumberOfTasks > 0)
                {
                    stepResultVisual.StepExecutionEventOccurred += OnStepExecutionEvent;
                    StepResultVisuals.Add(stepResultVisual);
                }
            }
            StepExecutionIndex = (StepResultVisuals.Count > 0) ? 0 : -1;
            InitializeAllStreamsVisualCollection();
        }
        public void UpdateStepsVisualCollection()
        {
            foreach (NavigationVisualItem navigationItem in StepResultVisuals)
            {
                StepResultVisual stepVisual = navigationItem as StepResultVisual;
                stepVisual.UpdateStepVisual();
            }
        }
        public void InitializeAllStreamsVisualCollection()
        {
            Application.Current.Dispatcher.BeginInvoke(
            DispatcherPriority.Background,
            new Action(() => {
                AllStreamResultVisuals.Clear();
                foreach (StepResultVisual stepResult in StepResultVisuals)
                {
                    foreach (StreamResultVisual streamResult in stepResult.StreamResultVisuals)
                    {
                        // Do not update visual objects for empty initializer nor finalizer streams
                        bool isEdgeStream = streamResult.ItemName == "Initializer" || streamResult.ItemName == "Finalizer";
                        if (!isEdgeStream)
                        {
                            AllStreamResultVisuals.Add(streamResult);
                        }
                        else
                        {
                            if (streamResult.TaskResultVisuals.Count > 0)
                            {
                                AllStreamResultVisuals.Add(streamResult);
                            }
                        }
                    }
                }
            }));
        }
        #endregion

        public async Task<int> AddPerformanceTaskDialog()
        { // Add snackBars
            if (_namePerformanceTaskDict.Count == 0) { return -1; }
            var newPerformanceTaskDialog = new AddPerformanceTaskDialog(_contentDialogService.GetDialogHost(), this);
            var result = await newPerformanceTaskDialog.ShowAsync();
            if (result == Wpf.Ui.Controls.ContentDialogResult.Primary)
            {
                _namePerformanceTaskDict[SelectedAvailableTaskName].Task.TaskExecutionTimeUpdatedEvent += OnTaskExecutionTimeUpdated;
                TaskList.Add(new PerformanceMonitorTask(_namePerformanceTaskDict[SelectedAvailableTaskName].TaskName, _namePerformanceTaskDict[SelectedAvailableTaskName].LastTask, _namePerformanceTaskDict[SelectedAvailableTaskName].AvgTask, _namePerformanceTaskDict[SelectedAvailableTaskName].Task));
                List<PerformanceMonitorTask> localTaskList = TaskList.ToList();
                TaskList = new List<PerformanceMonitorTask>(localTaskList);
                AvailableTaskNameList.Remove(SelectedAvailableTaskName);
                SelectedAvailableTaskName = null;
                return 1;
            }
            else
            {
                SelectedAvailableTaskName = null;
                return 0;
            }
        }

        [RelayCommand]
        private void OnChangePerformanceDisplay()
        {
            if (_showPerformanceExpanded)
            {
                _showPerformanceExpanded = false;
                PerformanceGridVisibility = Visibility.Collapsed;
                PerformanceMargin = new Thickness(5, 0, -30, 0);
            }
            else
            {
                _showPerformanceExpanded = true;
                PerformanceGridVisibility = Visibility.Visible;
                PerformanceMargin = new Thickness(5, 0, 0, 0);
            }
        }

        #region Emulation
        [RelayCommand]
        async Task OnNextStep()
        {
            // Retrieve step visual object
            var currentStepVisual = StepResultVisuals[StepExecutionIndex] as StepResultVisual;
            SwitchEmulationStatus(EmulationStatus.Run);
            // Execute step
            string? error = await CurrentJob.Step(CurrentJob.Part);
        }
        [RelayCommand]
        void OnStopJob()
        {
            CurrentJob.Stop();
        }
        [RelayCommand]
        void OnResetCycle()
        {
            SwitchEmulationStatus(EmulationStatus.Start);
            var currentJob = _jobManager.CurrentJob;
            if (currentJob == null) { return; }
            //StepExecutionIndex = 0;
            currentJob.Reset();
        }
        [RelayCommand]
        async Task OnRunJob()
        {
            // Run the current job
            SwitchEmulationStatus(EmulationStatus.Run);
            string? errorMessage = await CurrentJob.ExecuteAll(CurrentJob.Part);
            //SwitchEmulationStatus(EmulationStatus.Complete);
        }
        public void SwitchEmulationStatus(EmulationStatus newStatus)
        {
            switch (newStatus)
            {
                case EmulationStatus.Start:
                    StartEnable = true; NextStepEnable = true; StopEnable = false; ResetEnable = false;
                    break;
                case EmulationStatus.Run:
                    StartEnable = false; NextStepEnable = false; StopEnable = true; ResetEnable = false;
                    break;
                case EmulationStatus.Halt:
                    StartEnable = true; NextStepEnable = true; StopEnable = false; ResetEnable = true;
                    break;
                case EmulationStatus.Complete:
                    StartEnable = false; NextStepEnable = false; StopEnable = false; ResetEnable = true;
                    break;
                case EmulationStatus.Disable:
                    StartEnable = false; NextStepEnable = false; StopEnable = false; ResetEnable = false;
                    break;
                default:
                    StartEnable = false; NextStepEnable = false; StopEnable = false; ResetEnable = false;
                    break;
            }
        }
        #endregion

        #region Job Manager Events
        private void SubscribeToJobManagerEvents()
        {
            _jobManager.SetCurrentJobCompletedEvent += OnSetCurrentJobCompleted;
            _jobManager.ModifyJobCompletedEvent += OnModifyJobCompleted;
            _jobManager.ModifyJobStepCompletedEvent += OnModifyJobStepCompleted;
            _jobManager.ModifyJobStreamCompletedEvent += OnModifyJobStreamCompleted;
            _jobManager.ModifyJobTaskCompletedEvent += OnModifyJobTaskCompleted;
            _jobManager.RestartJobCompletedEvent += OnRestartJobCompleted;
            _jobManager.JobStatusChangedEvent += OnJobStatusChanged;
            // Error handling
            _jobManager.SetCurrentJobErrorEvent += OnSetCurrentJobError;
            // Performance Monitor
            _jobManager.JobExecutionTimeUpdatedEvent += OnJobExecutionTimeUpdated;
            _jobManager.CurrentStepExecutionTimeUpdatedEvent += OnCurrentStepExecutionTimeUpdated;
        }

        #region Time execution
        public void OnJobExecutionTimeUpdated(object sender, EventArgs e)
        {
            ExecutionTimeEventArgs args = (ExecutionTimeEventArgs)e;
            LastJob = ConvertStringToFiveDigits(args.Time);
            _jobExecutionTimeSum = _jobExecutionTimeSum + double.Parse(args.Time);
            _jobExecutionCount++;
            AvgJob = ConvertStringToFiveDigits((_jobExecutionTimeSum / _jobExecutionCount).ToString());
        }
        public void OnCurrentStepExecutionTimeUpdated(object sender, EventArgs e)
        {
            ExecutionTimeEventArgs args = (ExecutionTimeEventArgs)e;
            LastStep = ConvertStringToFiveDigits(args.Time);
            _stepExecutionTimeSum = _stepExecutionTimeSum + double.Parse(args.Time);
            _stepExecutionCount++;
            AvgStep = ConvertStringToFiveDigits((_stepExecutionTimeSum / _stepExecutionCount).ToString());
        }
        public void OnTaskExecutionTimeUpdated(object sender, EventArgs e)
        {
            ExecutionTimeEventArgs args = (ExecutionTimeEventArgs)e;
            BaseJMTask task = sender as BaseJMTask;
            for (int i = 0; i < TaskList.Count; i++)
            {
                if (TaskList[i].TaskName == task.TaskName)
                {
                    TaskList[i].LastTask = ConvertStringToFiveDigits(args.Time);
                    TaskList[i].TaskExecutionTimeSum = TaskList[i].TaskExecutionTimeSum + double.Parse(args.Time);
                    TaskList[i].TaskExecutionCount++;
                    TaskList[i].AvgTask = ConvertStringToFiveDigits((TaskList[i].TaskExecutionTimeSum / TaskList[i].TaskExecutionCount).ToString());
                    List<PerformanceMonitorTask> localTaskList = TaskList.ToList();
                    TaskList = new List<PerformanceMonitorTask>(localTaskList);
                    break;
                }
            }
        }
        #endregion

        public void OnSetCurrentJobCompleted(object sender, EventArgs e)
        {
            ChangeJobEventArgs changeJobEventArgs = e as ChangeJobEventArgs;
            string jobName = changeJobEventArgs.Jobname;
            if (string.IsNullOrEmpty(jobName)) { CurrentJob = null; return; }
            Application.Current.Dispatcher.BeginInvoke(
            DispatcherPriority.Background,
            new Action(() => {
                if (_jobManager.JobsList.Count > 0)
                {
                    CurrentJob = _jobManager.JobsList[jobName];
                    _jobExecutionTimeSum = 0;
                    _jobExecutionCount = 0;
                    _stepExecutionTimeSum = 0;
                    _stepExecutionCount = 0;
                    for (int i = 0; i < TaskList.Count; i++)
                    {
                        TaskList[i].Task.TaskExecutionTimeUpdatedEvent -= OnTaskExecutionTimeUpdated;
                        TaskList[i].TaskExecutionTimeSum = 0;
                        TaskList[i].TaskExecutionCount = 0;
                    }
                    UpdateTaskList();
                }
                CreateStepsVisualCollection();
            }));
        }
        #region Job Modification 
        private void OnModifyJobCompleted(object? sender, EventArgs e)
        {
            CreateStepsVisualCollection();
            InitializeAllStreamsVisualCollection();
        }
        private void OnModifyJobStepCompleted(object? sender, EventArgs e)
        {
            CreateStepsVisualCollection();
            InitializeAllStreamsVisualCollection();
        }
        private void OnModifyJobStreamCompleted(object? sender, EventArgs e)
        {
            CreateStepsVisualCollection();
            InitializeAllStreamsVisualCollection();
        }
        private void OnModifyJobTaskCompleted(object? sender, EventArgs e)
        {
            CreateStepsVisualCollection();
            InitializeAllStreamsVisualCollection();
        }
        #endregion
        public void OnRestartJobCompleted(object sender, EventArgs e)
        {
            StepExecutionIndex = 0;
            SwitchEmulationStatus(EmulationStatus.Start);
            UpdateStepsVisualCollection();
        }
        
        public void OnJobStatusChanged(object sender, EventArgs e)
        {
            if (CurrentJob == null) JobStatus = ExecutionStatus.NotConfigured;
            else  JobStatus = CurrentJob.Status;
            switch (JobStatus)
            {
                case ExecutionStatus.NotConfigured:
                    ProgressBarEnable = false;
                    SwitchEmulationStatus(EmulationStatus.Disable);
                    break;
                case ExecutionStatus.Ready:
                    ProgressBarEnable = false;
                    if (StepExecutionIndex == 0)
                    {
                        SwitchEmulationStatus(EmulationStatus.Start);
                    }
                    else
                    {
                        SwitchEmulationStatus(EmulationStatus.Halt);
                    }
                    break;
                case ExecutionStatus.Running:
                    ProgressBarEnable = true;
                    SwitchEmulationStatus(EmulationStatus.Run);
                    break;
                case ExecutionStatus.Stopped:
                    ProgressBarEnable = false;
                    SwitchEmulationStatus(EmulationStatus.Halt);
                    break;
                case ExecutionStatus.Complete:
                    ProgressBarEnable = false;
                    SwitchEmulationStatus(EmulationStatus.Complete);
                    break;
                case ExecutionStatus.Error:
                    ProgressBarEnable = false;
                    SwitchEmulationStatus(EmulationStatus.Disable);
                    break;
            }
        }
        public void OnStepExecutionEvent(object sender, EventArgs e)
        {
            JobStatus = CurrentJob.Status;
        }
        public string ConvertStringToFiveDigits(string time)
        {
            // Input comes in ms without decimal point
            double inputTime = double.Parse(time);
            inputTime = inputTime / 1000;
            string outputTime = inputTime.ToString();
            while (outputTime.Length > 6)
            {
                outputTime = outputTime.Remove(outputTime.Length - 1);
            }
            return outputTime;
        }


        // Error handling
        public void OnSetCurrentJobError(object sender, EventArgs e)
        {
            JobManagementErrorEventArgs args = (JobManagementErrorEventArgs)e;
            Application.Current.Dispatcher.BeginInvoke(
            DispatcherPriority.Background,
            new Action(() => {
                _snackBarService.Show("Set Current Job", "Failed due to: " + args.ErrorMessage, ControlAppearance.Danger, new SymbolIcon(SymbolRegular.ErrorCircle24), TimeSpan.FromSeconds(_snackBarDuration));
            }));
        }
        #endregion

        #region Page Navigation
        public async Task OnNavigatedFromAsync()
        {
        }
        public async Task OnNavigatedToAsync()
        {
            CurrentJob = _jobManager.CurrentJob;
            EmulationStatus emulationStatus;
            if (CurrentJob != null)
            {
                JobStatus = CurrentJob.Status;
                if (StepResultVisuals.Count < 1)
                {
                    CreateStepsVisualCollection();
                }
                else
                {
                    UpdateStepsVisualCollection();
                }
                switch (CurrentJob.Status)
                {
                    case ExecutionStatus.NotConfigured:
                        emulationStatus = EmulationStatus.Disable;
                        break;
                    case ExecutionStatus.Ready:
                        emulationStatus = EmulationStatus.Start;
                        break;
                    case ExecutionStatus.Running:
                        emulationStatus = EmulationStatus.Run;
                        break;
                    case ExecutionStatus.Stopped:
                        emulationStatus = EmulationStatus.Halt;
                        break;
                    case ExecutionStatus.Complete:
                        emulationStatus = EmulationStatus.Complete;
                        break;
                    case ExecutionStatus.Error:
                        emulationStatus = EmulationStatus.Complete;
                        break;
                    default:
                        emulationStatus = EmulationStatus.Start;
                        break;
                }
                SwitchEmulationStatus(emulationStatus);
                if (_namePerformanceTaskDict.Count == 0)
                {
                    UpdateTaskList();
                }
            }
            else
            {
                SwitchEmulationStatus(EmulationStatus.Disable);
            }
        }
        #endregion

        #region Debug
        public void OnDebugModeUpdated(object sender, EventArgs e)
        {
            // Show/Hide the debug helpers
            EnableDisableDebugHelpers(_sysConfig.DebugEnable, _sysConfig.RunMode);
        }
        public void OnRunModeUpdated(object sender, EventArgs e)
        {
            // Show/Hide the debug helpers
            EnableDisableDebugHelpers(_sysConfig.DebugEnable, _sysConfig.RunMode);
        }
        private void EnableDisableDebugHelpers(bool isDebugModeEnabled, RunMode runMode)
        {
            // Set the value of the debug flag
            DebugEnable = isDebugModeEnabled;
            // Set the value of the emulation controls' visibility flag
            if (_systemManager.RunMode == RunMode.offline)
            {
                ShowEmulationControls = true;
            }
            else if (_systemManager.RunMode == RunMode.online)
            {
                ShowEmulationControls = (DebugEnable) ? true : false;
            }
        }
        private void SubscribeToSystemConfigEvents()
        {
            _sysConfig.DebugModeUpdatedEvent += OnDebugModeUpdated;
            _sysConfig.RunModeUpdateCompletedEvent += OnRunModeUpdated;
        }
        #endregion



    }
}
