using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DevExpress.Mvvm;
using Wpf.Ui.Controls;
using Brush = System.Drawing.Brush;
using Button = System.Windows.Controls.Button;
using Image = System.Windows.Controls.Image;

namespace SAM.Controls;

[TemplatePart(Name = "PART_RootGrid", Type = typeof(Grid))]
[TemplatePart(Name = "PART_Border", Type = typeof(Border))]
[TemplatePart(Name = "PART_Button", Type = typeof(Button))]
[TemplatePart(Name = "PART_FavoriteButton", Type = typeof(Button))]
[TemplatePart(Name = "PART_HideButton", Type = typeof(Button))]
[TemplatePart(Name = "PART_Icon", Type = typeof(Button))]
[TemplatePart(Name = "PART_ButtonPanel", Type = typeof(StackPanel))]
[TemplatePart(Name = "PART_Image", Type = typeof(Image))]
[TemplatePart(Name = "PART_Header", Type = typeof(Viewbox))]
[TemplatePart(Name = "PART_MediaElement", Type = typeof(MediaElement))]
public class AppTileButton : Button
{

//#region Command

//    public static readonly DependencyProperty CommandProperty =
//        DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(AppTileButton), new (default(ICommand)));

//    public ICommand Command
//    {
//        get { return (ICommand) GetValue(CommandProperty); }
//        set { SetValue(CommandProperty, value); }
//    }

//#endregion

#region IsFavorite

    public static readonly DependencyProperty IsFavoriteProperty =
        DependencyProperty.Register(nameof(IsFavorite), typeof(bool), typeof(AppTileButton),
                                    new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsFavoriteChanged));

    public bool IsFavorite
    {
        get { return (bool) GetValue(IsFavoriteProperty); }
        set { SetValue(IsFavoriteProperty, value); }
    }
    
    private static void OnIsFavoriteChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not AppTileButton button) return;

        button.RefreshIcon();
    }

#endregion

#region IsHidden

    public static readonly DependencyProperty IsHiddenProperty =
        DependencyProperty.Register(nameof(IsHidden), typeof(bool), typeof(AppTileButton),
                                    new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsHiddenChanged));

    public bool IsHidden
    {
        get { return (bool) GetValue(IsHiddenProperty); }
        set { SetValue(IsHiddenProperty, value); }
    }

    private static void OnIsHiddenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not AppTileButton button) return;

        button.RefreshIcon();
    }
    
#endregion
    
#region IsSelected

    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(AppTileButton), new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));    

    public bool IsSelected
    {
        get { return (bool) GetValue(IsSelectedProperty); }
        set { SetValue(IsSelectedProperty, value); }
    }

#endregion

#region Icon

    private static readonly DependencyPropertyKey IconPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(Icon), typeof(SymbolRegular), typeof(AppTileButton), new (SymbolRegular.Empty));

    public static readonly DependencyProperty IconProperty = IconPropertyKey.DependencyProperty;

    public SymbolRegular Icon
    {
        get { return (SymbolRegular) GetValue(IconProperty); }
        protected set { SetValue(IconPropertyKey, value); }
    }

#endregion

#region IconBrush

    private static readonly DependencyPropertyKey IconBrushPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IconBrush), typeof(Brush), typeof(AppTileButton), new (default(SolidColorBrush)));

    public static readonly DependencyProperty IconBrushProperty = IconBrushPropertyKey.DependencyProperty;

    public Brush IconBrush
    {
        get { return (Brush) GetValue(IconBrushProperty); }
        protected set { SetValue(IconBrushPropertyKey, value); }
    }

#endregion

#region Header

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(string), typeof(AppTileButton), new PropertyMetadata(default(string)));

    public string Header
    {
        get { return (string) GetValue(HeaderProperty); }
        set { SetValue(HeaderProperty, value); }
    }

#endregion
    
#region IsAnimatedImage
    
    public static readonly DependencyProperty IsAnimatedImageProperty =
        DependencyProperty.Register(nameof(IsAnimatedImage), typeof(bool), typeof(AppTileButton),
                                    new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    public bool IsAnimatedImage
    {
        get { return (bool) GetValue(IsAnimatedImageProperty); }
        set { SetValue(IsAnimatedImageProperty, value); }
    }

#endregion

#region ImageSource
    
    public static readonly DependencyProperty ImageSourceProperty =
        DependencyProperty.Register(nameof(ImageSource), typeof(Uri), typeof(AppTileButton),
                                    new FrameworkPropertyMetadata(default(Uri), FrameworkPropertyMetadataOptions.AffectsRender, OnImageSourceChanged));

    public Uri ImageSource
    {
        get { return (Uri) GetValue(ImageSourceProperty); }
        set { SetValue(ImageSourceProperty, value); }
    }

#endregion

#region ShowImage

    private static readonly DependencyPropertyKey ShowImagePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ShowImage), typeof(bool), typeof(AppTileButton), new (false));

    public static readonly DependencyProperty ShowImageProperty = ShowImagePropertyKey.DependencyProperty;

    public bool ShowImage
    {
        get { return (bool) GetValue(ShowImageProperty); }
        protected set { SetValue(ShowImagePropertyKey, value); }
    }

