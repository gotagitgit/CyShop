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
            var sub = user.FindFirstValue("sub");

            if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var externalId))
            {
                return Results.Ok(Array.Empty<object>());
            }

            var addresses = await addressService.GetAddressesByExternalIdAsync(externalId, ct);
            return Results.Ok(addresses);
        });

        return app;
    }
}
