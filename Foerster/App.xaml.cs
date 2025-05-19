using Jobs.ViewModels.Pages;
using Jobs.Views.Pages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Reflection;
using System.Windows.Threading;
using Foerster.Models.Managers;
using Foerster.Services;
using Foerster.ViewModels.Pages;
using Foerster.ViewModels.Windows;
using Foerster.Views.Pages;
using Foerster.Views.Windows;
using Wpf.Ui;
using VistaHelpers.Log4Net;
using Wpf.Ui.Abstractions;
using TaskToolBox.Tasks.Acquisition;


namespace Foerster
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        // The.NET Generic Host provides dependency injection, configuration, logging, and other services.
        // https://docs.microsoft.com/dotnet/core/extensions/generic-host
        // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
        // https://docs.microsoft.com/dotnet/core/extensions/configuration
        // https://docs.microsoft.com/dotnet/core/extensions/logging

        private Mutex _instanceMutex;
        private SystemManager _systemManager;
        private static readonly IHost _host = Host
            .CreateDefaultBuilder()
            .ConfigureAppConfiguration(c => { c.SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)); })
            .ConfigureServices((context, services) =>
            {
                services.AddHostedService<ApplicationHostService>();

                // Page resolver service
                services.AddSingleton<INavigationViewPageProvider, PageService>();

                // Theme manipulation
                services.AddSingleton<IThemeService, ThemeService>();

                // TaskBar manipulation
                services.AddSingleton<ITaskBarService, TaskBarService>();

                // SnackBar manipulation
                services.AddSingleton<IContentDialogService, ContentDialogService>();   
                services.AddSingleton<ISnackbarService, SnackbarService>();

                // Service containing navigation, same as INavigationWindow... but without window
                services.AddSingleton<INavigationService, NavigationService>();

                // Main window with navigation
                services.AddSingleton<INavigationWindow, MainWindow>();
                services.AddSingleton<MainWindowViewModel>();
                // Main Pages
                services.AddSingleton<SystemPage>();
                services.AddSingleton<SystemViewModel>();

                services.AddSingleton<SettingsPage>();
                services.AddSingleton<SettingsViewModel>();

                services.AddSingleton<RuntimePage>();
                services.AddSingleton<RuntimeViewModel>();

                services.AddSingleton<JobsPage>();
                services.AddSingleton<JobsViewModel>();

                services.AddSingleton<UserPage>();
                services.AddSingleton<UserViewModel>();
                // Configuration Pages
                services.AddSingleton<ImageCaptureTaskConfigPage>();
                services.AddSingleton<ImageCaptureTaskConfigViewModel>();
                // System Manager 
                services.AddSingleton<UserManager>();
                services.AddSingleton<SystemManager>();
            }).Build();

        /// <summary>
        /// Gets registered service.
        /// </summary>
        /// <typeparam name="T">Type of the service to get.</typeparam>
        /// <returns>Instance of the service or <see langword="null"/>.</returns>
        public static T GetService<T>()
            where T : class
        {
            return _host.Services.GetService(typeof(T)) as T;
        }

        /// <summary>
        /// Occurs when the application is loading.
        /// </summary>
        private void OnStartup(object sender, StartupEventArgs e)
        {
            // To prevent a second app instance is created if already running
            bool createdNew;
            _instanceMutex = new Mutex(true, @"Global\ControlPanel", out createdNew);
            if (!createdNew)
            {
                MessageBox.Show($"Another instance of {Assembly.GetExecutingAssembly().GetName().Name} is already running" );
                _instanceMutex = null;
                Application.Current.Shutdown();
                return;
            }

            //SetUp Log 4 Net
            //Configure initial logs
            //Configure the system loggers
            VistaLogger.dumpOnFatal = true;

            //VistaDumpHelper.dumpDirectory = Application.StartupPath;
            ////Subscribe the unhandle exceptions and thread exceptions to be handle , and thus logged
            ////Below only logs them, if you eant to do something wit it, create handler fuctions
            //Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(VistaLogger.logThreadExceptions);
            ////The line below forces unhandled exepctions throgh the handles defined here
            //Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);


            //Unhandled Exception handler
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(VistaLogger.logUnhandledExceptions);



            // Initialize system Manager before GUI
            _systemManager = GetService<SystemManager>();

            _host.Start();
        }

        /// <summary>
        /// Occurs when the application is closing.
        /// </summary>
        private async void OnExit(object sender, ExitEventArgs e)
        {
            await _host.StopAsync();
            _host.Dispose();

            if (_instanceMutex != null)
                _instanceMutex.ReleaseMutex();
        }

        /// <summary>
        /// Occurs when an exception is thrown by an application but not handled.
        /// </summary>
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // For more info see https://docs.microsoft.com/en-us/dotnet/api/system.windows.application.dispatcherunhandledexception?view=windowsdesktop-6.0
        }
    }
}
