using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Orders.API.Tests.Helpers;

/// <summary>
/// A fake IExecutionStrategy that simply executes the operation delegate without retry logic.
/// This avoids Moq generic overload matching issues with the ExecuteAsync extension methods.
/// </summary>
public class FakeExecutionStrategy : IExecutionStrategy
{
    public bool RetriesOnFailure => false;

    public TResult Execute<TState, TResult>(
        TState state,
        Func<DbContext, TState, TResult> operation,
        Func<DbContext, TState, ExecutionResult<TResult>>? verifySucceeded)
    {
        // Use a dummy DbContext — the middleware doesn't use the DbContext parameter from the strategy
        return operation(null!, state);
    }

    public async Task<TResult> ExecuteAsync<TState, TResult>(
        TState state,
        Func<DbContext, TState, CancellationToken, Task<TResult>> operation,
        Func<DbContext, TState, CancellationToken, Task<ExecutionResult<TResult>>>? verifySucceeded,
        CancellationToken cancellationToken = default)
    {
        return await operation(null!, state, cancellationToken);
    }
}
