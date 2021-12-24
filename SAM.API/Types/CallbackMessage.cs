using System;
using System.Runtime.InteropServices;

namespace SAM.API.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CallbackMessage
    {
        public int User;
        public int Id;
        public IntPtr ParamPointer;
        public int ParamSize;
    }
}
