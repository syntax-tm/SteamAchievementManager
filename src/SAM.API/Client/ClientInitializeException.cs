namespace SAM.API;

public class ClientInitializeException : Exception
{
	public readonly ClientInitFailure Failure;

	public ClientInitializeException (ClientInitFailure failure)
		: base(failure.GetDescription())
	{
		Failure = failure;
	}

	public ClientInitializeException (ClientInitFailure failure, string message)
		: base(message)
	{
		Failure = failure;
	}

	public ClientInitializeException (ClientInitFailure failure, string message, Exception innerException)
		: base(message, innerException)
	{
		Failure = failure;
	}

	public ClientInitializeException ()
	{
	}

	public ClientInitializeException (string message) : base(message)
	{
	}

	public ClientInitializeException (string message, Exception innerException) : base(message, innerException)
	{
	}
}