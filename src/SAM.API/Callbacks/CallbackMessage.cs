using System.Runtime.InteropServices;

namespace SAM.API;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public record struct CallbackMessage (int User, int Id, nint ParamPointer, int ParamSize);
