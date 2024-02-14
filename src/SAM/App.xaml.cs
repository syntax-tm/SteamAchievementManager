using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using log4net;
using SAM.Core;

namespace SAM;

public partial class App
{
	protected readonly ILog Log = LogManager.GetLogger(nameof(App));

	protected override void OnStartup (StartupEventArgs e)
	{
		base.OnStartup(e);

		try
		{
			Log.Info("Application startup.");

			SplashScreenHelper.Show("Starting up...");

			SAMHelper.VerifySteamProcess();

			// handle any WPF dispatcher exceptions
			Current.DispatcherUnhandledException += OnDispatcherException;

			// handle any AppDomain exceptions
			var current = AppDomain.CurrentDomain;
			current.UnhandledException += OnAppDomainException;

			// handle any TaskScheduler exceptions
			TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

			// create the default Client instance
			SteamClientManager.Init(0);

			SteamLibraryManager.Init();

			MainWindow = new MainWindow();
			MainWindow.Show();

			ShutdownMode = ShutdownMode.OnMainWindowClose;
		}
		catch (Exception ex)
		{
			var message = $"An error occurred on application startup. {ex.Message}";

			Log.Error(message, ex);

			MessageBox.Show(message, @"SAM Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);

			Environment.Exit(SAMExitCode.UnhandledException);
		}
		finally
		{
			SplashScreenHelper.Close();
		}
	}

	protected override void OnExit (ExitEventArgs e)
	{
		base.OnExit(e);

		try
		{
			Log.Info(@"Application exiting. Ending any running manager processes...");

			SAMHelper.CloseAllManagers();
		}
		catch (Exception ex)
		{
			Log.Fatal($"An error occurred attempting to exit the SAM Managers. {ex.Message}", ex);
		}
	}

	private void OnUnobservedTaskException (object sender, UnobservedTaskExceptionEventArgs args)
	{
		try
		{
			var exception = args.Exception ?? throw new ArgumentNullException(nameof(args));

			var message = $"An unobserved task exception occurred. {exception.Message}";

			Log.Error(message, args.Exception);

			MessageBox.Show(message, $"Unhandled ${exception.GetType().Name}", MessageBoxButton.OK, MessageBoxImage.Error);

			args.SetObserved();
		}
		catch (Exception e)
		{
			Log.Fatal($"An error occurred in {nameof(OnUnobservedTaskException)}. {e.Message}", e);

			Environment.Exit(SAMExitCode.TaskException);
		}
	}

	private void OnAppDomainException (object sender, UnhandledExceptionEventArgs args)
	{
		try
		{
			var exception = (Exception) args.ExceptionObject;
			var message = $"Dispatcher unhandled exception occurred. {exception.Message}";

			Log.Fatal(message, exception);

			MessageBox.Show(message, $"Unhandled ${exception.GetType().Name}", MessageBoxButton.OK, MessageBoxImage.Error);
		}
		catch (Exception e)
		{
			Log.Fatal($"An error occurred in {nameof(OnAppDomainException)}. {e.Message}", e);
		}
		finally
		{
			Environment.Exit(SAMExitCode.AppDomainException);
		}
	}

	private void OnDispatcherException (object sender, DispatcherUnhandledExceptionEventArgs args)
	{
		try
		{
			var message = $"Dispatcher unhandled exception occurred. {args.Exception.Message}";

			Log.Fatal(message, args.Exception);

			Environment.Exit(SAMExitCode.DispatcherException);
		}
		catch (Exception e)
		{
			var message = $"An error occurred in {nameof(OnDispatcherException)}. {e.Message}";

			Environment.FailFast(message);
		}
	}
}
