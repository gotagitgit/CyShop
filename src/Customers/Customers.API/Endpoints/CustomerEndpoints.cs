using System.Security.Claims;
using Customers.Application.DTOs;
using Customers.Application.Interfaces;
using CyShop.ServiceDefaults;

namespace Customers.API.Endpoints;

public static class CustomerEndpoints
{
    public static WebApplication MapCustomerEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/customers").RequireAuthorization();

        group.MapGet("/addresses", async (HttpContext httpContext, ClaimsPrincipal user, ICustomerAddressService addressService, CancellationToken ct) =>
        {
            var externalId = user.ResolveExternalId(httpContext);

            if (externalId is null)
            {
                return Results.Ok(Array.Empty<object>());
            }

            var addresses = await addressService.GetAddressesByExternalIdAsync(externalId.Value, ct);
            return Results.Ok(addresses);
        });

        group.MapGet("/profile", async (HttpContext httpContext, ClaimsPrincipal user, ICustomerService customerService, CancellationToken ct) =>
        {
            var externalId = user.ResolveExternalId(httpContext);

            if (externalId is null)
            {
                return Results.NotFound();
            }

            var customer = await customerService.GetByExternalIdAsync(externalId.Value, ct);

            if (customer is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(customer);
        });

        group.MapPost("/profile", async (HttpContext httpContext, ClaimsPrincipal user, CreateCustomerDto dto, ICustomerService customerService, CancellationToken ct) =>
        {
            var externalId = user.ResolveExternalId(httpContext);

            if (externalId is null)
            {
                return Results.NotFound();
            }

            try
            {
                var result = await customerService.CreateAsync(externalId.Value, dto, ct);
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

        group.MapPut("/profile", async (HttpContext httpContext, ClaimsPrincipal user, UpdateCustomerDto dto, ICustomerService customerService, CancellationToken ct) =>
        {
            var externalId = user.ResolveExternalId(httpContext);

            if (externalId is null)
            {
                return Results.NotFound();
            }

            try
            {
                var result = await customerService.UpdateAsync(externalId.Value, dto, ct);
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

        group.MapDelete("/profile", async (HttpContext httpContext, ClaimsPrincipal user, ICustomerService customerService, CancellationToken ct) =>
        {
            var externalId = user.ResolveExternalId(httpContext);

            if (externalId is null)
            {
                return Results.NotFound();
            }

            var deleted = await customerService.DeleteAsync(externalId.Value, ct);

            if (!deleted)
            {
                return Results.NotFound();
            }

            return Results.NoContent();
        });

        group.MapPost("/addresses", async (HttpContext httpContext, ClaimsPrincipal user, CreateAddressDto dto, ICustomerAddressService addressService, CancellationToken ct) =>
        {
            var externalId = user.ResolveExternalId(httpContext);

            if (externalId is null)
            {
                return Results.NotFound();
            }

            try
            {
                var result = await addressService.CreateAsync(externalId.Value, dto, ct);
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

        group.MapPut("/addresses/{addressId}", async (Guid addressId, HttpContext httpContext, ClaimsPrincipal user, UpdateAddressDto dto, ICustomerAddressService addressService, CancellationToken ct) =>
        {
            var externalId = user.ResolveExternalId(httpContext);

            if (externalId is null)
            {
                return Results.NotFound();
            }

            try
            {
                var result = await addressService.UpdateAsync(externalId.Value, addressId, dto, ct);
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

        group.MapDelete("/addresses/{addressId}", async (Guid addressId, HttpContext httpContext, ClaimsPrincipal user, ICustomerAddressService addressService, CancellationToken ct) =>
        {
            var externalId = user.ResolveExternalId(httpContext);

            if (externalId is null)
            {
                return Results.NotFound();
            }

            try
            {
                var deleted = await addressService.DeleteAsync(externalId.Value, addressId, ct);

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
