using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace SAM.API
{
	internal static class NativeStrings
	{
		public static unsafe StringHandle StringToStringHandle (string value)
		{
			if (value == null)
				return new(nint.Zero, true);

			var bytes = Encoding.UTF8.GetBytes(value);
			var length = bytes.Length;

			var p = Marshal.AllocHGlobal(length + 1);
			Marshal.Copy(bytes, 0, p, bytes.Length);

			((byte*) p)! [length] = 0;

			return new(p, true);
		}

		public static unsafe string PointerToString (sbyte* bytes)
		{
			if (bytes == null)
				return null;

			var running = 0;

			var b = bytes;
			if (*b == 0)
				return string.Empty;

			while (*b++ != 0)
			{
				running++;
			}

			return new(bytes, 0, running, Encoding.UTF8);
		}

		public static unsafe string PointerToString (byte* bytes)
		{
			return PointerToString((sbyte*) bytes);
		}

		public static unsafe string PointerToString (nint nativeData)
		{
			return PointerToString((sbyte*) nativeData.ToPointer());
		}

		public static unsafe string PointerToString (sbyte* bytes, int length)
		{
			if (bytes == null)
				return null;

			var running = 0;

			var b = bytes;
			if (length == 0 || *b == 0)
				return string.Empty;

			while (*b++ != 0 &&
				running < length)
			{
				running++;
			}

			return new(bytes, 0, running, Encoding.UTF8);
		}

		public static unsafe string PointerToString (byte* bytes, int length)
		{
			return PointerToString((sbyte*) bytes, length);
		}

		public static unsafe string PointerToString (nint nativeData, int length)
		{
			return PointerToString((sbyte*) nativeData.ToPointer(), length);
		}

		public sealed class StringHandle : SafeHandleZeroOrMinusOneIsInvalid
		{
			public nint Handle => handle;

			internal StringHandle (nint preexistingHandle, bool ownsHandle)
				: base(ownsHandle)
			{
				SetHandle(preexistingHandle);
			}

			protected override bool ReleaseHandle ()
			{
				if (handle == nint.Zero)
					return false;

				Marshal.FreeHGlobal(handle);
				handle = nint.Zero;
				return true;
			}
		}
	}
}
