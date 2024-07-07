using System;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;

namespace SAM.WaitForm;

public class WaitFormViewModel
{
    public virtual ICurrentWindowService CurrentWindow { get; }

    public virtual bool ShowStatus { get; set; }
    public virtual string Status { get; set; }
    public virtual bool ShowProgress { get; set; }
    public virtual decimal PercentComplete { get; set; }
        
    protected WaitFormViewModel()
    {

    }

    public static WaitFormViewModel Create()
    {
        return ViewModelSource.Create(() => new WaitFormViewModel());
    }

    public void Show()
    {
        CurrentWindow.Show();
    }

    public void Hide()
    {
        CurrentWindow.Hide();
    }
        
    public void Exit()
    {
        Environment.Exit(0);
    }

    protected void OnStatusChanged()
    {
        ShowStatus = !string.IsNullOrWhiteSpace(Status);
    }
}
