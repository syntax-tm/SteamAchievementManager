using System.Diagnostics;
using log4net.Core;

namespace SAM.Core.Logging
{
	public class FormattedDebugAppender : log4net.Appender.DebugAppender
	{
		protected override void Append (LoggingEvent loggingEvent)
		{
			var message = RenderLoggingEvent(loggingEvent);
			if (string.IsNullOrWhiteSpace(message))
				return;

			Debug.Write(message);

			if (!ImmediateFlush)
				return;

			Debug.Flush();
		}
	}
}
