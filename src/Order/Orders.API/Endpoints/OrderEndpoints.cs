using Orders.Application.DTOs;
using Orders.Application.Interfaces;

namespace Orders.API.Endpoints;

public static class OrderEndpoints
{
    public static WebApplication MapOrderEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/orders").RequireAuthorization();

        group.MapPost("/", async (
            HttpContext httpContext,
            CreateOrderDto request,
            IOrderService orderService,
            IIdempotencyService idempotencyService,
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

            // Check idempotency
            if (await idempotencyService.IsDuplicateAsync(idempotencyKey, ct))
            {
                return Results.Ok();
            }

            await orderService.CreateOrderAsync(request, ct);

            // Save idempotency record
            await idempotencyService.RecordAsync(idempotencyKey, ct);

            return Results.Created();
        });

        group.MapGet("/", async (
            IOrderService orderService,
            CancellationToken ct) =>
        {
            var orders = await orderService.GetOrdersByCustomerAsync(ct);
            return Results.Ok(orders);
        });

        group.MapGet("/{id:guid}", async (
            Guid id,
            IOrderService orderService,
            CancellationToken ct) =>
        {
            var order = await orderService.GetOrderByIdAsync(id, ct);
            if (order is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(order);
        });

        return app;
    }
}
