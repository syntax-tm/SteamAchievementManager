using System.Runtime.Serialization;

namespace SAM.API.Stats;

[Serializable]
public class StatIsProtectedException : Exception
{
	public StatIsProtectedException ()
	{
	}

	public StatIsProtectedException (string message)
		: base(message)
	{
	}

	public StatIsProtectedException (string message, Exception innerException)
		: base(message, innerException)
	{
	}

	// This API supports obsolete formatter-based serialization. It should not be called or extended by application code
	protected StatIsProtectedException (SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
