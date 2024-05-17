using System;
using DevExpress.Mvvm.CodeGenerators;
using SAM.Core.Behaviors;

namespace SAM.Core.ViewModels;

[GenerateViewModel]
public partial class MainWindowViewModelBase
{
    private const string TITLE_BASE = "Steam Achievement Manager";

    [GenerateProperty] private string title = TITLE_BASE;
    [GenerateProperty] private string subTitle;
    [GenerateProperty] private WindowSettings config;

    protected MainWindowViewModelBase()
    {

    }

    [GenerateCommand]
    public void Exit()
    {
        Environment.Exit(0);
    }

    [GenerateCommand]
    protected void OnLoaded()
    {
        SplashScreenHelper.Close();
    }
    
    private void OnSubTitleChanged()
    {
        if (string.IsNullOrWhiteSpace(SubTitle))
        {
            Title = TITLE_BASE;
            return;
        }

        Title = $"{TITLE_BASE} | {SubTitle}";
    }
}
