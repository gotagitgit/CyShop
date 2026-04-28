using Basket.Application.DTOs;
using Basket.Application.Interfaces;

namespace Basket.API.Endpoints;

public static class BasketEndpoints
{
    public static WebApplication MapBasketEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/basket").RequireAuthorization();

        group.MapGet("/", async (IBasketService basketService, CancellationToken ct) =>
        {
            var basket = await basketService.GetBasketAsync(ct);
            return Results.Ok(basket);
        });

        group.MapPost("/", async (UpdateBasketDto dto, IBasketService basketService, CancellationToken ct) =>
        {
            var updated = await basketService.UpdateBasketAsync(dto, ct);
            return Results.Ok(updated);
        });

        group.MapDelete("/", async (IBasketService basketService, CancellationToken ct) =>
        {
            await basketService.DeleteBasketAsync(ct);
            return Results.Ok();
        });

        return app;
    }
}
