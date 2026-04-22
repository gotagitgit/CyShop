using System.Security.Claims;
using CyShop.ServiceDefaults;
using Orders.API.Middleware;
using Orders.Application.DTOs;
using Orders.Application.Interfaces;
using Orders.Infrastructure.Data;
using Orders.Infrastructure.Data.Entities;

namespace Orders.API.Endpoints;

public static class OrderEndpoints
{
    public static WebApplication MapOrderEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/orders").RequireAuthorization();

        group.MapPost("/", async (
            HttpContext httpContext,
            ClaimsPrincipal user,
            CreateOrderDto request,
            IOrderService orderService,
            OrdersDbContext dbContext,
            CancellationToken ct) =>
        {
            // Validate Idempotency-Key header
            if (!httpContext.Request.Headers.TryGetValue("Idempotency-Key", out var idempotencyHeader)
                || !Guid.TryParse(idempotencyHeader.FirstOrDefault(), out var idempotencyKey))
            {
                return Results.BadRequest(new { error = "Idempotency-Key header is required" });
            }

            // Validate customerName
            if (string.IsNullOrWhiteSpace(request.CustomerName))
            {
                return Results.BadRequest(new { error = "customerName is required" });
            }

            // Validate items
            if (request.Items is null || request.Items.Count == 0)
            {
                return Results.BadRequest(new { error = "At least one order item is required" });
            }

            // Resolve customer identity
            var customerId = user.ResolveExternalId(httpContext);
            if (customerId is null)
            {
                return Results.Unauthorized();
            }

            // Check idempotency
            if (await IdempotencyMiddleware.IsDuplicateAsync(idempotencyKey, dbContext))
            {
                return Results.Ok();
            }

            await orderService.CreateOrderAsync(customerId.Value, request, ct);

            // Save idempotency record
            dbContext.IdempotencyRecords.Add(new IdempotencyRecord
            {
                IdempotencyKey = idempotencyKey,
                Timestamp = DateTime.UtcNow
            });
            await dbContext.SaveChangesAsync(ct);

            return Results.Created();
        });

        group.MapGet("/", async (
            HttpContext httpContext,
            ClaimsPrincipal user,
            IOrderService orderService,
            CancellationToken ct) =>
        {
            var customerId = user.ResolveExternalId(httpContext);
            if (customerId is null)
            {
                return Results.Unauthorized();
            }

            var orders = await orderService.GetOrdersByCustomerAsync(customerId.Value, ct);
            return Results.Ok(orders);
        });

        group.MapGet("/{id:guid}", async (
            Guid id,
            HttpContext httpContext,
            ClaimsPrincipal user,
            IOrderService orderService,
            CancellationToken ct) =>
        {
            var customerId = user.ResolveExternalId(httpContext);
            if (customerId is null)
            {
                return Results.Unauthorized();
            }

            var order = await orderService.GetOrderByIdAsync(id, customerId.Value, ct);
            if (order is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(order);
        });

        return app;
    }
}
