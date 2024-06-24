using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;

namespace SAM.API;

public abstract class NativeWrapper<TNativeFunctions> : INativeWrapper
{
    private readonly Dictionary<nint, Delegate> functionCache = [ ];
    protected TNativeFunctions _functions;
    protected nint _objectAddress;

    public void SetupFunctions(nint objectAddress)
    {
        _objectAddress = objectAddress;

        if (Marshal.PtrToStructure(_objectAddress, typeof(NativeClass)) is not NativeClass iface)
        {
            throw new InvalidOperationException($"Failed to map {nameof(_objectAddress)} to {nameof(NativeClass)}.");
        }

        _functions = (TNativeFunctions) Marshal.PtrToStructure(iface.VirtualTable, typeof(TNativeFunctions));
    }

    public override string ToString()
    {
        return string.Format(CultureInfo.CurrentCulture,
                             "Steam Interface<{0}> #{1:X8}",
                             typeof(TNativeFunctions),
                             _objectAddress.ToInt32());
    }

    protected Delegate GetDelegate<TDelegate>(nint pointer)
    {
        Delegate function;

        if (functionCache.TryGetValue(pointer, out var value) == false)
        {
            function = Marshal.GetDelegateForFunctionPointer(pointer, typeof(TDelegate));
            functionCache[pointer] = function;
        }
        else
        {
            function = value;
        }

        return function;
    }

    protected TDelegate GetFunction<TDelegate>(nint pointer)
        where TDelegate : class
    {
        return (TDelegate)(object)GetDelegate<TDelegate>(pointer);
    }

    protected void Call<TDelegate>(nint pointer, params object[] args)
    {
        GetDelegate<TDelegate>(pointer).DynamicInvoke(args);
    }

    protected TReturn Call<TReturn, TDelegate>(nint pointer, params object[] args)
    {
        return (TReturn)GetDelegate<TDelegate>(pointer).DynamicInvoke(args);
    }
}
