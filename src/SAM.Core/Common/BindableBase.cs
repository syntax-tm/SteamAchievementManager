using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SAM.Core;

internal abstract class BindableBase : INotifyPropertyChanged
{
    public event Action StateChanged;
    public event PropertyChangedEventHandler PropertyChanged;

    private readonly object _syncLock = new ();
    private Dictionary<string, object> _propertyBag;
    protected Dictionary<string, object> PropertyBag => _propertyBag ??= [];

#region GetProperty

    protected T GetProperty<T>([CallerMemberName] string name = "")
    {
        return GetPropertyCore<T>(name);
    }

    protected T GetProperty<T>(Expression<Func<T>> expression)
    {
        return GetPropertyCore<T>(GetPropertyName(expression));
    }

    private T GetPropertyCore<T>(string propertyName)
    {
        return PropertyBag.TryGetValue(propertyName, out var val) ? (T) val : default;
    }

#endregion

#region SetProperty

    protected bool SetPropertyCore<T>(string propertyName, T value, Action changedCallback)
    {
        var result = SetPropertyCore(propertyName, value, out var oldValue);
        if (result)
        {
            changedCallback?.Invoke();
        }

        return result;
    }
    protected bool SetPropertyCore<T>(string propertyName, T value, Action<T> changedCallback)
    {
        var result = SetPropertyCore(propertyName, value, out var oldValue);
        if (result)
        {
            changedCallback?.Invoke(oldValue);
        }

        return result;
    }
    protected virtual bool SetPropertyCore<T>(string propertyName, T value, out T oldValue)
    {
        oldValue = default;

        if (PropertyBag.TryGetValue(propertyName, out var val))
        {
            oldValue = (T) val;
        }

        if (CompareValues(oldValue, value))
        {
            return false;
        }

        lock (_syncLock)
        {
            PropertyBag[propertyName] = value;
        }

        OnPropertyChanged(propertyName);
        return true;
    }

    protected bool SetProperty<T>(Expression<Func<T>> expression, T value)
    {
        return SetProperty(expression, value, (Action) null);
    }

    protected bool SetProperty<T>(Expression<Func<T>> expression, T value, Action callback)
    {
        var propertyName = GetPropertyName(expression);
        return SetPropertyCore(propertyName, value, callback);
    }

    protected bool SetProperty<T>(Expression<Func<T>> expression, T value, Action<T> callback)
    {
        var propertyName = GetPropertyName(expression);
        return SetPropertyCore(propertyName, value, callback);
    }

    protected virtual bool SetProperty<T>(ref T storage, T value, string propertyName, Action callback)
    {
        if (CompareValues(storage, value))
        {
            return false;
        }

        storage = value;
        OnPropertyChanged(propertyName);
        callback?.Invoke();

        return true;
    }

    protected bool SetProperty<T>(ref T storage, T value, string propertyName)
    {
        return SetProperty(ref storage, value, propertyName, null);
    }

#endregion

    protected static bool CompareValues<T>(T storage, T value)
    {
        return EqualityComparer<T>.Default.Equals(storage, value);
    }

    public static string GetPropertyName<T>(Expression<Func<T>> expression)
    {
        return GetPropertyNameFast(expression);
    }

    protected static string GetPropertyNameFast(LambdaExpression expression)
    {
        if (expression.Body is not MemberExpression memberExpression)
        {
            throw new ArgumentException($"A {nameof(MemberExpression)} is expected in {nameof(expression.Body)}.", nameof(expression));
        }

        const string VB_LOCAL_PREFIX = "$VB$Local_";
        var member = memberExpression.Member;

        if (member.MemberType == MemberTypes.Field && member.Name.StartsWith(VB_LOCAL_PREFIX))
        {
            return member.Name[VB_LOCAL_PREFIX.Length..];
        }

        return member.Name;
    }

    private void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new (propertyName));

        NotifyStateChanged();
    }
}
