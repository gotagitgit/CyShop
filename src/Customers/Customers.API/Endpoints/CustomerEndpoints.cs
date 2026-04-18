using System.Security.Claims;
using Customers.Application.DTOs;
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

        group.MapGet("/profile", async (ClaimsPrincipal user, ICustomerService customerService, CancellationToken ct) =>
        {
            var sub = user.FindFirstValue("sub");

            if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var externalId))
            {
                return Results.NotFound();
            }

            var customer = await customerService.GetByExternalIdAsync(externalId, ct);

            if (customer is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(customer);
        });

        group.MapPost("/profile", async (ClaimsPrincipal user, CreateCustomerDto dto, ICustomerService customerService, CancellationToken ct) =>
        {
            var sub = user.FindFirstValue("sub");

            if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var externalId))
            {
                return Results.NotFound();
            }

            try
            {
                var result = await customerService.CreateAsync(externalId, dto, ct);
                return Results.Created($"/api/customers/profile", result);
            }
            catch (InvalidOperationException)
            {
                return Results.Conflict();
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { errors = new { message = new[] { ex.Message } } });
            }
        });

        group.MapPut("/profile", async (ClaimsPrincipal user, UpdateCustomerDto dto, ICustomerService customerService, CancellationToken ct) =>
        {
            var sub = user.FindFirstValue("sub");

            if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var externalId))
            {
                return Results.NotFound();
            }

            try
            {
                var result = await customerService.UpdateAsync(externalId, dto, ct);
                return Results.Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { errors = new { message = new[] { ex.Message } } });
            }
        });

        group.MapDelete("/profile", async (ClaimsPrincipal user, ICustomerService customerService, CancellationToken ct) =>
        {
            var sub = user.FindFirstValue("sub");

            if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var externalId))
            {
                return Results.NotFound();
            }

            var deleted = await customerService.DeleteAsync(externalId, ct);

            if (!deleted)
            {
                return Results.NotFound();
            }

            return Results.NoContent();
        });

        group.MapPost("/addresses", async (ClaimsPrincipal user, CreateAddressDto dto, ICustomerAddressService addressService, CancellationToken ct) =>
        {
            var sub = user.FindFirstValue("sub");

            if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var externalId))
            {
                return Results.NotFound();
            }

            try
            {
                var result = await addressService.CreateAsync(externalId, dto, ct);
                return Results.Created($"/api/customers/addresses/{result.Id}", result);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { errors = new { message = new[] { ex.Message } } });
            }
        });

        group.MapPut("/addresses/{addressId}", async (Guid addressId, ClaimsPrincipal user, UpdateAddressDto dto, ICustomerAddressService addressService, CancellationToken ct) =>
        {
            var sub = user.FindFirstValue("sub");

            if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var externalId))
            {
                return Results.NotFound();
            }

            try
            {
                var result = await addressService.UpdateAsync(externalId, addressId, dto, ct);
                return Results.Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { errors = new { message = new[] { ex.Message } } });
            }
        });

        group.MapDelete("/addresses/{addressId}", async (Guid addressId, ClaimsPrincipal user, ICustomerAddressService addressService, CancellationToken ct) =>
        {
            var sub = user.FindFirstValue("sub");

            if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var externalId))
            {
                return Results.NotFound();
            }

            try
            {
                var deleted = await addressService.DeleteAsync(externalId, addressId, ct);

                if (!deleted)
                {
                    return Results.NotFound();
                }

                return Results.NoContent();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        });

        return app;
    }
}
