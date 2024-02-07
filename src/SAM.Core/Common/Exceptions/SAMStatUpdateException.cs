using System;

namespace SAM.Core
{
	[Serializable]
	public class SAMStatUpdateException : SAMException
	{
		public SAMStatUpdateException ()
		{

		}

		public SAMStatUpdateException (string message, Exception innerException = null)
			: base(message, innerException)
		{

		}
	}
}
