using System;

namespace SAM.API
{
    public class ClientInitializeException : Exception
    {
        public readonly ClientInitializeFailure Failure;

        public ClientInitializeException(ClientInitializeFailure failure)
        {
            Failure = failure;
        }

        public ClientInitializeException(ClientInitializeFailure failure, string message)
            : base(message)
        {
            Failure = failure;
        }

        public ClientInitializeException(ClientInitializeFailure failure, string message, Exception innerException)
            : base(message, innerException)
        {
            Failure = failure;
        }

        public ClientInitializeException()
        {
        }

        public ClientInitializeException(string message) : base(message)
        {
        }

        public ClientInitializeException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}