using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using Orders.API.Middleware;
using Orders.Infrastructure.Data;

namespace Orders.API.Tests.Helpers;

/// <summary>
/// Provides pre-configured mocks and helpers for testing TransactionMiddleware.
/// </summary>
public class MiddlewareTestHelper
{
    public Mock<OrdersDbContext> MockDbContext { get; }
    public Mock<DatabaseFacade> MockDatabase { get; }
    public Mock<IDbContextTransaction> MockTransaction { get; }
    public Mock<ILogger<Middleware.TransactionMiddleware>> MockLogger { get; }
    public FakeExecutionStrategy Strategy { get; }

    public MiddlewareTestHelper()
    {
        // Create DbContextOptions for the mock OrdersDbContext constructor
        var options = new DbContextOptionsBuilder<OrdersDbContext>().Options;

        MockDbContext = new Mock<OrdersDbContext>(options) { CallBase = false };
        MockDatabase = new Mock<DatabaseFacade>(MockDbContext.Object);
        MockTransaction = new Mock<IDbContextTransaction>();
        Strategy = new FakeExecutionStrategy();
        MockLogger = new Mock<ILogger<Middleware.TransactionMiddleware>>();

        // Wire up: DbContext.Database returns MockDatabase
        MockDbContext.Setup(c => c.Database).Returns(MockDatabase.Object);

        // Wire up: Database.CreateExecutionStrategy() returns the fake strategy
        MockDatabase.Setup(d => d.CreateExecutionStrategy()).Returns(Strategy);

        // Wire up: Database.BeginTransactionAsync() returns MockTransaction
        MockDatabase
            .Setup(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(MockTransaction.Object);

        // Wire up: Transaction.TransactionId returns a stable Guid
        MockTransaction.Setup(t => t.TransactionId).Returns(Guid.NewGuid());
    }

    /// <summary>
    /// Creates an HttpContext with the specified HTTP method, path, and a configurable response status code.
    /// The next delegate sets the response status code to <paramref name="responseStatusCode"/>.
    /// </summary>
    public static HttpContext CreateHttpContext(string method, string path, int responseStatusCode = 200)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.Path = path;
        context.Response.StatusCode = responseStatusCode;
        return context;
    }

    /// <summary>
    /// Creates an HttpContext with endpoint metadata attached (e.g., TransactionOptionsAttribute).
    /// </summary>
    public static HttpContext CreateHttpContextWithEndpoint(string method, string path, int responseStatusCode = 200, params object[] metadata)
    {
        var context = CreateHttpContext(method, path, responseStatusCode);
        var endpoint = new Endpoint(_ => Task.CompletedTask, new EndpointMetadataCollection(metadata), "TestEndpoint");
        context.SetEndpoint(endpoint);
        return context;
    }

    /// <summary>
    /// Creates a RequestDelegate that sets the response status code and optionally writes a body.
    /// </summary>
    public static RequestDelegate CreateNextDelegate(int statusCode, string? responseBody = null)
    {
        return async ctx =>
        {
            ctx.Response.StatusCode = statusCode;
            if (responseBody is not null)
            {
                await ctx.Response.WriteAsync(responseBody);
            }
        };
    }

    /// <summary>
    /// Creates a RequestDelegate that throws the specified exception.
    /// </summary>
    public static RequestDelegate CreateThrowingDelegate(Exception exception)
    {
        return _ => throw exception;
    }
}
