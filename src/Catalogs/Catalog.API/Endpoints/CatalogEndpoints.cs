using Catalog.Application.DTOs;
using Catalog.Application.Interfaces;

namespace Catalog.API.Endpoints;

public static class CatalogEndpoints
{
    public static WebApplication MapCatalogEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/catalog");

        group.MapGet("/", async (ICatalogService catalogService, CancellationToken ct) =>
        {
            var items = await catalogService.GetAllItemsAsync(ct);
            return Results.Ok(items);
        });

        group.MapGet("/{id:guid}", async (Guid id, ICatalogService catalogService, CancellationToken ct) =>
        {
            var item = await catalogService.GetItemByIdAsync(id, ct);
            return item is not null ? Results.Ok(item) : Results.NotFound();
        });

        group.MapGet("/type/{typeId:guid}", async (Guid typeId, ICatalogService catalogService, CancellationToken ct) =>
        {
            var items = await catalogService.GetItemsByTypeAsync(typeId, ct);
            return Results.Ok(items);
        });

        group.MapGet("/brand/{brandId:guid}", async (Guid brandId, ICatalogService catalogService, CancellationToken ct) =>
        {
            var items = await catalogService.GetItemsByBrandAsync(brandId, ct);
            return Results.Ok(items);
        });

        group.MapGet("/pic/{id:guid}", async (Guid id, ICatalogService catalogService, CancellationToken ct) =>
        {
            var stream = await catalogService.GetItemImageAsync(id, ct);
            if (stream is null)
                return Results.NotFound();

            return Results.File(stream, "image/webp");
        });

        group.MapPost("/", async (CreateCatalogItemDto dto, ICatalogService catalogService, CancellationToken ct) =>
        {
            var created = await catalogService.CreateAsync(dto, ct);
            return Results.Created($"/api/catalog/{created.Id}", created);
        }).RequireAuthorization();

        group.MapPut("/{id:guid}", async (Guid id, UpdateCatalogItemDto dto, ICatalogService catalogService, CancellationToken ct) =>
        {
            var updated = await catalogService.UpdateAsync(id, dto, ct);
            return updated is not null ? Results.Ok(updated) : Results.NotFound();
        }).RequireAuthorization();

        group.MapDelete("/{id:guid}", async (Guid id, ICatalogService catalogService, CancellationToken ct) =>
        {
            var deleted = await catalogService.DeleteAsync(id, ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        }).RequireAuthorization();

        return app;
    }
}
