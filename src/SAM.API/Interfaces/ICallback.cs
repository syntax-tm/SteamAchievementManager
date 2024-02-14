namespace SAM.API;

public interface ICallback
{
	int Id
	{
		get;
	}
	bool IsServer
	{
		get;
	}

	void Run (nint param);
}