#endregion

#region ToggleVisibilityCommand

    private static readonly DependencyPropertyKey toggleVisibilityCommandPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ToggleVisibilityCommand), typeof(ICommand), typeof(AppTileButton), new (null));

    public static readonly DependencyProperty ToggleVisibilityCommandProperty = toggleVisibilityCommandPropertyKey.DependencyProperty;

    public ICommand ToggleVisibilityCommand
    {
        get { return (ICommand) GetValue(ToggleVisibilityCommandProperty); }
        protected set { SetValue(toggleVisibilityCommandPropertyKey, value); }
    }

#endregion
    
#region ToggleFavoriteCommand

    private static readonly DependencyPropertyKey toggleFavoriteCommandPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ToggleFavoriteCommand), typeof(ICommand), typeof(AppTileButton), new (null));

    public static readonly DependencyProperty ToggleFavoriteCommandProperty = toggleFavoriteCommandPropertyKey.DependencyProperty;

    public ICommand ToggleFavoriteCommand
    {
        get { return (ICommand) GetValue(ToggleFavoriteCommandProperty); }
        protected set { SetValue(toggleFavoriteCommandPropertyKey, value); }
    }

#endregion

#region ContextMenuOpen
    
    public static readonly DependencyProperty ContextMenuOpenProperty =
        DependencyProperty.Register(nameof(ContextMenuOpen), typeof(bool), typeof(AppTileButton),
                                    new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    public bool ContextMenuOpen
    {
        get { return (bool) GetValue(ContextMenuOpenProperty); }
        set { SetValue(ContextMenuOpenProperty, value); }
    }

#endregion
    
    protected Grid PART_RootGrid;
    protected Border PART_Border;
    protected Button PART_Button;
    protected Button PART_FavoriteButton;
    protected Button PART_HideButton;
    protected Button PART_Icon;
    protected StackPanel PART_ButtonPanel;
    protected Image PART_Image;
    protected Viewbox PART_Header;
    protected MediaElement PART_MediaElement;

    public AppTileButton()
    {
        ToggleFavoriteCommand = new DelegateCommand(ToggleFavorite);
        ToggleVisibilityCommand = new DelegateCommand(ToggleHide);
    }

    public override void OnApplyTemplate()
    {
        ApplyTemplate();

        PART_RootGrid = GetTemplateChild("PART_RootGrid") as Grid;
        PART_Border = GetTemplateChild("PART_Border") as Border;
        PART_Button = GetTemplateChild("PART_Button") as Button;
        PART_FavoriteButton = GetTemplateChild("PART_FavoriteButton") as Button;
        PART_HideButton = GetTemplateChild("PART_HideButton") as Button;
        PART_Icon = GetTemplateChild("PART_Icon") as Button;
        PART_ButtonPanel = GetTemplateChild("PART_ButtonPanel") as StackPanel;
        PART_Image = GetTemplateChild("PART_Image") as Image;
        PART_Header = GetTemplateChild("PART_Header") as Viewbox;
        PART_MediaElement = GetTemplateChild("PART_MediaElement") as MediaElement;

        if (PART_FavoriteButton != null)
            PART_FavoriteButton.Click += OnToggleFavorited;
        
        if (PART_HideButton != null)
            PART_HideButton.Click += OnToggleHidden;
    }

    protected override void OnTemplateChanged(ControlTemplate oldTemplate, ControlTemplate newTemplate)
    {
        base.OnTemplateChanged(oldTemplate, newTemplate);

        if (PART_FavoriteButton != null)
            PART_FavoriteButton.Click -= OnToggleFavorited;

        if (PART_HideButton != null)
            PART_HideButton.Click -= OnToggleHidden;
    }

    protected override void OnContextMenuOpening(ContextMenuEventArgs e)
    {
        base.OnContextMenuOpening(e);

        ContextMenuOpen = true;
    }

    protected override void OnContextMenuClosing(ContextMenuEventArgs e)
    {
        base.OnContextMenuClosing(e);

        ContextMenuOpen = false;
    }

    private void OnToggleFavorited(object sender, RoutedEventArgs args)
    {
        ToggleFavorite();
    }

    private void ToggleFavorite()
    {
        IsFavorite = !IsFavorite;
    }
    
    private void OnToggleHidden(object sender, RoutedEventArgs args)
    {
        ToggleHide();
    }

    private void ToggleHide()
    {
        IsHidden = !IsHidden;
    }

    private void RefreshIcon()
    {
        if (IsHidden)
        {
            Icon = SymbolRegular.EyeOff24;
        }
        else if (IsFavorite)
        {
            Icon = SymbolRegular.Heart24;
        }
    }

    private static void OnImageSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not AppTileButton button) return;

        button.OnImageSourceChanged(e);
    }

    private void OnImageSourceChanged(DependencyPropertyChangedEventArgs e)
    {
        if (ImageSource == null) ShowImage = false;
        else if (string.IsNullOrWhiteSpace(ImageSource?.ToString())) ShowImage = false;
        else ShowImage = true;
    }

}
