using System;
using System.Runtime.InteropServices;

namespace SAM.API
{
    public abstract class Callback : ICallback
    {
        public delegate void CallbackFunction(IntPtr param);

        public abstract int Id { get; }
        public abstract bool IsServer { get; }

        public void Run(IntPtr param)
        {
            OnRun(param);
        }

        public event CallbackFunction OnRun;
    }

    public abstract class Callback<TParameter> : ICallback
        where TParameter : struct
    {
        public delegate void CallbackFunction(TParameter arg);

        public abstract int Id { get; }
        public abstract bool IsServer { get; }

        public void Run(IntPtr pvParam)
        {
            var data = (TParameter)Marshal.PtrToStructure(pvParam, typeof(TParameter));
            OnRun(data);
        }

        public event CallbackFunction OnRun;
    }
}
