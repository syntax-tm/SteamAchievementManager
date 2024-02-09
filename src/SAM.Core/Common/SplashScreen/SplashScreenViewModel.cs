using System;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;

namespace SAM.Core;

public class SplashScreenViewModel
{
	public virtual ICurrentWindowService CurrentWindow
	{
		get;
	}

	public virtual bool ShowStatus
	{
		get; set;
	}
	public virtual string Status
	{
		get; set;
	}
	public virtual bool ShowProgress
	{
		get; set;
	}
	public virtual decimal PercentComplete
	{
		get; set;
	}

	protected SplashScreenViewModel ()
	{

	}

	public static SplashScreenViewModel Create ()
	{
		return ViewModelSource.Create(() => new SplashScreenViewModel());
	}

	public void Show ()
	{
		CurrentWindow.Show();
	}

	public void Hide ()
	{
		CurrentWindow.Hide();
	}

	public static void Exit ()
	{
		Environment.Exit(0);
	}

	protected void OnStatusChanged ()
	{
		ShowStatus = !string.IsNullOrWhiteSpace(Status);
	}
}
