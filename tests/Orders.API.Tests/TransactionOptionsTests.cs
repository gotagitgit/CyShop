using Microsoft.AspNetCore.Http;
using Moq;
using Orders.API.Middleware;
using Orders.API.Tests.Helpers;
using Xunit;

namespace Orders.API.Tests;

public class TransactionOptionsTests
{
    [Fact]
    public async Task SkipTransaction_SkipsTransactionAndBuffering()
    {
        // Arrange
        var helper = new MiddlewareTestHelper();
        var context = MiddlewareTestHelper.CreateHttpContextWithEndpoint(
            "POST", "/api/orders",
            metadata: new TransactionOptionsAttribute { Skip = true });

        var nextCalled = false;
        var next = new RequestDelegate(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        var middleware = new TransactionMiddleware(next, helper.MockLogger.Object);

        // Act
        await middleware.InvokeAsync(context, helper.MockDbContext.Object);

        // Assert
        Assert.True(nextCalled);
        helper.MockDatabase.Verify(
            d => d.CreateExecutionStrategy(), Times.Never);
        helper.MockDatabase.Verify(
            d => d.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task BufferResponseFalse_StillWrapsInTransaction_ButDoesNotBuffer()
    {
        // Arrange
        var helper = new MiddlewareTestHelper();
        var context = MiddlewareTestHelper.CreateHttpContextWithEndpoint(
            "POST", "/api/orders/bulk-export",
            metadata: new TransactionOptionsAttribute { BufferResponse = false });

        var originalBody = context.Response.Body;
        var bodyDuringPipeline = (Stream?)null;

        var next = new RequestDelegate(ctx =>
        {
            bodyDuringPipeline = ctx.Response.Body;
            ctx.Response.StatusCode = 200;
            return Task.CompletedTask;
        });

        var middleware = new TransactionMiddleware(next, helper.MockLogger.Object);

        // Act
        await middleware.InvokeAsync(context, helper.MockDbContext.Object);

        // Assert — response body was NOT replaced with a MemoryStream
        Assert.Same(originalBody, bodyDuringPipeline);

        // Assert — transaction was still used and committed
        helper.MockDatabase.Verify(
            d => d.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        helper.MockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BufferResponseTrue_BuffersResponse()
    {
        // Arrange
        var helper = new MiddlewareTestHelper();
        var context = MiddlewareTestHelper.CreateHttpContextWithEndpoint(
            "POST", "/api/orders",
            metadata: new TransactionOptionsAttribute { BufferResponse = true });

        var bodyDuringPipeline = (Stream?)null;

        var next = new RequestDelegate(ctx =>
        {
            bodyDuringPipeline = ctx.Response.Body;
            ctx.Response.StatusCode = 200;
            return Task.CompletedTask;
        });

        var middleware = new TransactionMiddleware(next, helper.MockLogger.Object);

        // Act
        await middleware.InvokeAsync(context, helper.MockDbContext.Object);

        // Assert — response body WAS replaced with a MemoryStream during pipeline
        Assert.IsType<MemoryStream>(bodyDuringPipeline);

        helper.MockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task NoAttribute_DefaultsToBufferedTransaction()
    {
        // Arrange
        var helper = new MiddlewareTestHelper();
        var context = MiddlewareTestHelper.CreateHttpContext("POST", "/api/orders");

        var bodyDuringPipeline = (Stream?)null;

        var next = new RequestDelegate(ctx =>
        {
            bodyDuringPipeline = ctx.Response.Body;
            ctx.Response.StatusCode = 200;
            return Task.CompletedTask;
        });

        var middleware = new TransactionMiddleware(next, helper.MockLogger.Object);

        // Act
        await middleware.InvokeAsync(context, helper.MockDbContext.Object);

        // Assert — default behavior: buffered + transaction
        Assert.IsType<MemoryStream>(bodyDuringPipeline);
        helper.MockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BufferResponseFalse_NonSuccessStatus_RollsBack()
    {
        // Arrange
        var helper = new MiddlewareTestHelper();
        var context = MiddlewareTestHelper.CreateHttpContextWithEndpoint(
            "PUT", "/api/orders/1",
            metadata: new TransactionOptionsAttribute { BufferResponse = false });

        var next = new RequestDelegate(ctx =>
        {
            ctx.Response.StatusCode = 400;
            return Task.CompletedTask;
        });

        var middleware = new TransactionMiddleware(next, helper.MockLogger.Object);

        // Act
        await middleware.InvokeAsync(context, helper.MockDbContext.Object);

        // Assert — transaction rolled back, no buffering
        helper.MockTransaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        helper.MockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
