using System;
using System.Runtime.InteropServices;

namespace SAM.API;

[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
internal struct NativeClass
{
	public nint VirtualTable;
}
