using System;

namespace SAM.WPF.Core
{
    [Serializable]
    public class SAMException : Exception
    {

        public SAMException()
        {

        }

        public SAMException(string message, Exception innerException = null)
            : base(message, innerException)
        {

        }

    }
}
