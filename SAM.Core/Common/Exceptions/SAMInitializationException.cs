using System;

namespace SAM.Core;

[Serializable]
public class SAMInitializationException : SAMException
{
    public SAMInitializationException()
    {

    }

    public SAMInitializationException(string message, Exception innerException = null)
        : base(message, innerException)
    {

    }
}
