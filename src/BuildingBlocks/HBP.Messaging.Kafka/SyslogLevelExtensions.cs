using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace HBP.Messaging.Kafka;

internal static class SyslogLevelExtensions
{
    public static LogLevel ToMsLogLevel(this SyslogLevel level)
    {
        return level switch
        {
            SyslogLevel.Emergency or SyslogLevel.Alert or SyslogLevel.Critical => LogLevel.Critical,
            SyslogLevel.Error => LogLevel.Error,
            SyslogLevel.Warning => LogLevel.Warning,
            SyslogLevel.Debug => LogLevel.Debug,
            _ => LogLevel.Information,
        };
    }
}
