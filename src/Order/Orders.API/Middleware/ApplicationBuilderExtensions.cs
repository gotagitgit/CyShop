namespace Orders.API.Middleware;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseTransactionMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TransactionMiddleware>();
    }
}
