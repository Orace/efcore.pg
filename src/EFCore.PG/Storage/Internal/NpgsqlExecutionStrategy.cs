﻿namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

public class NpgsqlExecutionStrategy : IExecutionStrategy
{
    private ExecutionStrategyDependencies Dependencies { get; }

    public NpgsqlExecutionStrategy(ExecutionStrategyDependencies dependencies)
        => Dependencies = dependencies;

    public virtual bool RetriesOnFailure => false;

    public virtual TResult Execute<TState, TResult>(
        TState state,
        Func<DbContext, TState, TResult> operation,
        Func<DbContext, TState, ExecutionResult<TResult>>? verifySucceeded)
    {
        try
        {
            return operation(Dependencies.CurrentContext.Context, state);
        }
        catch (Exception ex) when (ExecutionStrategy.CallOnWrappedException(ex, NpgsqlTransientExceptionDetector.ShouldRetryOn))
        {
            throw new InvalidOperationException("An exception has been raised that is likely due to a transient failure.", ex);
        }
    }

    public virtual async Task<TResult> ExecuteAsync<TState, TResult>(
        TState state,
        Func<DbContext, TState, CancellationToken, Task<TResult>> operation,
        Func<DbContext, TState, CancellationToken, Task<ExecutionResult<TResult>>>? verifySucceeded,
        CancellationToken cancellationToken)
    {
        try
        {
            return await operation(Dependencies.CurrentContext.Context, state, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ExecutionStrategy.CallOnWrappedException(ex, NpgsqlTransientExceptionDetector.ShouldRetryOn))
        {
            throw new InvalidOperationException("An exception has been raised that is likely due to a transient failure.", ex);
        }
    }
}