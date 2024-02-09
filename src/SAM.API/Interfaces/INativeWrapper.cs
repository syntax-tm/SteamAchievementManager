using System;

namespace SAM.API;

public interface INativeWrapper
{
	void SetupFunctions (nint objectAddress);
}
