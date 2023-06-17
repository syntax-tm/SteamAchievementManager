using System;
using System.Runtime.InteropServices;

namespace SAM.API
{
    public abstract class Callback : ICallback
    {
        public abstract int Id { get; init; }
        public abstract bool IsServer { get; init; }
        public event Action<nint>? OnRun;
        public virtual void Run(nint param) => OnRun?.Invoke(param);
    }

    public abstract class Callback<TParameter> : Callback
        where TParameter : struct
    {
        private event Action<TParameter>? _onRun;
        public event Action<TParameter>? OnRun
        {
            add => _onRun += value;
            remove => _onRun -= value;
        }

        public void Run(nint param)
        {
            TParameter? data = Marshal.PtrToStructure<TParameter>(param);
            if (data.HasValue)
            {
                Run(data.Value);
            }
            else
            {
                throw new ArgumentException($"Failed to marshal the parameter to {nameof(TParameter)}.");
            }
        }

        protected virtual void Run(TParameter data)
        {
            _onRun?.Invoke(data);
        }
    }
}
