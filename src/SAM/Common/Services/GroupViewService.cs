using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using DevExpress.Mvvm;
using DevExpress.Mvvm.UI;
using SAM.Core.Extensions;

namespace SAM.Services;

public interface IGroupViewService
{
    void ExpandItem(int index);
    void Expand(object param);
    void InvertAll();
    void ExpandAll();
    void CollapseAll();
}

public class GroupViewService : ServiceBase, IGroupViewService
{
    private ItemsControl Control => AssociatedObject as ItemsControl;
    
    private static readonly DependencyPropertyKey ExpandCommandPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ExpandCommand), typeof(ICommand), typeof(GroupViewService), new (default(ICommand)));

    private static readonly DependencyPropertyKey ExpandAllCommandPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ExpandAllCommand), typeof(ICommand), typeof(GroupViewService), new (default(ICommand)));

    private static readonly DependencyPropertyKey CollapseAllCommandPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(CollapseAllCommand), typeof(ICommand), typeof(GroupViewService), new (default(ICommand)));

    private static readonly DependencyPropertyKey InvertAllCommandPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(InvertAllCommand), typeof(ICommand), typeof(GroupViewService), new (default(ICommand)));
    
    public static readonly DependencyProperty ExpandCommandProperty = ExpandCommandPropertyKey.DependencyProperty;
    public static readonly DependencyProperty ExpandAllCommandProperty = ExpandAllCommandPropertyKey.DependencyProperty;
    public static readonly DependencyProperty CollapseAllCommandProperty = CollapseAllCommandPropertyKey.DependencyProperty;
    public static readonly DependencyProperty InvertAllCommandProperty = InvertAllCommandPropertyKey.DependencyProperty;
    
    public ICommand ExpandCommand
    {
        get { return (ICommand) GetValue(ExpandCommandProperty); }
        protected set { SetValue(ExpandCommandPropertyKey, value); }
    }

    public ICommand ExpandAllCommand
    {
        get { return (ICommand) GetValue(ExpandAllCommandProperty); }
        protected set { SetValue(ExpandAllCommandPropertyKey, value); }
    }
    
    public ICommand CollapseAllCommand
    {
        get { return (ICommand) GetValue(CollapseAllCommandProperty); }
        protected set { SetValue(CollapseAllCommandPropertyKey, value); }
    }
    
    public ICommand InvertAllCommand
    {
        get { return (ICommand) GetValue(InvertAllCommandProperty); }
        protected set { SetValue(InvertAllCommandPropertyKey, value); }
    }

    public GroupViewService()
    {
        ExpandCommand = new DelegateCommand<object>(Expand);
        ExpandAllCommand = new DelegateCommand(ExpandAll);
        CollapseAllCommand = new DelegateCommand(CollapseAll);
        InvertAllCommand = new DelegateCommand(InvertAll);
    }

    public void ExpandItem(int index)
    {
        if (Control == null) return;

        var expanders = DependencyObjectHelper.GetVisualChildren<Expander>(Control);
        if (index >= expanders.Count) throw new ArgumentOutOfRangeException(nameof(index));

        var expander = expanders[index];

        expander.IsExpanded = true;
    }

    public void Expand(object param)
    {
        if (Control == null) return;

        var expanders = DependencyObjectHelper.GetVisualChildren<Expander>(Control);

        Expander group = null;

        if (param is string name)
        {
            group = expanders.FirstOrDefault((e) =>
            {
                if (e.DataContext is CollectionViewGroup context)
                {
                    return context.Name.Equals(name);
                }

                var header = e.Header;

                if (header is string sHeader)
                {
                    return name.EqualsIgnoreCase(sHeader);
                }

                return header.ToString().EqualsIgnoreCase(name);
            });
        }
        else if (param is Expander expander)
        {
            group = expander;
        }

        if (group == null) return;

        group.IsExpanded = true;
    }

    public void InvertAll()
    {
        if (Control == null) return;

        var expanders = DependencyObjectHelper.GetVisualChildren<Expander>(Control);

        foreach (var expander in expanders)
        {
            expander.IsExpanded = !expander.IsExpanded;
        }
    }

    public void ExpandAll()
    {
        if (Control == null) return;

        var expanders = DependencyObjectHelper.GetVisualChildren<Expander>(Control);

        foreach (var expander in expanders)
        {
            expander.IsExpanded = true;
        }
    }

    public void CollapseAll()
    {
        if (Control == null) return;

        var expanders = DependencyObjectHelper.GetVisualChildren<Expander>(Control);

        foreach (var expander in expanders)
        {
            expander.IsExpanded = false;
        }
    }
}
