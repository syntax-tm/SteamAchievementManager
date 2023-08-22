using System;
using System.Runtime.InteropServices;

namespace SAM.API
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CallbackMessage
    {
        public readonly int User;
        public readonly int Id;
        public readonly nint ParamPointer;
        public readonly int ParamSize;
    }
}
