using Microsoft.EntityFrameworkCore.Diagnostics;
using Polly.Retry;
using System.Data.Common;

public class ResilientDbCommandInterceptor : DbCommandInterceptor
{
    private readonly AsyncRetryPolicy _retryPolicy;

    public ResilientDbCommandInterceptor(AsyncRetryPolicy retryPolicy)
    {
        _retryPolicy = retryPolicy;
    }

    public override async ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            return await base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
        });
    }

    public override async ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result,
        CancellationToken cancellationToken = default)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            return await base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
        });
    }
}
