using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using log4net.Core;

namespace SAM.WPF.Core.Logging
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class FormattedDebugAppender : log4net.Appender.DebugAppender
    {
        protected override void Append(LoggingEvent loggingEvent)
        {
            var message = RenderLoggingEvent(loggingEvent);
            if (string.IsNullOrWhiteSpace(message)) return;

            Debug.Write(message);

            if (!ImmediateFlush) return;

            Debug.Flush();
        }
    }
}
