using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CareerHub.Api.Infrastructure;

public class SlowQueryInterceptor : DbCommandInterceptor
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SlowQueryInterceptor> _logger;

    public SlowQueryInterceptor(IConfiguration configuration, ILogger<SlowQueryInterceptor> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public override DbDataReader ReaderExecuted(
        DbCommand command, 
        CommandExecutedEventData eventData, 
        DbDataReader result)
    {
        LogSlowQuery(command, eventData);
        return base.ReaderExecuted(command, eventData, result);
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command, 
        CommandExecutedEventData eventData, 
        DbDataReader result, 
        CancellationToken cancellationToken = default)
    {
        LogSlowQuery(command, eventData);
        return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    private void LogSlowQuery(DbCommand command, CommandExecutedEventData eventData)
    {
        // Fallback to 100ms if setting is missing in appsettings.json
        var threshold = _configuration.GetValue<int?>("SlowQueryThresholdMs") ?? 100; 

        if (eventData.Duration.TotalMilliseconds > threshold)
        {
            _logger.LogWarning("⚠️ SLOW QUERY DETECTED ({Duration}ms):\n{CommandText}", 
                eventData.Duration.TotalMilliseconds, 
                command.CommandText);
        }
    }
}