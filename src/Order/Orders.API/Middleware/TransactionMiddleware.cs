using Microsoft.EntityFrameworkCore;
using Orders.Infrastructure.Data;

namespace Orders.API.Middleware;

public class TransactionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TransactionMiddleware> _logger;

    public TransactionMiddleware(RequestDelegate next, ILogger<TransactionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, OrdersDbContext dbContext)
    {
        if (HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method))
        {
            await _next(context);
            return;
        }

        var endpoint = context.GetEndpoint();
        var options = endpoint?.Metadata.GetMetadata<TransactionOptionsAttribute>();

        if (options?.Skip == true)
        {
            await _next(context);
            return;
        }

        var bufferResponse = options?.BufferResponse ?? true;
        var strategy = dbContext.Database.CreateExecutionStrategy();
        var originalBody = context.Response.Body;

        await strategy.ExecuteAsync(async () =>
        {
            MemoryStream? bufferStream = null;

            if (bufferResponse)
            {
                bufferStream = new MemoryStream();
                context.Response.Body = bufferStream;
            }

            await using var transaction = await dbContext.Database.BeginTransactionAsync();

            using (_logger.BeginScope(new Dictionary<string, object> { ["TransactionId"] = transaction.TransactionId }))
            {
                _logger.LogInformation("Transaction started for {Method} {Path}",
                    context.Request.Method, context.Request.Path);

                try
                {
                    await _next(context);

                    if (context.Response.StatusCode >= 200 && context.Response.StatusCode <= 299)
                    {
                        await transaction.CommitAsync();
                        _logger.LogInformation("Transaction committed for {Method} {Path}",
                            context.Request.Method, context.Request.Path);
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                        _logger.LogInformation("Transaction rolled back for {Method} {Path}",
                            context.Request.Method, context.Request.Path);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Transaction error for {Method} {Path}",
                        context.Request.Method, context.Request.Path);
                    throw;
                }
            }

            if (bufferStream is not null)
            {
                bufferStream.Position = 0;
                await bufferStream.CopyToAsync(originalBody);
                context.Response.Body = originalBody;
            }
        });
    }
}
