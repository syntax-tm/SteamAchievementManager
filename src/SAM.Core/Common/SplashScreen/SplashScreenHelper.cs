using Wpf.Ui.Controls;

namespace SAM.Core;

public static class SplashScreenHelper
{
	private static bool _isInitialized;
	private static UiWindow _splashWindow;
	private static SplashScreenViewModel _splashScreenVm;

	public static void Init ()
	{
		if (_isInitialized)
			{
			throw new SAMException();
		}

		_splashScreenVm = SplashScreenViewModel.Create();

		_splashWindow = new SplashScreenView();
		_splashWindow.DataContext = _splashScreenVm;

		_isInitialized = true;
	}

	public static void SetStatus (string status = null)
	{
		_splashScreenVm.Status = status;
	}

	public static void Show (string status = null)
	{
		if (!_isInitialized)
		{
			Init();
		}

		_splashScreenVm.Status = status;

		if (_splashWindow.IsVisible)
			{
			return;
		}

		_splashWindow.Show();
	}

	public static void Close ()
	{
		_splashWindow.Close();
	}
}
