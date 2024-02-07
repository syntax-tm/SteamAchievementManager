using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;

namespace SAM.API
{
	public abstract class NativeWrapper<TNativeFunctions> : INativeWrapper
	{
		private readonly Dictionary<nint, Delegate> _FunctionCache = [];
		protected TNativeFunctions Functions;
		protected nint ObjectAddress;

		public void SetupFunctions (nint objectAddress)
		{
			ObjectAddress = objectAddress;

			var iface = (NativeClass) Marshal.PtrToStructure(
				ObjectAddress,
				typeof(NativeClass));

			Functions = (TNativeFunctions) Marshal.PtrToStructure(
				iface.VirtualTable,
				typeof(TNativeFunctions));
		}

		public override string ToString ()
		{
			return string.Format(
				CultureInfo.CurrentCulture,
				"Steam Interface<{0}> #{1:X8}",
				typeof(TNativeFunctions),
				ObjectAddress.ToInt32());
		}

		protected Delegate GetDelegate<TDelegate> (nint pointer)
		{
			Delegate function;

			if (_FunctionCache.ContainsKey(pointer) == false)
			{
				function = Marshal.GetDelegateForFunctionPointer(pointer, typeof(TDelegate));
				_FunctionCache [pointer] = function;
			}
			else
			{
				function = _FunctionCache [pointer];
			}

			return function;
		}

		protected TDelegate GetFunction<TDelegate> (nint pointer)
			where TDelegate : class
		{
			return (TDelegate) (object) GetDelegate<TDelegate>(pointer);
		}

		protected void Call<TDelegate> (nint pointer, params object [] args)
		{
			GetDelegate<TDelegate>(pointer).DynamicInvoke(args);
		}

		protected TReturn Call<TReturn, TDelegate> (nint pointer, params object [] args)
		{
			return (TReturn) GetDelegate<TDelegate>(pointer).DynamicInvoke(args);
		}
	}
}
