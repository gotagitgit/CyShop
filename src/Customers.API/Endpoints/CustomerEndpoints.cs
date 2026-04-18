using System.Security.Claims;
using Customers.Application.Interfaces;

namespace Customers.API.Endpoints;

public static class CustomerEndpoints
{
    public static WebApplication MapCustomerEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/customers").RequireAuthorization();

        group.MapGet("/addresses", async (ClaimsPrincipal user, ICustomerAddressService addressService, CancellationToken ct) =>
        {
            var email = user.FindFirstValue(ClaimTypes.Email)
                        ?? user.FindFirstValue("email");

            if (string.IsNullOrEmpty(email))
            {
                return Results.Ok(Array.Empty<object>());
            }

            var addresses = await addressService.GetAddressesByEmailAsync(email, ct);
            return Results.Ok(addresses);
        });

        return app;
    }
}
