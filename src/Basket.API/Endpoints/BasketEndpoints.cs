using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Basket.API.Models;
using Basket.API.Repositories;

namespace Basket.API.Endpoints;

public static class BasketEndpoints
{
    public static WebApplication MapBasketEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/basket").RequireAuthorization();

        group.MapGet("/", async (ClaimsPrincipal user, IBasketRepository repository) =>
        {
            var buyerId = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
                          ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(buyerId))
            {
                return Results.Ok(new CustomerBasket());
            }

            var basket = await repository.GetBasketAsync(buyerId);
            return Results.Ok(basket ?? new CustomerBasket { BuyerId = buyerId });
        }).AllowAnonymous();

        group.MapPost("/", async (ClaimsPrincipal user, CustomerBasket basket, IBasketRepository repository) =>
        {
            var buyerId = (user.FindFirstValue(JwtRegisteredClaimNames.Sub)
                          ?? user.FindFirstValue(ClaimTypes.NameIdentifier))!;

            if (basket.Items.Any(item => item.Quantity < 1))
            {
                return Results.BadRequest("All item quantities must be at least 1.");
            }

            var basketToSave = basket with { BuyerId = buyerId };
            var updated = await repository.UpdateBasketAsync(basketToSave);

            return Results.Ok(updated);
        });

        group.MapDelete("/", async (ClaimsPrincipal user, IBasketRepository repository) =>
        {
            var buyerId = (user.FindFirstValue(JwtRegisteredClaimNames.Sub)
                          ?? user.FindFirstValue(ClaimTypes.NameIdentifier))!;
            await repository.DeleteBasketAsync(buyerId);
            return Results.Ok();
        });

        return app;
    }
}
